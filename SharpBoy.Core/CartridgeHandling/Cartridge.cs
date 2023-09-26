using SharpBoy.Core.Memory;

namespace SharpBoy.Core.CartridgeHandling
{
    public class Cartridge : ICartridge
    {
        public CartridgeHeader Header { get; } = new CartridgeHeader();

        private IReadableMemory rom;
        private IReadWriteMemory eram = new Ram(0x2000);

        public Cartridge()
        {
            var data = new byte[0x8000];
            Array.Fill<byte>(data, 0xff);
            rom = new Rom(data);
        }

        public Cartridge(byte[] rom)
        {
            this.rom = new Rom(rom);
        }

        public byte ReadRom(ushort address) => rom.Read(address);
        public byte ReadERam(ushort address) => eram.Read(address);
        public void WriteERam(ushort address, byte value) => eram.Write(address, value);

        public void LoadCartridge(byte[] data)
        {
            rom = new Rom(data);
            Header.ReadRom(rom);
        }
    }
}
