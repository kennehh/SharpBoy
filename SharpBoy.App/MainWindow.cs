using SDL2;
using SharpBoy.App.Sdl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App
{
    public class MainWindow
    {
        private readonly GameBoyFramebuffer _gbFramebuffer;
        private Window _window;
        private Renderer _renderer;

        private int _width;
        private int _height;
        private bool running = false;

        public MainWindow(GameBoyFramebuffer gbFramebuffer)
        {
            _width = 160 * 4;
            _height = 144 * 4;
            _gbFramebuffer = gbFramebuffer;
        }

        public void Initialise()
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            _window = new Window("SharpBoy", _width, _height, SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            _renderer = new Renderer(_window);

            _gbFramebuffer.Initialise(_renderer);
            _gbFramebuffer.Resize(_width, _height);
        }

        public void Run()
        {
            SDL.SDL_Event e;
            running = true;

            while (running)
            {
                PollEvents();
                Update();
                Render();
            }
        }
        
        public void PollEvents()
        {
            while (SDL.SDL_PollEvent(out var e) != 0)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        running = false;
                        break;

                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        if (e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED ||
                            e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED)
                        {
                            // The window was resized. Call the Resize function.
                            _gbFramebuffer.Resize(e.window.data1, e.window.data2);
                        }
                        break;
                }
            }
        }

        public void Update()
        {

        }

        public void Render()
        {
            SDL.SDL_RenderClear(_renderer.Handle);
            _gbFramebuffer.Render();
            SDL.SDL_RenderPresent(_renderer.Handle);
        }

        public void Dispose()
        {
            _gbFramebuffer?.Dispose();
            _renderer?.Dispose();
            _window?.Dispose();
            SDL.SDL_Quit();
        }
    }
}
