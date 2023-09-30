using SharpBoy.Core.Cartridges;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core.Memory
{
    public interface IMmu
    {
        bool BootRomLoaded { get; }
        void LoadBootRom(byte[] rom);
        void LoadCartridge(ICartridge cartridge);
        void Write(int address, byte value);
        byte Read(int address);
    }
}
