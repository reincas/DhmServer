##########################################################################
# Copyright (c) 2024 Reinhard Caspary                                    #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################

import logging
from pathlib import Path
from shutil import rmtree
import numpy as np
import cv2 as cv
import matplotlib.pyplot as plt


# Standard logging formatter
LOGFMT = logging.Formatter(fmt="%(asctime)s / %(levelname)s / %(message)s",
                           datefmt="%Y-%m-%d %H:%M:%S")

# BGR colors
CV_WHITE = (255, 255, 255)
CV_BLUE = (255, 0, 0)
CV_GREEN = (0, 255, 0)
CV_RED = (0, 0, 255)


def mkdir(path):

    """ Make sure that the given folder exists and is empty. """

    p = Path(path)
    p.mkdir(parents=True, exist_ok=True)
    for sub in p.iterdir():
        if sub.is_file():
            sub.unlink()
        elif sub.is_dir():
            rmtree(sub)
    return path


def normcolor(img, cmap=None):

    """ Normalize given floating point image and convert it to 8-bit BGR
    image. """

    if cmap == None:
        cmap = "viridis"
    cmap = plt.get_cmap(cmap)
    img = cv.normalize(img, None, 0.0, 1.0, cv.NORM_MINMAX, cv.CV_64F)
    img = cmap(img)[:,:,:3]
    img *= 255
    img = img.astype(np.uint8)
    img = cv.cvtColor(img, cv.COLOR_RGB2BGR)
    return img


def colorize(img, cmap=None):

    if cmap == None:
        cmap = "viridis"
    cmap = plt.get_cmap(cmap)
    img = cmap(img)[:,:,:3]
    img *= 255
    img = img.astype(np.uint8)
    return img


def drawCross(img, x, y, s, color, thickness=1, center=True):

    if center:
        h, w, _ = img.shape
        x = round(x + w/2)
        y = round(y + h/2)
    img = cv.line(img, (x-s//2, y), (x+s//2, y), color, thickness)
    img = cv.line(img, (x, y-s//2), (x, y+s//2), color, thickness)
    return img
  

def drawCircle(img, x, y, r, color, thickness=1, center=True):

    if center:
        h, w, _ = img.shape
        x = round(x + w/2)
        y = round(y + h/2)
    r = round(r)
    img = cv.circle(img, (x, y), r, color, thickness)
    return img


def addBorder(img, width, value=0):
    
    """ Add a frame with given width and value to the given image. """
    
    if len(img.shape) == 3:
        h, w, d = img.shape
        result = np.ones((h+2*width, w+2*width, d), dtype=img.dtype) * value
        result[width:width+h,width:width+w,:] = img
    else:
        h, w = img.shape
        result = np.ones((h+2*width, w+2*width), dtype=img.dtype) * value
        result[width:width+h,width:width+w] = img
    return result


def contrast(img):

    """ Return image contrast based on the relative 5% quantile range of
    its pixel values. The return value is between 0.0 and 1.0. """
    
    vlo, vhi = np.quantile(img, [0.05, 0.95], method="nearest")
    return float(vhi-vlo) / 255


##########################################################################
if __name__ == "__main__":

    pass    
