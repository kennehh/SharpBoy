using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core.Memory
{
    public interface IMmu
    {
        void LoadBootRom(byte[] rom);
        byte ReadValue(ushort address);
        void WriteValue(ushort address, byte value);
    }
}
