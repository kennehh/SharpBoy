using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core
{
    public interface IMmu
    {
        byte Read8Bit(ushort address);
        ushort Read16Bit(ushort address);
        void Write8Bit(ushort address, byte value);
        void Write16Bit(ushort address, ushort value);
    }
}
