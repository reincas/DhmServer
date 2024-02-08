##########################################################################
# Copyright (c) 2024 Reinhard Caspary                                    #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################

from holoclient import HoloClient

HOST = "192.168.22.2"
PORT = 27182
AUTHOR = "Reinhard Caspary"
EMAIL = "reinhard.caspary@phoenixd.uni-hannover.de"


with HoloClient(host=HOST, port=PORT, oplmode="both") as client:
    cid = 178
    configs = client.ConfigList
    name = dict(configs)[cid]
    client.Config = cid
    print("Objective: %s [%d]" % (name, cid))

    m = client.motorScan(show=True)
    print("Motor pos: %.1f µm (set: %.1f µm)", (client.MotorPos, m))

    client.getImage(opt=True, show=True)

    dc = client.container(opt=True, show=True, author=AUTHOR, email=EMAIL)
    dc.write("hologram.zdc")
    print(dc)

    shutter = client.CameraShutter
    shutterus = client.CameraShutterUs
    print("Shutter: %.1f us [%d]" % (shutterus, shutter))

    print("Done.")
