using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Graphics.Interfaces
{
    public interface IPpuMemory
    {
        IReadWriteMemory Oam { get; }
        IReadWriteMemory Vram { get; }
    }
}