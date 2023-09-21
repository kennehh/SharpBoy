using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Cartridge
{
    public class CartridgeReader : ICartridgeReader
    {
        private IReadableMemory rom;
        private IReadWriteMemory eram = new Ram(0x2000);

        public CartridgeReader()
        {
            var data = new byte[0x8000];
            Array.Fill<byte>(data, 0xff);
            LoadCartridge(data);
        }

        public CartridgeReader(byte[] rom)
        {
            this.rom = new Rom(rom);
        }

        public byte ReadRom(ushort address) => rom.Read(address);
        public byte ReadERam(ushort address) => eram.Read(address);
        public void WriteERam(ushort address, byte value) => eram.Write(address, value);

        public void LoadCartridge(byte[] data)
        {
            rom = new Rom(data);
        }
    }
}
