using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core.Memory
{
    public interface IMmu
    {
        byte ReadValue(ushort address);
        void WriteValue(ushort address, byte value);
    }
}
