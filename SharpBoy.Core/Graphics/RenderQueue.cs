using System.Collections.Concurrent;

namespace SharpBoy.Core.Graphics
{
    public class RenderQueue : IRenderQueue
    {
        private AutoResetEvent frameReady = new AutoResetEvent(false);
        private ConcurrentQueue<Memory<byte>> queue = new ConcurrentQueue<Memory<byte>>();
        private Memory<byte> frontBuffer = new byte[160 * 144 * 4];
        private Memory<byte> backBuffer = new byte[160 * 144 * 4];

        public void Enqueue(ReadOnlyMemory<byte> frame)
        {
            // Always write to the back buffer
            frame.CopyTo(backBuffer);

            // Swap buffers when a frame is complete
            var temp = frontBuffer;
            frontBuffer = backBuffer;
            backBuffer = temp;

            queue.Enqueue(frontBuffer);
            if (queue.Count > 0xff)
            {
                queue.TryDequeue(out _);
            }
            frameReady.Set();
        }

        public bool TryDequeue(out ReadOnlySpan<byte> frameBuffer)
        {
            if (queue.TryDequeue(out var fb))
            {
                frameBuffer = fb.Span;
                return true;
            }
            frameBuffer = default;
            return false;
        }

        public Memory<byte> GetBackBuffer() => backBuffer;

        public void WaitForNextFrame() => frameReady.WaitOne();
    }
}
