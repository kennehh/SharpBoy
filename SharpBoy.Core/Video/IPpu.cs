using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Video
{
    internal interface IPpu
    {
        ReadOnlySpan<byte> FrameBuffer { get; }
        byte ReadVram(ushort address);
        byte ReadOam(ushort address);
        byte ReadRegister(ushort address);
        void WriteVram(ushort address, byte value);
        void WriteOam(ushort address, byte value);
        void WriteRegister(ushort address, byte value);
        void Sync(int cycles);
    }
}
