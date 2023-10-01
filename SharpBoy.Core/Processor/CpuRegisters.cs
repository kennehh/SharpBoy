using System.Runtime.InteropServices;

namespace SharpBoy.Core.Processor
{
    [StructLayout(LayoutKind.Explicit)]
    public class CpuRegisters
    {
        [FieldOffset(1)]
        public byte A;
        [FieldOffset(0)]
        public Flags F;
        [FieldOffset(0)]
        public ushort AF;

        [FieldOffset(3)]
        public byte B;
        [FieldOffset(2)]
        public byte C;
        [FieldOffset(2)]
        public ushort BC;

        [FieldOffset(5)]
        public byte D;
        [FieldOffset(4)]
        public byte E;
        [FieldOffset(4)]
        public ushort DE;

        [FieldOffset(7)]
        public byte H;
        [FieldOffset(6)]
        public byte L;
        [FieldOffset(6)]
        public ushort HL;

        [FieldOffset(8)]
        public ushort SP;

        [FieldOffset(10)]
        public ushort PC;

        public void SetFlag(Flags flag, bool val) => F = (val ? F | flag : F & ~flag);
    }

    [Flags]
    public enum Flags : byte
    {
        Zero = 1 << 7,
        Subtract = 1 << 6,
        HalfCarry = 1 << 5,
        Carry = 1 << 4
    }
}
