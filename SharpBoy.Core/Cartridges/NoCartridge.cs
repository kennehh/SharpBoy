using SharpBoy.Core.Cartridges.Interfaces;

namespace SharpBoy.Core.Cartridges
{
    public class NoCartridge : ICartridge
    {
        private static readonly NoCartridge instance = new NoCartridge();
        public static NoCartridge Instance => instance;

        private static readonly CartridgeHeader header = new CartridgeHeader();
        public CartridgeHeader Header => header;

        public byte ReadRam(ushort address) => 0xff;
        public byte ReadRom(ushort address) => 0xff;
        public void WriteRam(ushort address, byte value) { }
        public void WriteRom(ushort address, byte value) { }
    }
}
