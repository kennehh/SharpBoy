using SharpBoy.Core.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    public interface IPpu
    {
        PpuRegisters Registers { get; }
        byte ReadVram(ushort address);
        byte ReadOam(ushort address);
        byte ReadRegister(ushort address);
        void WriteVram(ushort address, byte value);
        void WriteOam(ushort address, byte value);
        void WriteRegister(ushort address, byte value);
        void DoOamDmaTransfer(byte[] sourceData);
        void Tick();
    }
}
