##########################################################################
# Copyright (c) 2024 Reinhard Caspary                                    #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################

import random

VERSION = 6

############################################################################
Commands = [
  "Version", "Quit", "Error",
  "GetVersion", "GetCmdVersion",
  "GetDhmStatus",
  "GetConfigList", "GetConfig", "SetConfig",
  "GetDhmSerial",
  "GetObjectiveName", "GetObjectiveDescription",
  "GetObjectiveMagnification", "GetObjectiveNumericalAperture", 
  "GetObjectivePixelSizeXUm", "GetObjectivePixelSizeYUm", 
  "GetCameraSerial", "GetCameraName",
  "GetCameraMaxWidth", "GetCameraMaxHeight",
  "GetCameraWidth", "GetCameraHeight", "GetCameraOffsetX", "GetCameraOffsetY",
  "GetCameraBitPerPixel", "SetCameraBitPerPixel", "GetCameraStride",
  "GetCameraPixelSizeUm",
  "MinCameraShutter", "MaxCameraShutter", "GetCameraShutter", "SetCameraShutter",
  "MinCameraShutterUs", "MaxCameraShutterUs", "GetCameraShutterUs", "SetCameraShutterUs",
  "MinCameraGain", "MaxCameraGain", "GetCameraGain", "SetCameraGain",
  "MinCameraBrightness", "MaxCameraBrightness", "GetCameraBrightness", "SetCameraBrightness",
  "GetCameraImage", "GetOptCameraImage",
  "StartCameraGrabTime",
  "GetLaserWavelength",
  "SetLaserOutput",
  "MinMotorCoderPos", "MaxMotorCoderPos", "GetMotorCoderPos",
  "MinMotorPos", "MaxMotorPos", "GetMotorPos", "SetMotorPos",
  "UnitMotorPos",
  ]

############################################################################
def readCodes(fn, cmds, version):
    
    # Read known codes
    with open(fn, "r") as fp:
        codes = fp.readlines()
    codes = [x.split(",") for x in codes]
    codes = dict([(x.strip(), int(y.strip())) for x,y in codes])
    #print(codes)

    # Delete obsolete commands
    for cmd in set(codes.keys()) - set(cmds):
        del codes[cmd]

    # Add new command codes
    for cmd in cmds:
        if cmd == "Version":
            codes[cmd] = version
        elif cmd not in codes:
            code = codes["Version"]
            while code in codes.values():
                code = random.randint(0x10000, 0x7fffffff)
            codes[cmd] = code

    # Store command codes
    lines = ["%s, %d" % (k, v) for k,v in codes.items()]
    lines = "\n".join(lines)
    with open(fn, "w") as fp:
        fp.writelines(lines)

    # Return new codes dictionary
    return codes


def writeServer(fn, cmds, codes):

    # Get headlines and footlines from current C# file
    with open(fn, "r") as fp:
        lines = fp.readlines()
    head = True
    for i, line in enumerate(lines):
        if head and "public const int %s" % cmds[0] in line:
            space = line[:line.index("public const int")]
            headlines = lines[:i]
            head = False
        if not head and not "public const int" in line:
            footlines = lines[i:]
            break

    # Prepare code lines for C# file
    size = max([len(c) for c in codes.keys()])
    fmt = space + "public const int %%-%ds = 0x%%08x;\n" % size
    codelines = []
    for i, cmd in enumerate(cmds):
        code = codes[cmd]
        codelines.append(fmt % (cmd, code))

    # Store new C# file
    lines = "".join(headlines + codelines + footlines)
    with open(fn, "w") as fp:
        fp.write(lines)
    print("Wrote %s." % fn)


def writeClient(fn, cmds, codes):

    # Prepare code lines for Python file
    size = max([len(c) for c in codes.keys()])
    fmt = "C_%%-%ds = 0x%%08x;" % size
    lines = []
    for i, cmd in enumerate(cmds):
        code = codes[cmd]
        lines.append(fmt % (cmd, code))
    lines = "\n".join(lines)

    # Store new Python file
    with open(fn, "w") as fp:
        fp.write(lines)
    print("Wrote %s." % fn)


############################################################################
codes = readCodes("codes.txt", Commands, VERSION)
writeServer("DhmServ/Commands.cs", Commands, codes)
writeClient("DhmClient/holoclient/commands.py", Commands, codes)
