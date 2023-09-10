using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Processor
{
    internal interface ITimer
    {
        void Update(int cycles);
        byte ReadRegister(ushort address);
        void WriteRegister(ushort address, byte value);
    }
}
