using SDL2;

namespace SharpBoy.App.SdlCore
{
    public class SdlTexture : IDisposable
    {
        private IntPtr texture;
        private readonly int width;
        private readonly int height;
        private readonly int bytesPerPixel;

        public SdlTexture(SdlRenderer renderer, int width, int height, uint format, int bytesPerPixel)
        {
            SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "0");

            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;
            texture = SDL.SDL_CreateTexture(renderer.Handle, format, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, this.width, this.height);
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
                Update(pBuffer);
            }
        }

        public unsafe void Update(byte* pixelData)
        {
            SDL.SDL_UpdateTexture(texture, IntPtr.Zero, (IntPtr)pixelData, width * bytesPerPixel);
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
