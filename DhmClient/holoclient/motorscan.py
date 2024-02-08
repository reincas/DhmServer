##########################################################################
# Copyright (c) 2024 Reinhard Caspary                                    #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################

"""
This module contains functions to perform an optical path length (OPL) scan in
order to balance the lengths of the object and reference beams.
"""

import bisect
import numpy as np

import .utils
from .hologram import locateOrder


class Contrast(object):
    
    def __init__(self):
        
        self.position = []
        self.values = []


    def __len__(self):
        
        return len(self.position)
    

    def __iter__(self):
        
        return self.values.__iter__()
    

    def __getitem__(self, pos):

        try:
            i = self.position.index(pos)
        except ValueError:
            return
        return self.values[i]
    
        
    def add(self, pos, value):
    
        """ Add motor position and contrast to the list of datapoints. """

        if pos in self.position:
            i = self.position.index(pos)
            self.position[i] = value
        else:
            i = bisect.bisect_left(self.position, pos)
            self.position.insert(i, pos)
            self.values.insert(i, value)


    @property
    def imax(self):
        
        return np.argmax(self.values)
    
    
    @property
    def minPos(self):
        
        if len(self) == 0:
            return None
        return self.position[0]
    
    
    @property
    def maxPos(self):
        
        if len(self) == 0:
            return None
        return self.position[-1]
    
    
    @property
    def nextPos(self):
        
        imax = self.imax 
        if imax <= 0:
            return -1
        elif imax >= len(self)-1:
            return 1
        return 0


    @property
    def hasTriple(self):
        
        return self.nextPos == 0


    @property
    def triple(self):
        
        if not self.hasTriple:
            raise RuntimeError("No triple!")
        imax = self.imax
        return self.position[imax-1:imax+2]


    @property
    def tripleValues(self):
        
        if not self.hasTriple:
            raise RuntimeError("No triple!")
        imax = self.imax
        return self.values[imax-1:imax+2]


    def tripleThreshold(self, thresh):
        
        return self.hasTriple and min(self.tripleValues) > thresh

        
    def maxThreshold(self, thresh):
        
        return max(self) > thresh
        

def moveMotor(client, m, opt=False):

    """ Move the OPL motor to the given position, take a camera image and
    return the relative weight of its first diffraction order peak. """

    # Move the OPL motor
    client.MotorPos = m

    # Grab camera image
    img = client.getImage(opt)[0]
    if opt:
        client.CameraShutter = client.CameraShutter-2

    # Return weight of diffraction order peak
    return locateOrder(img)[-1]


def addMotor(client, data, m, opt=False, show=False):

    """ Move the OPL motor to the given position, take a camera image and
    return its contrast and the weight of its first diffraction order peak. """

    value = data[m]
    if value != None:
        return

    w = moveMotor(client, m, opt)
    data.add(m, w)
    
    if show:
        print("Motor scan %02d: %.1f µm - %.1f%%" % (len(data), m, 100*w))


def shortScan(client, steps=11, dm=250.0, thresh=0.05, m0=None, opt=True,
              show=False):

    # Minimum and maximum motor position
    minpos = client.MotorMinPos
    maxpos = client.MotorMaxPos

    # Center point of the scan
    if m0 is None:
        m0 = client.MotorPos
    else:
        m0 = float(m0)

    # Determine coarse scan range
    m1 = m0 - dm*(steps-1)/2
    if m1 < minpos:
        m1 = minpos
    m2 = m1 + (steps-1)*dm
    if m2 > maxpos:
        m2 = maxpos
        m1 = m2 - (steps-1)*dm

        # Sanity check: That should never happen.
        if m1 < minpos:
            raise RuntimeError("OPL scan range too small!")

    return runScan(client, m1, m2, dm, thresh, opt, show)


def longScan(client, dm=250.0, thresh=0.05, opt=True, show=False):

    # Minimum and maximum motor position
    m1 = client.MotorMinPos
    m2 = client.MotorMaxPos

    return runScan(client, m1, m2, dm, thresh, opt, show)


def runScan(client, m1, m2, dm=250.0, thresh=0.05, opt=True, show=False):

    # Minimum and maximum motor position
    minpos = client.MotorMinPos
    maxpos = client.MotorMaxPos

    # Perform scan
    data = Contrast()
    i = 0
    while True:

        # Get weight of diffraction peak at current position
        m = min(m2, m1 + i*dm)
        
        if data:
            opt = False
        addMotor(client, data, m, opt, show=show)

        if data[m] is None:
            print(data.position)
            print(m)
        if data[m] > thresh:
            addMotor(client, data, m-0.7*dm, show=show)
            addMotor(client, data, m-0.3*dm, show=show)
            addMotor(client, data, m+0.3*dm, show=show)
            addMotor(client, data, m+0.7*dm, show=show)
            
        # Stop if a increased diffraction peak indicates interference
        if data.tripleThreshold(thresh):
            break

        if m == m2:
            break
        i += 1
        
    # No interference detected
    if not data.maxThreshold(thresh):
        return

    # Make sure that there is at least one data point below the
    # maximum contrast point
    while data.nextPos < 0:
        m = max(minpos, data.minPos - dm)
        if m >= data.minPos:
            return
        addMotor(client, data, m, show=show)

    # Make sure that there is at least one data point above the
    # maximum contrast point
    while data.nextPos > 0:
        m = min(maxpos, data.maxPos + dm)
        if m >= data.maxPos:
            return
        addMotor(client, data, m, show=show)

    # Return motor position with maximum contrast and its two neighbors
    if show:
        print("Scan result: %.1f µm - %.1f%%" % (data.triple[1], max(data)))
    return data.triple


def bisectMax(client, triple, minc=0.005, minm=5.0, opt=True, show=False):

    # Determine image contrast at the initial motor positions.
    # Eventually optimize the exposure time at the center position.
    m0, m1, m2 = triple
    c1 = moveMotor(client, m1, opt)
    c0 = moveMotor(client, m0)
    c2 = moveMotor(client, m2)
    count = 0
    if show:
        print("Bisect %02d: %.1f µm - %.1f%%  ==  %.1f µm - %.1f%%  ==  %.1f µm %.1f%%" % \
              (count, m0, 100*c0, m1, 100*c1, m2, 100*c2))

    # Center point must have maximum contrast
    if c0 > c1 or c2 > c1:
        raise RuntimeError("No contrast maximum!")

    # Bisectioning algorithm to find the motor position with maximum
    # image contrast. Proceed until either the difference of the
    # contrast or the difference of the positions falls below the given
    # limits.
    while max(abs(c1-c0), abs(c1-c2)) > minc and m2-m0 > minm:
        count += 1

        if m1 - m0 > m2 - m1:
            m = (m0 + m1) / 2
            c = moveMotor(client, m)
            #print("lo: %.1f %.2f" % (m, c))
            if (c1 - c) < 0:
                m2, c2 = m1, c1
                m1, c1 = m, c
            else:
                m0, c0 = m, c
        else:
            m = (m1 + m2) / 2
            c = moveMotor(client, m)
            #print("hi: %.1f %.2f" % (m, c))
            if (c - c1) > 0:
                m0, c0 = m1, c1
                m1, c1 = m, c
            else:
                m2, c2 = m, c

        if show:
            print("Bisect %02d: %.1f µm - %.1f%%  ==  %.1f µm - %.1f%%  ==  %.1f µm %.1f%%" % \
                  (count, m0, 100*c0, m1, 100*c1, m2, 100*c2))

    # Return the center of the final range
    m = (m0 + m2) / 2
    if show:
        print("Bisect result: %.1f µm" % m)
    return m


def motorScan(client, mode="both", steps=11, dm=250.0, thresh=0.05,
              init=None, minc=0.005, minm=5.0, opt=True, show=False):

    mode = mode.lower()
    if mode not in ("short", "long", "both"):
        raise RuntimeError("Unknown OPL scan mode!")

    result = None
    if mode in ("short", "both"):
        if init is None:
            init = client.MotorPos
        else:
            init = float(init)
        result = shortScan(client, steps=steps, dm=dm, thresh=thresh,
                           m0=init, opt=opt, show=show)
    else:
        init = None
    if result is None and mode == "short":
        raise RuntimeError("Short OPL scan detected no interference!")
    if result is None:
        result = longScan(client, dm=dm, thresh=thresh, opt=opt, show=show)
    if result is None:
        raise RuntimeError("Long OPL scan detected no interference!")

    m = bisectMax(client, result, minc, minm, opt, show)
    client.MotorPos = m
    return m, init
