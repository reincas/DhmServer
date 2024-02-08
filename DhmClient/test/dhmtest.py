##########################################################################
# Copyright (c) 2024 Reinhard Caspary                                    #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################

import cv2 as cv
from holoclient import DhmClient


with DhmClient() as client:
    
    cid = 178
    configs = client.ConfigList
    name = dict(configs)[cid]
    client.Config = cid
    print("Objective: %s [%d]" % (name, cid))
    print("Motor pos:", client.MotorPos)

    shutter = client.CameraShutter
    shutterus = client.CameraShutterUs
    print("Shutter: %.1f us [%d]" % (shutterus, shutter))

    img = client.CameraImage
    cv.imwrite("hologram.png", img)

    imin = np.min(img)
    imax = np.max(img)
    iavg = np.average(img)
    print("Pixel values: %d - %d (avg: %.1f)" % (imin, imax, iavg))

    print("Done.")


