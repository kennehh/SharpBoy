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
        public IRenderer Renderer { get; set; }
        private bool initialised = false;

        protected override void OnOpenGlInit(GlInterface gl)
        {
            if (!initialised)
            {
                Renderer.Initialise(gl.GetProcAddress);                
                this.WhenAnyValue(x => x.Bounds.Size).Subscribe(size => Renderer.Resize((int)size.Width, (int)size.Height));
                initialised = true;
            }
        }

        protected override void OnOpenGlRender(GlInterface gl, int fb)
        {
            Renderer.Render();
            RequestNextFrameRendering();
        }

        protected override void OnOpenGlDeinit(GlInterface gl)
        {
            base.OnOpenGlDeinit(gl);
            Renderer.Dispose();
        }
    }
}
