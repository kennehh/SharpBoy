using SDL2;
using SharpBoy.App.ImGuiCore;
using SharpBoy.App.SdlCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.App
{
    public class MainWindow : IDisposable
    {
        private readonly SdlManager sdlManager;
        private readonly GameBoyFramebuffer gbFramebuffer;
        private readonly ImGuiManager imGuiManager;
        private SdlWindow window;

        private int width;
        private int height;
        private bool running = false;

        public MainWindow(SdlManager sdlManager, GameBoyFramebuffer gbFramebuffer)
        {
            width = 160 * 4;
            height = 144 * 4;
            this.sdlManager = sdlManager;
            this.gbFramebuffer = gbFramebuffer;
        }

        public void Initialise()
        {
            window = new SdlWindow("SharpBoy", width, height, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            gbFramebuffer.Initialise(window.Renderer);
            gbFramebuffer.Resize(width, height);
        }

        public void Run()
        {
            running = true;

            while (running)
            {                
                PollEvent();
                window.Render(Render);
            }
        }
        
        public void PollEvent()
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
                            gbFramebuffer.Resize(e.window.data1, e.window.data2);
                        }
                        break;
                }
            }
        }

        public void Render()
        {
            gbFramebuffer.Render();
        }

        public void Dispose()
        {
            sdlManager.Dispose();

            gbFramebuffer?.Dispose();
            window?.Dispose();
        }
    }
}
