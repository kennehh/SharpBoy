using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using SharpBoy.Core.Rendering;
using System;
using System.Drawing;

namespace SharpBoy.App.Avalonia.Controls
{
    public partial class EmulatorRenderer : OpenGlControlBase
    {
        public IRenderer Renderer { get; set; }
        private bool initialised = false;

        public EmulatorRenderer()
        {
            // Listen for property changes
            PropertyChanged += HandlePropertyChanged;
        }

        private void HandlePropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (initialised && e.Property == BoundsProperty)
            {
                var size = ((Rect)e.NewValue).Size;
                Renderer.Resize((int)size.Width, (int)size.Height);
            }
        }

        protected override void OnOpenGlInit(GlInterface gl)
        {
            if (!initialised)
            {
                Renderer.Initialise(gl.GetProcAddress);
                Renderer.Resize((int)Bounds.Size.Width, (int)Bounds.Size.Height);
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
