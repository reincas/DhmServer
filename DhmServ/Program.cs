/*************************************************************************
 * Copyright (c) 2023-2024 Reinhard Caspary                              *
 * <reinhard.caspary@phoenixd.uni-hannover.de>                           *
 * This program is free software under the terms of the MIT license.     *
 *************************************************************************/

//using Basler.Pylon;
using System;

namespace DhmServ
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DhmServ server = new DhmServ();
            server.Start();

            //DhmCamera camera = new DhmCamera();
            //camera.Grab();
        }
    }
}
