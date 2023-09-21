using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Cartridge
{
    public interface ICartridgeReader
    {
        void LoadCartridge(byte[] rom);
        byte ReadRom(ushort address);
        byte ReadERam(ushort address);
        void WriteERam(ushort address, byte value);
    }
}
