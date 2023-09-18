using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using SharpBoy.Core;
using SharpBoy.Core.Rendering;
using SharpBoy.Rendering.Silk;
using System;

namespace SharpBoy.App.Controls
{
    public partial class EmulatorRenderer : OpenGlControlBase
    {
        private readonly GameBoy gameBoy;
        private readonly IRenderer renderer;
        private bool initialised = false;

        public EmulatorRenderer()
        {
            var serviceCollection = new ServiceCollection()
                .RegisterCoreServices()
                .AddSingleton<IRenderer, SilkRenderer>()
                .BuildServiceProvider();

            gameBoy = serviceCollection.GetRequiredService<GameBoy>();
            renderer = serviceCollection.GetRequiredService<IRenderer>();

            const string romPath = "C:\\Projects\\Tetris (World) (Rev A).gb";
            const string bootPath = "Z:\\games\\bios\\gb\\gb_bios.bin";
            gameBoy.LoadBootRom(bootPath);
            gameBoy.LoadCartridge(romPath);
            gameBoy.Run();
        }

        protected override void OnOpenGlInit(GlInterface gl)
        {
            if (!initialised)
            {
                renderer.Initialise(gl.GetProcAddress);                
                this.WhenAnyValue(x => x.Bounds.Size)
                    .Subscribe(size => renderer.Resize((int)size.Width, (int)size.Height));
                initialised = true;
            }
        }

        protected override void OnOpenGlRender(GlInterface gl, int fb)
        {
            renderer.Render();
            RequestNextFrameRendering();
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            base.OnOpenGlDeinit(gl);
            gameBoy.Stop();
            renderer.Dispose();
        }
    }
}
