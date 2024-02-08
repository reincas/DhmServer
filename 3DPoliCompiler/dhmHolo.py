##########################################################################
# Copyright (c) 2023-2024 Reinhard Caspary                               #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################
# External command for 3DPoliCompiler:
#   GenerateCode2("dhmHolo", "myholo.zdc", "John Doe", "john@home.net")
#
# Read and store current hologram from DHM as SciDataContainer
##########################################################################

import sys
import os.path

from holoclient import HoloClient

deskPath = "C:/Users/Nanofactory/Desktop/"

# Handle spaces in the command line correctly
args = " ".join(sys.argv[1:])
args = args.split("|")

# This external command requires three parameters: filename, author name
# and email. Relative path of filename relates to the desktop.
fn, author, email = args

# Add extension '.zdc' if it is missing
if fn[-4:] != ".zdc":
    fn += ".zdc"

# Add path to desktop
if not os.path.isabs(fn):
    fn = os.path.join(deskPath, fn)

# Normalize path
fn = os.path.normpath(fn)

# Show parameters
print("File: %s" % fn)
print("Author: %s" % author)
print("Email: %s" % email)

# Read hologram with optimized shutter value
with HoloClient() as client:
    dc = client.container(opt=True, show=False, author=author, email=email)
    dc.write(fn)
