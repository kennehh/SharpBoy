using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Graphics
{
    public interface IPpuMemory
    {
        IReadWriteMemory Oam { get; }
        IReadWriteMemory Vram { get; }
    }
}