using SharpBoy.Core.Graphics.Interfaces;

namespace SharpBoy.Core.Graphics
{
    public class DoubleBufferManager : IFrameBufferManager
    {
        private Memory<byte> frontBuffer = new byte[160 * 144 * 4];
        private Memory<byte> backBuffer = new byte[160 * 144 * 4];
        private int frameReadyFlag = 0; // 0 for not ready, 1 for ready

        // This method is intended to be called from only one thread.
        // Pushes a new frame to the back buffer and then swaps buffers.
        public void PushFrame(ReadOnlyMemory<byte> frame)
        {
            frame.CopyTo(backBuffer);
            Swap();

            // Atomically set the flag to 1, indicating a frame is ready.
            Interlocked.Exchange(ref frameReadyFlag, 1);
        }

        // This method can be called from any thread. 
        // It atomically checks if a frame is ready, returns it if so, and resets the flag.
        public bool TryGetNextFrame(out ReadOnlySpan<byte> nextFrame)
        {
            if (Interlocked.CompareExchange(ref frameReadyFlag, 0, 1) == 1)
            {
                // Atomically checks if frameReadyFlag is 1 and, if so, sets it to 0
                nextFrame = frontBuffer.Span;
                return true;
            }
            nextFrame = default;
            return false;
        }

        public void ClearBuffers()
        {
            frontBuffer.Span.Fill(0);
            backBuffer.Span.Fill(0);
            Interlocked.Exchange(ref frameReadyFlag, 1);
        }

        private void Swap()
        {
            var temp = frontBuffer;
            frontBuffer = backBuffer;
            backBuffer = temp;
        }
    }
}
