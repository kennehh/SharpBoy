using System.Collections.Concurrent;

namespace SharpBoy.Core.Graphics
{
    public class RenderQueue : IRenderQueue
    {
        private AutoResetEvent frameReady = new AutoResetEvent(false);
        private ConcurrentQueue<ReadOnlyMemory<byte>> queue = new ConcurrentQueue<ReadOnlyMemory<byte>>();

        public void Enqueue(ReadOnlyMemory<byte> frame)
        {
            queue.Enqueue(frame);
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

        public void WaitForNextFrame() => frameReady.WaitOne();
    }
}
