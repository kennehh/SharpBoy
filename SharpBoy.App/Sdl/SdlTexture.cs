using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App.Sdl
{
    public class SdlTexture : IDisposable
    {
        private IntPtr texture;
        private int width;
        private int height;

        public SdlTexture(SdlRenderer renderer, int width, int height)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "0");

            this.width = width;
            this.height = height;

            texture = SDL.SDL_CreateTexture(renderer.Handle, SDL.SDL_PIXELFORMAT_ABGR8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, this.width, this.height);
            if (texture == IntPtr.Zero)
            {
                throw new SdlException("Texture creation failed");
            }
        }

        public IntPtr Handle => texture;

        public unsafe void Update(ReadOnlySpan<byte> pixelData)
        {
            fixed (byte* pBuffer = &pixelData.GetPinnableReference())
            {
                SDL.SDL_UpdateTexture(texture, IntPtr.Zero, (IntPtr)pBuffer, width * 4);
            }
        }

        public void Dispose()
        {
            if (texture != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(texture);
                texture = IntPtr.Zero;
            }
        }
    }
}
