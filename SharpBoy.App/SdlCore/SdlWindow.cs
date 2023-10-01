using SDL2;

namespace SharpBoy.App.SdlCore
{
    public class SdlWindow : IDisposable
    {
        private IntPtr window;
        public SdlRenderer Renderer { get; }

        public SdlWindow(string title, int width, int height, SDL.SDL_WindowFlags flags)
        {
            window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, flags);
            if (window == IntPtr.Zero)
            {
                throw new SdlException("Window creation failed");
            }

            Renderer = new SdlRenderer(this);
        }

        public void Render(Action render)
        {
            if (Renderer == null)
            {
                return;
            }

            SDL.SDL_RenderClear(Renderer.Handle);
            render();
            SDL.SDL_RenderPresent(Renderer.Handle);
        }

        public IntPtr Handle => window;

        public void Dispose()
        {
            if (window != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(window);
                window = IntPtr.Zero;
            }
            Renderer?.Dispose();
        }
    }
}
