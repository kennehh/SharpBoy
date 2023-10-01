namespace SharpBoy.Core.Graphics
{
    public interface IFrameBufferManager
    {
        void PushFrame(ReadOnlyMemory<byte> frame);
        bool TryGetNextFrame(out ReadOnlySpan<byte> nextFrame);
        void ClearBuffers();
    }
}
