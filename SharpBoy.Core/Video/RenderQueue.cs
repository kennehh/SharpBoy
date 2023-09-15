using System.Collections.Concurrent;

namespace SharpBoy.Core.Video
{
    public class RenderQueue
    {
        public AutoResetEvent FrameReady { get; } = new AutoResetEvent(false);
        private ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();

        public void Enqueue(byte[] frame)
        {
            queue.Enqueue(frame);
            FrameReady.Set();
        }

        public bool TryDequeue(out byte[] frame)
        {
            return queue.TryDequeue(out frame);
        }
    }
}
