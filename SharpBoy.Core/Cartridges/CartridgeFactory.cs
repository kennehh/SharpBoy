using SharpBoy.Core.Cartridges.Interfaces;
using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Cartridges
{
    public class CartridgeFactory : ICartridgeFactory
    {
        public ICartridge CreateCartridge(byte[] romData, byte[] ramData = null)
        {
            var rom = new Rom(romData);
            var header = new CartridgeHeader(rom);
            Ram ram = null;

            if (ramData != null)
            {
                if (ramData.Length != header.RamSize)
                {
                    Console.WriteLine($"Invalid ERam - expected: {header.RamSize}, actual: {ramData.Length}");
                }
                else
                {
                    ram = new Ram(ramData);
                }
            }

            switch (header.Type)
            {
                case CartridgeType.NoMbc:
                    return new NoMbcCartridge(header, rom, ram);
                case CartridgeType.Mbc1:
                    return new Mbc1Cartridge(header, rom, ram);
                case CartridgeType.Mbc2:
                    return new Mbc2Cartridge(header, rom, ram);
                case CartridgeType.Mbc3:
                    return new Mbc3Cartridge(header, rom, ram);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
