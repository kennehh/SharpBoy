using SharpBoy.Core.Rendering;
using SharpBoy.Rendering.Silk;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.ComponentModel;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace SharpBoy.Rendering.Silk
{
    public class SilkRenderer : IRenderer, IDisposable
    {
        private const int LcdWidth = 160;
        private const int LcdHeight = 144;
        private readonly IRenderQueue renderQueue;
        private bool disposed = false;

        private IWindow window = null;
        private GL gl = null;
        private Display display = null;

        public event Action Closing;

        public SilkRenderer(IRenderQueue renderQueue)
        {
            this.renderQueue = renderQueue;
        }

        public void Initialise()
        {
            var options = WindowOptions.Default with
            {
                Size = new Vector2D<int>(LcdWidth * 4, LcdHeight * 4),
                Title = "Game Boy Emulator"
            };
            window = Window.Create(options);
            window.Resize += Resize;
            window.Render += Render;
            window.Closing += () => Closing?.Invoke();
            window.Load += () =>
            {
                gl = window.CreateOpenGL();
                gl.ClearColor(Color.Black);
                display = new Display(gl, LcdWidth, LcdHeight);
            };
        }

        public void Run()
        {
            window.Run();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                window?.Close();
                display?.Dispose();
                window?.Dispose();
                gl?.Dispose();
                disposed = true;
            }
        }

        private void Render(double deltaTime)
        {
            renderQueue.WaitForNextFrame();
            if (renderQueue.TryDequeue(out var fb))
            {
                gl.Clear((uint)ClearBufferMask.ColorBufferBit);
                display.Render(fb);
            }
        }

        private void Resize(Vector2D<int> size)
        {
            var width = size.X;
            var height = size.Y;

            var ratioX = width / (float)LcdWidth;
            var ratioY = height / (float)LcdHeight;
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            // Calculate the width and height that the will be rendered to
            var viewWidth = Convert.ToUInt32(LcdWidth * ratio);
            var viewHeight = Convert.ToUInt32(LcdHeight * ratio);
            // Calculate the position, which will apply proper "pillar" or "letterbox" 
            var viewX = Convert.ToInt32((width - LcdWidth * ratio) / 2);
            var viewY = Convert.ToInt32((height - LcdHeight * ratio) / 2);

            gl.Viewport(viewX, viewY, viewWidth, viewHeight);
        }
    }
}
