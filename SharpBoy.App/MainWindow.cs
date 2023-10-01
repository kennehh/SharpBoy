using SDL2;
using SharpBoy.App.ImGuiCore;
using SharpBoy.App.SdlCore;
using SharpBoy.Core;
using System.Runtime.InteropServices;

namespace SharpBoy.App
{
    public class MainWindow : IDisposable
    {
        private readonly GameBoy gameboy;
        private readonly SdlManager sdlManager;
        private readonly GameBoyFramebuffer gbFramebuffer;
        private readonly ImGuiManager imGuiManager;
        private SdlWindow window;

        private int width;
        private int height;
        private bool running = false;
        private bool romLoaded = false;

        public MainWindow(GameBoy gameboy, SdlManager sdlManager, GameBoyFramebuffer gbFramebuffer)
        {
            width = 160 * 4;
            height = 144 * 4;
            this.gameboy = gameboy;
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

        public void Dispose()
        {
            sdlManager.Dispose();

            gbFramebuffer?.Dispose();
            window?.Dispose();
        }

        private void Render()
        {
            if (romLoaded)
            {
                gbFramebuffer.Render();
            }
        }

        private void PollEvent()
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
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        HandleInput(e.key.keysym.sym, true);
                        break;
                    case SDL.SDL_EventType.SDL_KEYUP:
                        HandleInput(e.key.keysym.sym, false);
                        break;
                    case SDL.SDL_EventType.SDL_DROPFILE:
                        LoadRom(e.drop.file);
                        break;
                }
            }
        }

        Task gameBoyTask = null;

        private void LoadRom(nint pathPtr)
        {
            gameboy?.Stop();
            gameBoyTask?.Wait();

            var pathToRom = Marshal.PtrToStringUTF8(pathPtr);
            var pathToRam = pathToRom.Replace(Path.GetExtension(pathToRom), ".sav");

            if (Path.Exists(pathToRam))
            {
                gameboy.LoadCartridge(pathToRom, pathToRam);
            }
            else
            {
                gameboy.LoadCartridge(pathToRom);
            }

            gameBoyTask = Task.Run(gameboy.Run);
            romLoaded = true;
        }

        private void HandleInput(SDL.SDL_Keycode key, bool isKeyDown)
        {
            switch (key)
            {
                case SDL.SDL_Keycode.SDLK_TAB:
                    gameboy.UncapSpeed(isKeyDown);
                    break;
            }
        }
    }
}
