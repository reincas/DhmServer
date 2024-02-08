/*************************************************************************
 * Copyright (c) 2023-2024 Reinhard Caspary                              *
 * <reinhard.caspary@phoenixd.uni-hannover.de>                           *
 * This program is free software under the terms of the MIT license.     *
 *************************************************************************/

namespace DhmServ
{
    public static class Result
    {
        public const int quit = 2;
        public const int unknown = 1;
        public const int success = 0;
        public const int err_no_hardware = -1;
        public const int err_not_initialized = -2;
        public const int err_unknown_config = -3;
        public const int err_recv_int_size = -4;
        public const int err_recv_float_size = -5;
        public const int err_shutter_underflow = -6;
        public const int err_shutter_overflow = -7;
        public const int err_shutter_us_underflow = -8;
        public const int err_shutter_us_overflow = -9;
        public const int err_gain_underflow = -10;
        public const int err_gain_overflow = -11;
        public const int err_brightness_underflow = -12;
        public const int err_brightness_overflow = -13;
        public const int err_pos_underflow = -14;
        public const int err_pos_overflow = -15;
        public const int err_bpp_underflow = -16;
        public const int err_unknown_command = -17;

        public static bool IsError(int result)
        {
            return result < 0;
        }
    }

    public static class Status
    {
        public const int closed = 0;
        public const int initialized = 1;
        public const int running = 2;
    }
}
