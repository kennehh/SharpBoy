﻿using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.Sdl
{
    public class SdlException : Exception
    {
        public SdlException() : this(string.Empty)
        {
        }

        public SdlException(string message) : base(GetSdlMessage(message))
        {
        }

        private static string GetSdlMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message += ": ";
            }
            return message?.Trim() ?? string.Empty + $" {SDL.SDL_GetError()}";
        }
    }
}
