using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Cartridges
{
    public class NoMbcCartridge : Cartridge
    {
        public NoMbcCartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram) { }
    }
}
