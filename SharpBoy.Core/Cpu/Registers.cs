using System;
using System.Runtime.InteropServices;

namespace SharpBoy.Core.Cpu
{
    [StructLayout(LayoutKind.Explicit)]
    internal class Registers
    {
        [FieldOffset(1)]
        public byte A;
        [FieldOffset(0)]
        public Flag F;
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

        [FieldOffset(12)]
        public byte IE;

        [FieldOffset(13)]
        public bool IME;

        public void SetFlag(Flag flag, bool val) => F = val ? F | flag : F & ~flag;
        public void ToggleFlag(Flag flag) => F ^= flag;
        public bool GetFlag(Flag flag) => F.HasFlag(flag);
    }

    internal enum Flag : byte
    {
        Zero = 1 << 7,
        Subtract = 1 << 6,
        HalfCarry = 1 << 5,
        Carry = 1 << 4
    }
}
