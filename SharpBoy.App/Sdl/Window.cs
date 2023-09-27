using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.Sdl
{
    public class Window
    {
        private IntPtr _window;

        public Window(string title, int width, int height, SDL.SDL_WindowFlags flags)
        {
            _window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, flags);
            if (_window == IntPtr.Zero)
            {
                throw new Exception($"Window creation failed: {SDL.SDL_GetError()}");
            }
        }

        public IntPtr Handle => _window;

        public void Dispose()
        {
            if (_window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(_window);
                _window = IntPtr.Zero;
            }
        }
    }
}
