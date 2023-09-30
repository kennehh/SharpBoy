using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Cartridges
{
    public class CartridgeFactory : ICartridgeFactory
    {
        public ICartridge CreateCartridge(byte[] data)
        {
            var rom = new Rom(data);
            var header = new CartridgeHeader(rom);

            switch (header.Type)
            {
                case CartridgeType.NoMbc:
                    return new NoMbcCartridge(header, rom);
                case CartridgeType.Mbc1:
                    return new Mbc1Cartridge(header, rom);
                case CartridgeType.Mbc3:
                    return new Mbc1Cartridge(header, rom);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
