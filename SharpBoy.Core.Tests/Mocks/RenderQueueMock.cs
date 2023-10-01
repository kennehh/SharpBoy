using SharpBoy.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Tests.Mocks
{
    internal class RenderQueueMock : IRenderQueue
    {
        private ReadOnlyMemory<byte> frameBuffer;

        public void Enqueue(ReadOnlyMemory<byte> frame)
        {
            frameBuffer = frame;
        }

        public bool TryDequeue(out ReadOnlySpan<byte> fb)
        {
            fb = frameBuffer.Span;
            return true;
        }

        public void WaitForNextFrame()
        {
        }
    }
}
