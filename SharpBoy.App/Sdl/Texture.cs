using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.Sdl
{
    public class Texture
    {
        private IntPtr _texture;
        private int _width;
        private int _height;

        public Texture(Renderer renderer, int width, int height)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "0");

            _width = width;
            _height = height;

            _texture = SDL.SDL_CreateTexture(renderer.Handle, SDL.SDL_PIXELFORMAT_ABGR8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, _width, _height);
            if (_texture == IntPtr.Zero)
            {
                throw new Exception($"Texture creation failed: {SDL.SDL_GetError()}");
            }
        }

        public IntPtr Handle => _texture;

        public unsafe void Update(ReadOnlySpan<byte> pixelData)
        {
            fixed (byte* pBuffer = &pixelData.GetPinnableReference())
            {
                SDL.SDL_UpdateTexture(_texture, IntPtr.Zero, (IntPtr)pBuffer, _width * 4);
            }
        }

        public void Dispose()
        {
            if (_texture != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(_texture);
                _texture = IntPtr.Zero;
            }
        }
    }
}
