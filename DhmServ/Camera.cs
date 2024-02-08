/*************************************************************************
 * Copyright (c) 2023-2024 Reinhard Caspary                              *
 * <reinhard.caspary@phoenixd.uni-hannover.de>                           *
 * This program is free software under the terms of the MIT license.     *
 *************************************************************************/

using Basler.Pylon;
using System;

namespace DhmServ
{
    internal class DhmCamera
    {
        public void Grab()
        {
            //DhmServ server = new DhmServ();
            //server.Start();
            using (Camera camera = new Camera())
            {
                Console.WriteLine("Using camera {0}.", camera.CameraInfo[CameraInfoKey.ModelName]);

                camera.CameraOpened += Configuration.AcquireContinuous;

                // Open the connection to the camera device.
                camera.Open();

                camera.StreamGrabber.Start();

                IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(5000, TimeoutHandling.ThrowException);
                using (grabResult)
                {
                    // Image grabbed successfully?
                    if (grabResult.GrabSucceeded)
                    {
                        // Access the image data.
                        Console.WriteLine("SizeX: {0}", grabResult.Width);
                        Console.WriteLine("SizeY: {0}", grabResult.Height);
                        byte[] buffer = grabResult.PixelData as byte[];
                        Console.WriteLine("Gray value of first pixel: {0}", buffer[0]);
                        Console.WriteLine("");
                    }
                    else
                    {
                        Console.WriteLine("Error: {0} {1}", grabResult.ErrorCode, grabResult.ErrorDescription);
                    }
                }
                // Stop grabbing.
                camera.StreamGrabber.Stop();

                // Close the connection to the camera device.
                camera.Close();
            }
        }
    }
}
