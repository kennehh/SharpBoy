using SDL2;

namespace SharpBoy.App.SdlCore
{
    public class SdlRenderer : IDisposable
    {
        private IntPtr renderer;

        public SdlRenderer(SdlWindow window)
        {
            renderer = SDL.SDL_CreateRenderer(window.Handle, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (renderer == IntPtr.Zero)
            {
                throw new SdlException("Renderer creation failed");
            }
        }

        public IntPtr Handle => renderer;

        public void Dispose()
        {
            if (renderer != IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(renderer);
                renderer = IntPtr.Zero;
            }
        }
    }
}
