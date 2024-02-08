/*************************************************************************
 * Copyright (c) 2023-2024 Reinhard Caspary                              *
 * <reinhard.caspary@phoenixd.uni-hannover.de>                           *
 * This program is free software under the terms of the MIT license.     *
 *************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;


namespace DhmServ
{
    class DhmServ
    {
        public string addr = "0.0.0.0";
        public int port = 27182;

        private TcpListener server = null;
        private TcpClient client = null;
        private NetworkStream clientStream = null;

        private Dhm dhm = null;
        private int delayMs = 1000;

        public DhmServ()
        {
            this.dhm = new Dhm();
            Task task = dhm.Initialize(this.delayMs);
            this.server = new TcpListener(IPAddress.Parse(this.addr), this.port);
            this.server.Start();
        }

        public void Start()
        {
            int result = Result.success;

            Console.WriteLine($"This is DhmServ {this.dhm.classVersion}");
            while (true)
            {
                Console.WriteLine($"Waiting {this.addr}:{this.port}...");
                this.client = this.server.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                this.clientStream = this.client.GetStream();

                result = this.RunSession();
                if (Result.IsError(result))
                {
                    Console.WriteLine($"Error [{result}].");
                    this.Send(Command.Error, result);
                }
                else
                {
                    Console.WriteLine($"Closed [{result}].");
                }

                // Not required
                this.clientStream.Close();
                this.client.Close();
                //this.dhm.laserOutput = 0;
            }
        }

        // Main server method: Read and handle commands

        private int RunSession()
        {
            int result = Result.unknown;
            int cmd = 0;

            // Main server loop: Read and handle commands
            while (result != Result.quit & client.Connected)
            {
                // Read command code from network stream
                result = this.Receive(ref cmd);
                if (Result.IsError(result)) return result;
                //Console.WriteLine($"Command 0x{cmd:x8}");

                // Handle commands without hardware access
                result = this.BaseCommands(cmd);
                //Console.WriteLine($"Stage 1 result 0x{result:x8}");
                if (Result.IsError(result)) return result;

                // Handle commands with hardware access
                if (result == Result.unknown & this.dhm.status >= Status.initialized)
                {
                    // Handle commands requiring no initialized hardware
                    result = this.InitCommands(cmd);
                    //Console.WriteLine($"Stage 2 result 0x{result:x8}");
                    if (Result.IsError(result)) return result;

                    // Handle commands requiring initialized hardware
                    if (result == Result.unknown & dhm.dhmConfig >= 0)
                    {
                        result = this.FullCommands(cmd);
                        //Console.WriteLine($"Stage 3 result 0x{result:x8}");
                        if (Result.IsError(result)) return result;
                    }
                }

                // Unhandled command code
                if (result == Result.unknown) return Result.err_unknown_command;
            }

            // Quit server session
            return Result.quit;
        }

        // Commands without hardware access
        private int BaseCommands(int cmd)
        {
            switch (cmd)
            {
                case Command.Quit:
                    this.client.Close();
                    break;

                case Command.GetVersion:
                    this.Send(cmd, this.dhm.dhmVersion);
                    break;

                case Command.GetCmdVersion:
                    this.Send(cmd, this.dhm.dhmCmdVersion);
                    break;

                case Command.GetDhmStatus:
                    this.Send(cmd, this.dhm.status);
                    break;

                default:
                    return Result.unknown;
            }
            return Result.success;
        }

        // Commands requiring no initialized hardware
        private int InitCommands(int cmd)
        {
            int result;

            switch (cmd)
            {
                case Command.GetConfigList:
                    ConfigItemList configList = this.dhm.dhmConfigList;
                    this.Send(cmd, configList);
                    break;

                case Command.GetConfig:
                    this.Send(cmd, this.dhm.dhmConfig);
                    break;

                case Command.SetConfig:
                    int configId = 0;
                    result = this.Receive(ref configId);
                    if (Result.IsError(result)) return result;
                    this.dhm.dhmConfig = configId;
                    if (this.dhm.dhmConfig != configId) return Result.err_unknown_config;
                    this.dhm.laserOutput = 1;
                    this.Send(cmd);
                    break;

                default:
                    return Result.unknown;
            }
            return Result.success;
        }

        // Commands requiring initialized hardware
        private int FullCommands(int cmd)
        {
            int result;

            switch (cmd)
            {
                case Command.GetDhmSerial:
                    this.Send(cmd, this.dhm.dhmSerial);
                    break;

                case Command.GetObjectiveName:
                    this.Send(cmd, this.dhm.objectiveName);
                    break;

                case Command.GetObjectiveDescription:
                    this.Send(cmd, this.dhm.objectiveDescription);
                    break;

                case Command.GetObjectiveMagnification:
                    this.Send(cmd, this.dhm.objectiveMagnification);
                    break;

                case Command.GetObjectiveNumericalAperture:
                    this.Send(cmd, this.dhm.objectiveNumericalAperture);
                    break;

                case Command.GetObjectivePixelSizeXUm:
                    this.Send(cmd, this.dhm.objectivePixelSizeXUm);
                    break;

                case Command.GetObjectivePixelSizeYUm:
                    this.Send(cmd, this.dhm.objectivePixelSizeYUm);
                    break;

                case Command.GetCameraSerial:
                    this.Send(cmd, this.dhm.cameraSerial);
                    break;

                case Command.GetCameraName:
                    this.Send(cmd, this.dhm.cameraName);
                    break;

                case Command.GetCameraMaxWidth:
                    this.Send(cmd, this.dhm.cameraMaxWidth);
                    break;

                case Command.GetCameraMaxHeight:
                    this.Send(cmd, this.dhm.cameraMaxHeight);
                    break;

                case Command.GetCameraWidth:
                    this.Send(cmd, this.dhm.cameraWidth);
                    break;

                case Command.GetCameraHeight:
                    this.Send(cmd, this.dhm.cameraHeight);
                    break;

                case Command.GetCameraOffsetX:
                    this.Send(cmd, this.dhm.cameraOffsetX);
                    break;

                case Command.GetCameraOffsetY:
                    this.Send(cmd, this.dhm.cameraOffsetY);
                    break;

                case Command.GetCameraBitPerPixel:
                    this.Send(cmd, this.dhm.cameraBitPerPixel);
                    break;

                case Command.SetCameraBitPerPixel:
                    int bpp = 0;
                    result = this.Receive(ref bpp);
                    if (Result.IsError(result)) return result;
                    if (bpp < 1) return Result.err_bpp_underflow;
                    this.dhm.cameraBitPerPixel = bpp;
                    this.Send(cmd);
                    break;

                case Command.GetCameraStride:
                    this.Send(cmd, this.dhm.cameraStride);
                    break;

                case Command.GetCameraPixelSizeUm:
                    this.Send(cmd, this.dhm.cameraPixelSizeUm);
                    break;

                case Command.MinCameraShutter:
                    this.Send(cmd, this.dhm.cameraMinShutter);
                    break;

                case Command.MaxCameraShutter:
                    this.Send(cmd, this.dhm.cameraMaxShutter);
                    break;

                case Command.GetCameraShutter:
                    this.Send(cmd, this.dhm.cameraShutter);
                    break;

                case Command.SetCameraShutter:
                    int shutter = 0;
                    result = this.Receive(ref shutter);
                    if (Result.IsError(result)) return result;
                    if (shutter < this.dhm.cameraMinShutter) return Result.err_shutter_underflow;
                    if (shutter > this.dhm.cameraMaxShutter) return Result.err_shutter_overflow;
                    this.dhm.cameraShutter = shutter;
                    this.Send(cmd);
                    break;

                case Command.MinCameraShutterUs:
                    this.Send(cmd, this.dhm.cameraMinShutterUs);
                    break;

                case Command.MaxCameraShutterUs:
                    this.Send(cmd, this.dhm.cameraMaxShutterUs);
                    break;

                case Command.GetCameraShutterUs:
                    this.Send(cmd, this.dhm.cameraShutterUs);
                    break;

                case Command.SetCameraShutterUs:
                    float shutter_us = 0;
                    result = this.Receive(ref shutter_us);
                    if (Result.IsError(result)) return result;
                    if (shutter_us < this.dhm.cameraMinShutterUs) return Result.err_shutter_us_underflow;
                    if (shutter_us > this.dhm.cameraMaxShutterUs) return Result.err_shutter_us_overflow;
                    this.dhm.cameraShutterUs = shutter_us;
                    this.Send(cmd);
                    break;

                case Command.MinCameraGain:
                    this.Send(cmd, this.dhm.cameraMinGain);
                    break;

                case Command.MaxCameraGain:
                    this.Send(cmd, this.dhm.cameraMaxGain);
                    break;

                case Command.GetCameraGain:
                    this.Send(cmd, this.dhm.cameraGain);
                    break;

                case Command.SetCameraGain:
                    int gain = 0;
                    result = this.Receive(ref gain);
                    if (Result.IsError(result)) return result;
                    if (gain < this.dhm.cameraMinGain) return Result.err_gain_underflow;
                    if (gain > this.dhm.cameraMaxGain) return Result.err_gain_overflow;
                    this.dhm.cameraGain = gain;
                    this.Send(cmd);
                    break;

                case Command.MinCameraBrightness:
                    this.Send(cmd, this.dhm.cameraMinBrightness);
                    break;

                case Command.MaxCameraBrightness:
                    this.Send(cmd, this.dhm.cameraMaxBrightness);
                    break;

                case Command.GetCameraBrightness:
                    this.Send(cmd, this.dhm.cameraBrightness);
                    break;

                case Command.SetCameraBrightness:
                    int brightness = 0;
                    result = this.Receive(ref brightness);
                    if (Result.IsError(result)) return result;
                    if (brightness < this.dhm.cameraMinBrightness) return Result.err_brightness_underflow;
                    if (brightness > this.dhm.cameraMaxBrightness) return Result.err_brightness_overflow;
                    this.dhm.cameraBrightness = brightness;
                    this.Send(cmd);
                    break;

                case Command.GetCameraImage:
                    this.Send(cmd, this.dhm.cameraImage);
                    break;

                case Command.StartCameraGrabTime:
                    this.dhm.SetGrabTime();
                    this.Send(cmd);
                    break;

                case Command.GetLaserWavelength:
                    this.Send(cmd, this.dhm.laserWavelength);
                    break;

                case Command.SetLaserOutput:
                    int output = 0;
                    result = this.Receive(ref output);
                    if (Result.IsError(result)) return result;
                    if (output > 0) output = 1;
                    if (output < 0) output = 0;
                    this.dhm.laserOutput = output;
                    this.Send(cmd);
                    break;

                case Command.MinMotorCoderPos:
                    this.Send(cmd, this.dhm.motorMinCoderPos);
                    break;

                case Command.MaxMotorCoderPos:
                    this.Send(cmd, this.dhm.motorMaxCoderPos);
                    break;

                case Command.GetMotorCoderPos:
                    this.Send(cmd, this.dhm.motorCoderPos);
                    break;

                case Command.MinMotorPos:
                    this.Send(cmd, this.dhm.motorMinPos);
                    break;

                case Command.MaxMotorPos:
                    this.Send(cmd, this.dhm.motorMaxPos);
                    break;

                case Command.GetMotorPos:
                    this.Send(cmd, this.dhm.motorPos);
                    break;

                case Command.SetMotorPos:
                    float pos = 0.0F;
                    result = this.Receive(ref pos);
                    if (Result.IsError(result)) return result;
                    if (pos < this.dhm.motorMinPos) return Result.err_pos_underflow;
                    if (pos > this.dhm.motorMaxPos) return Result.err_pos_overflow;
                    this.dhm.motorPos = pos;
                    this.Send(cmd);
                    break;

                case Command.UnitMotorPos:
                    this.Send(cmd, this.dhm.motorUnitPos);
                    break;

                default:
                    return Result.unknown;
            }
            return Result.success;
        }

        private int Receive(ref int value)
        {
            byte[] buffer = new byte[sizeof(int)];
            int count = clientStream.Read(buffer, 0, buffer.Length);
            if (count != buffer.Length) return Result.err_recv_int_size;

            value = BitConverter.ToInt32(buffer, 0);
            return 0;
        }

        private int Receive(ref float value)
        {
            byte[] buffer = new byte[sizeof(float)];
            int count = clientStream.Read(buffer, 0, buffer.Length);
            if (count != buffer.Length) return Result.err_recv_float_size;

            value = BitConverter.ToSingle(buffer, 0);
            return 0;
        }

        private void Send(int cmd)
        {
            byte[] msg = BitConverter.GetBytes(cmd);
            clientStream.Write(msg, 0, msg.Length);
        }

        private void Send(int cmd, int value)
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(cmd));
            buffer.AddRange(BitConverter.GetBytes(value));
            byte[] msg = buffer.ToArray();
            clientStream.Write(msg, 0, msg.Length);
        }

        private void Send(int cmd, float value)
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(cmd));
            buffer.AddRange(BitConverter.GetBytes(value));
            byte[] msg = buffer.ToArray();
            clientStream.Write(msg, 0, msg.Length);
        }

        private void Send(int cmd, double value)
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(cmd));
            buffer.AddRange(BitConverter.GetBytes(value));
            byte[] msg = buffer.ToArray();
            clientStream.Write(msg, 0, msg.Length);
        }

        private void Send(int cmd, string value)
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(cmd));
            byte[] avalue = Encoding.UTF8.GetBytes(value);
            buffer.AddRange(BitConverter.GetBytes(avalue.Length));
            buffer.AddRange(avalue);
            byte[] msg = buffer.ToArray();
            clientStream.Write(msg, 0, msg.Length);
        }

        private void Send(int cmd, ConfigItemList value)
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(cmd));
            buffer.AddRange(BitConverter.GetBytes(value.Count));
            foreach (ConfigItem config in value)
            {
                buffer.AddRange(BitConverter.GetBytes(config.id));
                byte[] avalue = Encoding.UTF8.GetBytes(config.name);
                buffer.AddRange(BitConverter.GetBytes(avalue.Length));
                buffer.AddRange(avalue);
            }
            byte[] msg = buffer.ToArray();
            clientStream.Write(msg, 0, msg.Length);
        }

        private void Send(int cmd, Image value)
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(cmd));
            buffer.AddRange(BitConverter.GetBytes(value.height));
            buffer.AddRange(BitConverter.GetBytes(value.width));
            buffer.AddRange(BitConverter.GetBytes(value.stride));
            buffer.AddRange(value.data);
            byte[] msg = buffer.ToArray();
            clientStream.Write(msg, 0, msg.Length);
        }
    }
}
