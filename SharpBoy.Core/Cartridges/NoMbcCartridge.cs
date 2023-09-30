using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Cartridges
{
    public class NoMbcCartridge : Cartridge
    {
        public NoMbcCartridge(CartridgeHeader header, IReadableMemory rom) : base(header, rom) { }
    }
}
