namespace SharpBoy.Core.Graphics.Interfaces
{
    public interface IFrameBufferManager
    {
        void PushFrame(ReadOnlyMemory<byte> frame);
        bool TryGetNextFrame(out ReadOnlySpan<byte> nextFrame);
        void ClearBuffers();
    }
}
