using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Cartridges
{
    public abstract class Cartridge : ICartridge
    {
        public CartridgeHeader Header { get; }

        protected IReadableMemory Rom { get; private set; }
        protected IReadWriteMemory Ram { get; private set; }

        public Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : this(header, rom, ram, header.RamSize)
        {
        }

        public Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram, int ramSize)
        {
            Header = header;
            Rom = rom;

            if (ramSize > 0)
            {
                Ram = ram ?? new Ram(ramSize);
            }
        }

        public virtual byte ReadRom(ushort address) => Rom.Read(address);
        public virtual byte ReadRam(ushort address) => Ram?.Read(address) ?? 0xff;
        public virtual void WriteRom(ushort address, byte value) { }
        public virtual void WriteRam(ushort address, byte value) => Ram?.Write(address, value);
    }
}
