/*************************************************************************
 * Copyright (c) 2023-2024 Reinhard Caspary                              *
 * <reinhard.caspary@phoenixd.uni-hannover.de>                           *
 * This program is free software under the terms of the MIT license.     *
 *************************************************************************/

using System;

namespace DhmServ
{
    public static class Command
    {
        public const int Version                       = 0x00000006;
        public const int Quit                          = 0x1e769ffe;
        public const int Error                         = 0x4bdca849;
        public const int GetVersion                    = 0x3961955f;
        public const int GetCmdVersion                 = 0x6aaeafb6;
        public const int GetDhmStatus                  = 0x45aa4da5;
        public const int GetConfigList                 = 0x10b2b1b4;
        public const int GetConfig                     = 0x2f5bb0d2;
        public const int SetConfig                     = 0x753eadfa;
        public const int GetDhmSerial                  = 0x4d4e407e;
        public const int GetObjectiveName              = 0x3caec94f;
        public const int GetObjectiveDescription       = 0x50189a53;
        public const int GetObjectiveMagnification     = 0x4a5a5c7e;
        public const int GetObjectiveNumericalAperture = 0x08643fec;
        public const int GetObjectivePixelSizeXUm      = 0x4d85491e;
        public const int GetObjectivePixelSizeYUm      = 0x01b07d12;
        public const int GetCameraSerial               = 0x7455294f;
        public const int GetCameraName                 = 0x6b332fd4;
        public const int GetCameraMaxWidth             = 0x3c10ce2c;
        public const int GetCameraMaxHeight            = 0x21338322;
        public const int GetCameraWidth                = 0x46811c76;
        public const int GetCameraHeight               = 0x1dff8e99;
        public const int GetCameraOffsetX              = 0x188c1213;
        public const int GetCameraOffsetY              = 0x7f724a3b;
        public const int GetCameraBitPerPixel          = 0x5bcc81c3;
        public const int SetCameraBitPerPixel          = 0x03af8c0f;
        public const int GetCameraStride               = 0x3c32983a;
        public const int GetCameraPixelSizeUm          = 0x453fa1da;
        public const int MinCameraShutter              = 0x6b122b3a;
        public const int MaxCameraShutter              = 0x465b506b;
        public const int GetCameraShutter              = 0x3cfa2458;
        public const int SetCameraShutter              = 0x32d64e41;
        public const int MinCameraShutterUs            = 0x51cdfc15;
        public const int MaxCameraShutterUs            = 0x6e36f8f0;
        public const int GetCameraShutterUs            = 0x13486622;
        public const int SetCameraShutterUs            = 0x1db09ec3;
        public const int MinCameraGain                 = 0x51441d3f;
        public const int MaxCameraGain                 = 0x13695ca2;
        public const int GetCameraGain                 = 0x6f1d1bc2;
        public const int SetCameraGain                 = 0x7682c6da;
        public const int MinCameraBrightness           = 0x42f89a18;
        public const int MaxCameraBrightness           = 0x31ea7248;
        public const int GetCameraBrightness           = 0x5ee2574c;
        public const int SetCameraBrightness           = 0x01f15409;
        public const int GetCameraImage                = 0x55f1cfac;
        public const int GetOptCameraImage             = 0x3d4e070b;
        public const int StartCameraGrabTime           = 0x4955ed50;
        public const int GetLaserWavelength            = 0x637d6d1f;
        public const int SetLaserOutput                = 0x08332ea8;
        public const int MinMotorCoderPos              = 0x14683792;
        public const int MaxMotorCoderPos              = 0x610eb049;
        public const int GetMotorCoderPos              = 0x7a904391;
        public const int MinMotorPos                   = 0x4baa26b5;
        public const int MaxMotorPos                   = 0x057b1c69;
        public const int GetMotorPos                   = 0x2690f04f;
        public const int SetMotorPos                   = 0x63da8bec;
        public const int UnitMotorPos                  = 0x03f93167;
    }
}