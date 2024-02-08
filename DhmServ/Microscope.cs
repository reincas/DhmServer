/*************************************************************************
 * Copyright (c) 2023-2024 Reinhard Caspary                              *
 * <reinhard.caspary@phoenixd.uni-hannover.de>                           *
 * This program is free software under the terms of the MIT license.     *
 *************************************************************************/

using System;
using LynceeTec.Eucalyptus;
using LynceeTec.Interfaces;
using LynceeTec.DHM;
using LynceeTec.Imaging;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using System.Threading;

namespace DhmServ
{
    public class ConfigItem
    {
        public int id { get; }
        public string name { get; }

        public ConfigItem(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public class ConfigItemList : List<ConfigItem> { }

    public class Image
    {
        public int height { get; }
        public int width { get; }
        public int stride { get; }
        public byte[] data { get; }

        public Image(int height, int width, int stride, byte[] data)
        {
            this.height = height;
            this.width = width;
            this.stride = stride;
            this.data = data;
        }
    }

    public class Dhm
    {
        public int status = Status.closed;

        private readonly string databasePath = @"C:\ProgramData\LynceeTec\Koala";
        private ConfigManager configManager = null;
        private IUserManager userManager = null;
        private IDHMDevice dhmDevice = null;

        private string username = "user";
        private string password = "user";

        public readonly int classVersion = 4;
        private static List<string> physUnits = new List<string>() { "", "µm", "degree", "coder" };

        DateTime grabTime = DateTime.Now;

        // This method should be called whenever the camera settings are modified. It stores a time at which the camera image can savely be grabbed.
        public void SetGrabTime()
        {
            // A delay of 4 times the exposure time delivered reliable results
            double delayMs = 4e-3 * (double)this.dhmDevice.Camera.ShutterUs;
            this.grabTime = DateTime.Now + TimeSpan.FromMilliseconds(delayMs);
        }

        // Grab the current camera image and return as Image object. Wait eventually until the grab time is reached.
        public Image GrabImage()
        {
            Hologram hologram;

            TimeSpan delay = this.grabTime - DateTime.Now;
            if (delay > TimeSpan.Zero)
            {
                hologram = this.dhmDevice.Camera.Grab(delay, new object());
            }
            else
            {
                hologram = this.dhmDevice.Camera.Grab(new object());
            }
            return new Image(hologram.Height, hologram.Width, hologram.Stride, hologram.Data);
        }

        // Property methods
        public int dhmVersion { get { return this.classVersion; } }
        public int dhmCmdVersion { get { return Command.Version; } }
        public int dhmStatus { get { return this.status; } }
        public ConfigItemList dhmConfigList {
            get
            {
                ConfigItemList configs = new ConfigItemList();
                foreach (IMeasurementConfig mc in this.configManager.AllMeasurementConfigsInfo)
                {
                    configs.Add(new ConfigItem((int)mc.MeasurementId, mc.Name));
                }
                return configs;
            }
        }
        public int dhmConfig {
            get
            {
                // No measurement setup configured yet
                if (this.configManager.CurrentHoloMeasurementConfig == null)
                    return -1;

                // Return id of current measurement setup
                return this.configManager.CurrentHoloMeasurementConfig.ConfigId;
            }
            set
            {
                // Ignore invalid measurement setup id
                bool match = false;
                foreach (IMeasurementConfig mc in this.configManager.AllMeasurementConfigsInfo)
                {
                    if ((int)mc.MeasurementId == value)
                    {
                        match = true;
                        break;
                    }
                }
                if (!match) { return; }

                // Don't load the same measurement setup again
                if (value == this.dhmConfig) { return; }

                // Load new measurement setup and set parameters (OPL motor position, etc.) to their standard values
                this.configManager.LoadMeasurementConfiguration(value, null);
            }
        }
        public string dhmSerial { get { return this.dhmDevice.SerialNumber; } }

        public string objectiveName { get { return this.configManager.CurrentHoloMeasurementConfig.ObjectiveInfo.Name; } }
        public string objectiveDescription { get { return this.configManager.CurrentHoloMeasurementConfig.ObjectiveInfo.Description; } }
        public double objectiveMagnification { get { return this.configManager.CurrentHoloMeasurementConfig.ObjectiveInfo.Magnification; } }
        public double objectiveNumericalAperture { get { return this.configManager.CurrentHoloMeasurementConfig.ObjectiveInfo.NumericalAperture; } }
        public double objectivePixelSizeXUm { get { return this.configManager.CurrentHoloMeasurementConfig.ObjectiveInfo.PixelXSize_um; } }
        public double objectivePixelSizeYUm { get { return this.configManager.CurrentHoloMeasurementConfig.ObjectiveInfo.PixelYSize_um; } }

        public string cameraSerial { get { return this.dhmDevice.Camera.UniqueID; } }
        public string cameraName { get { return this.dhmDevice.Camera.CameraName; } }
        public int cameraMaxWidth { get { return this.dhmDevice.Camera.MaxWidth; } }
        public int cameraMaxHeight { get { return this.dhmDevice.Camera.MaxHeight; } }
        public int cameraWidth { get { return this.dhmDevice.Camera.Width; } }
        public int cameraHeight { get { return this.dhmDevice.Camera.Height; } }
        public int cameraOffsetX { get { return this.dhmDevice.Camera.OffsetX; } }
        public int cameraOffsetY { get { return this.dhmDevice.Camera.OffsetY; } }
        public int cameraBitPerPixel {
            get { return this.dhmDevice.Camera.Bpp; }
            set { this.dhmDevice.Camera.Bpp = value;
                Console.WriteLine($"BPP = {value} [{this.dhmDevice.Camera.Bpp}]");
            }
        }
        public int cameraStride { get { return this.dhmDevice.Camera.Stride; } }
        public float cameraPixelSizeUm { get { return this.dhmDevice.Camera.PixelSizeum; } }
        public int cameraMinShutter { get { return this.dhmDevice.Camera.MinShutter; } }
        public int cameraMaxShutter { get { return this.dhmDevice.Camera.MaxShutter; } }
        public int cameraShutter {
            get { return this.dhmDevice.Camera.Shutter; }
            set
            { 
                this.dhmDevice.Camera.Shutter = value;
                this.SetGrabTime();
            }
        }
        public float cameraMinShutterUs { get { return this.dhmDevice.Camera.MinShutterUs; } }
        public float cameraMaxShutterUs { get { return this.dhmDevice.Camera.MaxShutterUs; } }
        public float cameraShutterUs
        {
            get { return this.dhmDevice.Camera.ShutterUs; }
            set
            {
                this.dhmDevice.Camera.ShutterUs = value;
                this.SetGrabTime();
            }
        }
        public int cameraMinGain { get { return this.dhmDevice.Camera.MinGain; } }
        public int cameraMaxGain { get { return this.dhmDevice.Camera.MaxGain; } }
        public int cameraGain
        {
            get { return this.dhmDevice.Camera.Gain; }
            set
            {
                this.dhmDevice.Camera.Gain = value;
                this.SetGrabTime();
            }
        }
        public int cameraMinBrightness { get { return this.dhmDevice.Camera.MinBrightness; } }
        public int cameraMaxBrightness { get { return this.dhmDevice.Camera.MaxBrightness; } }
        public int cameraBrightness
        {
            get { return this.dhmDevice.Camera.Brightness; }
            set
            {
                this.dhmDevice.Camera.Brightness = value;
                this.SetGrabTime();
            }
        }
        public Image cameraImage { get { return this.GrabImage(); } }
        public double laserWavelength { get { return this.configManager.CurrentHoloMeasurementConfig.SourcesWavelengths[0]; } }

        public int laserOutput {
            set
            {
                if (value > 0)
                {
                    //Switch the source(s) of the configuration on, and the other(s) off
                    this.dhmDevice.LaserSourcesController.SwitchConfigSourcesOn();
                    this.SetGrabTime();
                }
                else
                {
                    this.dhmDevice.LaserSourcesController.SwitchAllSourcesOff();
                }
            }
        }

        public int motorMinCoderPos { get { return this.dhmDevice.Motors[MotorizedDevicesType.OPL].MinCoderPos; } }
        public int motorMaxCoderPos { get { return this.dhmDevice.Motors[MotorizedDevicesType.OPL].MaxCoderPos; } }
        public int motorCoderPos { get { return this.dhmDevice.Motors[MotorizedDevicesType.OPL].GetCoderPosition(); } }
        public float motorMinPos { get { return this.dhmDevice.Motors[MotorizedDevicesType.OPL].MinPos; } }
        public float motorMaxPos { get { return this.dhmDevice.Motors[MotorizedDevicesType.OPL].MaxPos; } }
        public float motorPos
        {
            get
            {
                return this.dhmDevice.Motors[MotorizedDevicesType.OPL].GetPhysPosition();
            }
            set
            {
                // Move motor synchronously
                this.dhmDevice.Motors[MotorizedDevicesType.OPL].GoToPositionPhys(value).Wait();
                // This should be equivalent, but results in a 50 µm offset:
                // this.dhmDevice.Motors[MotorizedDevicesType.OPL].GoToPhysicalPosition(value);
                this.SetGrabTime();
            }
        }
        public string motorUnitPos { get { return physUnits[(int)this.dhmDevice.Motors[MotorizedDevicesType.OPL].UnitType]; } }

        // Constructor method
        public Dhm() 
        {
            this.status = Status.closed;
        }

        // Asynchonous initialization
        public async Task Initialize(int delayMs)
        {
            // Load configuration database
            while (this.status < Status.running)
                await Task.Run(() => InitDhm(delayMs));
        }

        // Load configuration database and connect to DHM hardware
        private void InitDhm(int delayMs)
        {
            try
            {
                Console.WriteLine("Load config database...");
                this.configManager = new ConfigManager(this.databasePath);

                // Login to user manager => as we connected a DHM instance before, it will log into the DHM automatically
                this.userManager = configManager.UserManager;
                this.userManager.Login(this.username, this.Secure(this.password));

                // Init the configuration manager AFTER you've logged in. Without logging, you do not have the access to the encrypted configuration files
                this.configManager.Init();
                this.status = Status.initialized;

                // Create an instance of the dhm device
                Console.WriteLine("Initialize DHM...");
                this.dhmDevice = DHMDeviceFactory.CreateDHM(this.configManager.GetDHMParameters(), this.configManager);

                //Init digital microscope 
                this.Start();
                //this.dhmDevice.InitDhm(null).Wait();
                //this.configManager.LinkDhmDevice(this.dhmDevice);

                //First grab for camera warm-up
                //this.dhmDevice.Camera.Grab();
                this.status = Status.running;
                Console.WriteLine("DHM running...");
            }
            catch
            {
                Thread.Sleep(delayMs);
            }
            /*
            this.Stop();
            DhmCamera camera = new DhmCamera();
            camera.Grab();
            */
        }

        public void Start()
        {
            this.dhmDevice.InitDhm(null).Wait();
            this.configManager.LinkDhmDevice(this.dhmDevice);
            this.dhmDevice.Camera.Grab();
        }

        public void Stop()
        {
            this.configManager.UnlinkDhmDevice();
            this.dhmDevice.UnloadConfig();
        }

        private SecureString Secure(string password)
        {
            SecureString userPassword = new SecureString();

            foreach (char c in password)
            {
                userPassword.AppendChar(c);
            }
            return userPassword;
        }
    }
}