using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Cartridges
{
    public abstract class Cartridge : ICartridge
    {
        public CartridgeHeader Header { get; }

        protected IReadableMemory Rom { get; private set; }
        protected IReadWriteMemory ERam { get; private set; }

        public Cartridge(CartridgeHeader header, IReadableMemory rom)
        {
            Header = header;
            Rom = rom;

            if (header.RamSize > 0)
            {
                ERam = new Ram(header.RamSize);
            }
        }

        public virtual byte ReadRom(ushort address) => Rom.Read(address);
        public virtual byte ReadERam(ushort address) => ERam?.Read(address) ?? 0xff;
        public virtual void WriteRom(ushort address, byte value) { }
        public virtual void WriteERam(ushort address, byte value) => ERam?.Write(address, value);
    }
}
