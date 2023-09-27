using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.Sdl
{
    public class Renderer : IDisposable
    {
        private IntPtr _renderer;

        public Renderer(Window window)
        {
            _renderer = SDL.SDL_CreateRenderer(window.Handle, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (_renderer == IntPtr.Zero)
            {
                throw new Exception($"Renderer creation failed: {SDL.SDL_GetError()}");
            }
        }

        public IntPtr Handle => _renderer;

        public void Dispose()
        {
            if (_renderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(_renderer);
                _renderer = IntPtr.Zero;
            }
        }
    }
}
