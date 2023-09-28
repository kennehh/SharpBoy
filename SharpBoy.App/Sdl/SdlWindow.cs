using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.Sdl
{
    public class SdlWindow : IDisposable
    {
        private IntPtr window;

        public SdlWindow(string title, int width, int height, SDL.SDL_WindowFlags flags)
        {
            window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, flags);
            if (window == IntPtr.Zero)
            {
                throw new SdlException("Window creation failed");
            }
        }

        public IntPtr Handle => window;

        public void Dispose()
        {
            if (window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(window);
                window = IntPtr.Zero;
            }
        }
    }
}
