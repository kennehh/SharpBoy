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
using System.Numerics;
using System.Runtime.InteropServices;

namespace SharpBoy.Rendering.Silk
{
    public class SilkRenderer : IRenderer, IDisposable
    {
        private const int LcdWidth = 160;
        private const int LcdHeight = 144;
        private readonly IRenderQueue renderQueue;
        private bool disposed = false;

        private GL gl = null;
        private Display display = null;

        public SilkRenderer(IRenderQueue renderQueue)
        {
            this.renderQueue = renderQueue;
        }

        public void Initialise(Func<string, nint> getProcAddress)
        {
            if (gl == null)
            {
                gl = GL.GetApi(getProcAddress);
                gl.ClearColor(Color.Black);
                display = new Display(gl, LcdWidth, LcdHeight);
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                gl?.Dispose();
                disposed = true;
            }
        }

        public void Render()
        {
            renderQueue.WaitForNextFrame();
            if (renderQueue.TryDequeue(out var fb))
            {
                gl.Clear((uint)ClearBufferMask.ColorBufferBit);
                gl.Viewport(position.X, position.Y, size.X, size.Y);
                display.Render(fb);
            }
        }

        private Vector2D<int> position = new Vector2D<int>(0, 0);
        private Vector2D<uint> size = new Vector2D<uint>(LcdWidth, LcdHeight);


        public void Resize(int width, int height)
        {
            var ratioX = width / (float)LcdWidth;
            var ratioY = height / (float)LcdHeight;
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            // Calculate the width and height that the will be rendered to
            size.X = Convert.ToUInt32(LcdWidth * ratio);
            size.Y = Convert.ToUInt32(LcdHeight * ratio);
            // Calculate the position, which will apply proper "pillar" or "letterbox" 
            position.X = Convert.ToInt32((width - LcdWidth * ratio) / 2);
            position.Y = Convert.ToInt32((height - LcdHeight * ratio) / 2);
        }
    }
}
