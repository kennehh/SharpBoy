using SharpBoy.Core.Graphics.Interfaces;

namespace SharpBoy.Core.Tests.Mocks
{
    internal class FrameBufferManagerMock : IFrameBufferManager
    {
        private ReadOnlyMemory<byte> frameBuffer;

        public void ClearBuffers()
        {
            frameBuffer = null;
        }

        public void PushFrame(ReadOnlyMemory<byte> frame)
        {
            frameBuffer = frame;
        }

        public bool TryGetNextFrame(out ReadOnlySpan<byte> nextFrame)
        {
            nextFrame = frameBuffer.Span;
            return true;
        }
    }
}
