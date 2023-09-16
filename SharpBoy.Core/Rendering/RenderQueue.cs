using System.Collections.Concurrent;

namespace SharpBoy.Core.Rendering
{
    public class RenderQueue
    {
        private AutoResetEvent frameReady = new AutoResetEvent(false);
        private ConcurrentQueue<ReadOnlyMemory<byte>> queue = new ConcurrentQueue<ReadOnlyMemory<byte>>();

        public void Enqueue(byte[] frame)
        {
            if (queue.Count <= 0xff)
            {
                queue.Enqueue(frame);
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
