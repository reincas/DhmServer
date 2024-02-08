##########################################################################
# Copyright (c) 2024 Reinhard Caspary                                    #
# <reinhard.caspary@phoenixd.uni-hannover.de>                            #
# This program is free software under the terms of the MIT license.      #
##########################################################################

import socket
import struct
import numpy as np

from .commands import *

# Info:
#   CameraShutter = (CameraShutterUs - 30) / 20
#   CameraShutterUs = CameraShutter * 20 + 30
# OPL search with Koala:
#   FullSearchStep: 250 µm
#   LocalSearchStep: 15 µm
#   LocalSearchInterval: 750 µm


class DhmClient(object):

    """ Client class to access the remote server HoloServ, which
    provides access to the digital holographic microscope from LynéeTec.
    This class only provides the most basic functionality of data
    transfer in both directions. Higher level methods should be defined
    in classes inheriting DhmClient. """

    manufacturer = "LyncéeTec"
    
    _commands = {
        "ServerVersion": [None, C_GetVersion, "i"],
        "CmdVersion": [None, C_GetCmdVersion, "i"],
        "Status": [None, C_GetDhmStatus, "i"],
        "ConfigList": [None, C_GetConfigList, "c"],
        "Config": [C_SetConfig, C_GetConfig, "i"],
        "DhmSerial": [None, C_GetDhmSerial, "s"],
        "ObjectiveName": [None, C_GetObjectiveName, "s"],
        "ObjectiveDescription": [None, C_GetObjectiveDescription, "s"],
        "ObjectiveMagnification": [None, C_GetObjectiveMagnification, "d"],
        "ObjectiveNumericalAperture": [None, C_GetObjectiveNumericalAperture, "d"],
        "ObjectivePixelSizeXUm": [None, C_GetObjectivePixelSizeXUm, "d"],
        "ObjectivePixelSizeYUm": [None, C_GetObjectivePixelSizeYUm, "d"],
        "CameraSerial": [None, C_GetCameraSerial, "s"],
        "CameraName": [None, C_GetCameraName, "s"],
        "CameraMaxWidth": [None, C_GetCameraMaxWidth, "i"],
        "CameraMaxHeight": [None, C_GetCameraMaxHeight, "i"],
        "CameraWidth": [None, C_GetCameraWidth, "i"],
        "CameraHeight": [None, C_GetCameraHeight, "i"],
        "CameraOffsetX": [None, C_GetCameraOffsetX, "i"],
        "CameraOffsetY": [None, C_GetCameraOffsetY, "i"],
        "CameraBitPerPixel": [C_SetCameraBitPerPixel, C_GetCameraBitPerPixel, "i"],
        "CameraStride": [None, C_GetCameraStride, "i"],
        "CameraPixelSizeUm": [None, C_GetCameraPixelSizeUm, "f"],
        "CameraMinShutter": [None, C_MinCameraShutter, "i"],
        "CameraMaxShutter": [None, C_MaxCameraShutter, "i"],
        "CameraShutter": [C_SetCameraShutter, C_GetCameraShutter, "i"],
        "CameraMinShutterUs": [None, C_MinCameraShutterUs, "f"],
        "CameraMaxShutterUs": [None, C_MaxCameraShutterUs, "f"],
        "CameraShutterUs": [C_SetCameraShutterUs, C_GetCameraShutterUs, "f"],
        "CameraMinGain": [None, C_MinCameraGain, "i"],
        "CameraMaxGain": [None, C_MaxCameraGain, "i"],
        "CameraGain": [C_SetCameraGain, C_GetCameraGain, "i"],
        "CameraMinBrightness": [None, C_MinCameraBrightness, "i"],
        "CameraMaxBrightness": [None, C_MaxCameraBrightness, "i"],
        "CameraBrightness": [C_SetCameraBrightness, C_GetCameraBrightness, "i"],
        "CameraImage": [None, C_GetCameraImage, "h"],
        "LaserWavelength": [None, C_GetLaserWavelength, "d"],
        "LaserOutput": [C_SetLaserOutput, None, "i"],
        "MotorMinCoderPos": [None, C_MinMotorCoderPos, "i"],
        "MotorMaxCoderPos": [None, C_MaxMotorCoderPos, "i"],
        "MotorCoderPos": [None, C_GetMotorCoderPos, "i"],
        "MotorMinPos": [None, C_MinMotorPos, "f"],
        "MotorMaxPos": [None, C_MaxMotorPos, "f"],
        "MotorPos": [C_SetMotorPos, C_GetMotorPos, "f"],
        "MotorUnitPos": [None, C_UnitMotorPos, "s"],
        }

    _functions = {
        "OptCameraImage": [C_GetOptCameraImage, "h", "i"],
        "StartCameraGrabTime": [C_StartCameraGrabTime, "h", "i"],
        }

    def __init__(self, host, port):

        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)  
        self.sock.connect((host, port))

        if self.ServerVersion < 3:
            raise RuntimeError("HoloServ is outdated!")


    def __enter__(self):

        return self


    def __exit__(self, etype, value, traceback):

        data = struct.pack("i", C_Quit)
        self.sock.sendall(data)
        self.sock.close()


    def remoteCmd(self, cmd, outfmt, infmt, *inargs):

        # Normalize format strings
        if outfmt is None:
            outfmt = ""
        if infmt is None:
            infmt = ""

        # Sanity check
        if len(inargs) != len(infmt):
            raise RuntimeError("%d arguments expected!" % len(infmt))

        # Prepare transmission data
        #print(cmd, outfmt, infmt, *inargs)
        fmt = "i"
        values = [cmd]
        for i, code in enumerate(infmt):
            fmt += code
            if code == "i":
                values.append(int(inargs[i]))
            elif code == "f":
                values.append(float(inargs[i]))
            else:
                raise RuntimeError("Unknown input format '%s'!" % code)
        data = struct.pack(fmt, *values)
        #print("".join(["%02x" % x for x in data]))

        # Send transmission data
        self.sock.sendall(data)

        # Receive command code
        ans = struct.unpack("i", self.sock.recv(4))[0]
        if ans != cmd:
            raise RuntimeError("Invalid server response!")

        # No return values
        if not outfmt:
            return

        # Receive result values
        result = []
        for code in outfmt:
            if code == "i":
                value = struct.unpack("i", self.sock.recv(4))[0]
                
            elif code == "f":
                    value = struct.unpack("f", self.sock.recv(4))[0]
                                    
            elif code == "d":
                value = struct.unpack("d", self.sock.recv(8))[0]
                
            elif code == "s":
                size = struct.unpack("i", self.sock.recv(4))[0]
                value = struct.unpack("%ds" % size, self.sock.recv(size))[0]
                value = value.decode("utf8")
                    
            elif code == "c":
                count = struct.unpack("i", self.sock.recv(4))[0]
                value = []
                for i in range(count):
                    cid, size = struct.unpack("ii", self.sock.recv(8))
                    name = struct.unpack("%ds" % size, self.sock.recv(size))[0]
                    name = name.decode("utf8")
                    value.append((cid, name))
                    
            elif code == "h":
                height, width, stride = struct.unpack("iii", self.sock.recv(12))
                size = stride*height
                data = b""
                while len(data) < size:
                    data += self.sock.recv(size - len(data))
                data = struct.unpack("%ds" % size, data)[0]
                if stride // width == 2:
                    value = np.frombuffer(data, np.uint16).reshape((height, width))
                else:
                    value = np.frombuffer(data, np.uint8).reshape((height, width))

            else:
                raise AttributeError("Unknown value type '%s'!" % code)

            result.append(value)

        # Return result values
        if len(result) == 1:
            return result[0]
        return tuple(result)


    def __setattr__(self, name, value):

        if name in self._commands:
            cmd, _, infmt = self._commands[name]
            if cmd is None:
                raise(AttributeError, "Attribute '%s' is not writeable!" % name)

            if isinstance(value, tuple):
                self.remoteCmd(cmd, "", infmt, *value)
            else:
                self.remoteCmd(cmd, "", infmt, value)

        else:
            super().__setattr__(name, value)


    def __getattr__(self, name):

        if name in self._commands:
            _, cmd, outfmt = self._commands[name]
            if cmd is None:
                raise(AttributeError, "Attribute '%s' is not readable!" % name)
            return self.remoteCmd(cmd, outfmt, "")

        elif name in self._functions:
            cmd, outfmt, infmt = self._functions[name]

            def func(*inargs):
                return self.remoteCmd(cmd, outfmt, infmt, *inargs)
            return func

        return super().__getattr__(name)


    def parameters(self):

        # Compile dhm data dictionary
        params = {
            "server": {
                "version": self.ServerVersion,
                "commandVersion": self.CmdVersion,
                },
            "dhm": {
                "serial": self.DhmSerial,
                "configId": self.Config,
                "configName": dict(self.ConfigList)[self.Config],
                },
            "objective": {
                "name": self.ObjectiveName,
                "description": self.ObjectiveDescription,
                "magnification": self.ObjectiveMagnification,
                "numericalAperture": self.ObjectiveNumericalAperture,
                "xPixelSizeUm": self.ObjectivePixelSizeXUm,
                "yPixelSizeUm": self.ObjectivePixelSizeYUm,
                },
            "camera": {
                "serial": self.CameraSerial,
                "name": self.CameraName,
                "maxWidth": self.CameraMaxWidth,
                "maxHeight": self.CameraMaxHeight,
                "width": self.CameraWidth,
                "height": self.CameraHeight,
                "xOffset": self.CameraOffsetX,
                "yOffset": self.CameraOffsetY,
                "bitPerPixel": self.CameraBitPerPixel,
                "stride": self.CameraStride,
                "pixelSizeUm": self.CameraPixelSizeUm,
                "minShutter": self.CameraMinShutter,
                "maxShutter": self.CameraMaxShutter,
                "shutter": self.CameraShutter,
                "minShutterUs": self.CameraMinShutterUs,
                "maxShutterUs": self.CameraMaxShutterUs,
                "shutterUs": self.CameraShutterUs,
                "minGain": self.CameraMinGain,
                "maxGain": self.CameraMaxGain,
                "gain": self.CameraGain,
                "minBrightness": self.CameraMinBrightness,
                "maxBrightness": self.CameraMaxBrightness,
                "brightness": self.CameraBrightness,
                },
            "laser": {
                "wavelengthUm": self.LaserWavelength * 1e6,
                },
            "motor": {
                "minCoderPos": self.MotorMinCoderPos,
                "maxCoderPos": self.MotorMaxCoderPos,
                "coderPos": self.MotorCoderPos,
                "minPos": self.MotorMinPos,
                "maxPos": self.MotorMaxPos,
                "pos": self.MotorPos,
                "unitPos": self.MotorUnitPos,
                }
            }

        # Add manufacturer names
        if "Basler" in params["camera"]["name"]:
            params["camera"]["manufacturer"] = "Basler"
        params["dhm"]["manufacturer"] = self.manufacturer

        # Return dhm data dictionary
        return params



############################################################################
if __name__ == "__main__":

    pass