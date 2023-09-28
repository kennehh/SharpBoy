using SDL2;
using SharpBoy.App.ImGui;
using SharpBoy.App.Sdl;
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
        private SdlRenderer renderer;

        private int width;
        private int height;
        private bool running = false;

        public MainWindow(SdlManager sdlManager, GameBoyFramebuffer gbFramebuffer)
        {
            width = 160 * 4;
            height = 144 * 4;
            this.sdlManager = sdlManager;
            this.gbFramebuffer = gbFramebuffer;
            imGuiManager = new ImGuiManager("hello");

            this.sdlManager.OnRender += Render;
            this.sdlManager.OnPollEvent += PollEvent;
        }

        public void Initialise()
        {
            window = new SdlWindow("SharpBoy", width, height, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            renderer = new SdlRenderer(window);

            sdlManager.SetCurrentRenderer(renderer);
            
            gbFramebuffer.Initialise(renderer);
            gbFramebuffer.Resize(width, height);
        }

        public void Run()
        {
            running = true;

            while (running)
            {
                sdlManager.PollEvents();
                Update();
                sdlManager.Render();
            }
        }
        
        public void PollEvent(SDL.SDL_Event e)
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

        public void Update()
        {

        }

        public void Render()
        {
            gbFramebuffer.Render();
            //_imGuiManager.Render();
        }

        public void Dispose()
        {
            sdlManager.OnRender -= Render;
            sdlManager.OnPollEvent -= PollEvent;
            sdlManager.Dispose();

            gbFramebuffer?.Dispose();
            renderer?.Dispose();
            window?.Dispose();
        }
    }
}
