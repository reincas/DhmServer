##########################################################################
# Copyright (c) 2023-2024 Reinhard Caspary                               #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################
# External command for 3DPoliCompiler
#   GenerateCode2("dhmInit", "178")
#
# Initialize the DHM library
#
# Requires number of objective as parameter:
#   178: 20x Zeiss Femtika
#   179: 20x Nikon Femtika
#   180: 63x Zeiss Femtika
##########################################################################

import sys
from holoclient import HoloClient

# Given configurartion ID
cid = int(sys.argv[1])
print("Config: %d" % cid)

# Initialize DHM library
with HoloClient() as client:
    client.Config = cid

