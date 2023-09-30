using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Cartridges
{
    public abstract class Cartridge : ICartridge
    {
        public CartridgeHeader Header { get; }

        private readonly IReadableMemory rom = null;
        private readonly IReadWriteMemory eram = null;

        public Cartridge(CartridgeHeader header, IReadableMemory rom)
        {
            Header = header;
            this.rom = rom;

            if (header.RamSize > 0)
            {
                eram = new Ram(header.RamSize);
            }
        }

        public virtual byte ReadRom(ushort address) => rom.Read(address);
        public virtual byte ReadERam(ushort address) => eram?.Read(address) ?? 0xff;
        public virtual void WriteERam(ushort address, byte value) => eram?.Write(address, value);
    }
}
