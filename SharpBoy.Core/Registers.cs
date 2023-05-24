using System;
using System.Runtime.InteropServices;

namespace SharpBoy.Core
{
    [StructLayout(LayoutKind.Explicit)]
    internal class Registers
    {
        [FieldOffset(0)]
        public byte F;
        [FieldOffset(1)]
        public byte A;
        [FieldOffset(0)]
        public ushort AF;

        [FieldOffset(2)]
        public byte C;
        [FieldOffset(3)]
        public byte B;
        [FieldOffset(2)]
        public ushort BC;

        [FieldOffset(4)]
        public byte E;
        [FieldOffset(5)]
        public byte D;
        [FieldOffset(4)]
        public ushort DE;

        [FieldOffset(6)]
        public byte L;
        [FieldOffset(7)]
        public byte H;
        [FieldOffset(6)]
        public ushort HL;

        [FieldOffset(8)]
        public ushort SP;

        [FieldOffset(10)]
        public ushort PC;

        public void SetFlag(Flag flag, bool val)
        {
            var bitPosition = (int)flag;
            F = val ? Utils.SetBit(F, bitPosition) : Utils.ClearBit(F, bitPosition);
        }

        public void ToggleFlag(Flag flag)
        {
            var bitPosition = (int)flag;
            F = Utils.ToggleBit(F, bitPosition);
        }

        public bool GetFlag(Flag flag) => Utils.IsBitSet(F, (int)flag);
    }

    internal enum Flag
    {
        Zero = 7,
        Subtract = 6,
        HalfCarry = 5,
        Carry = 4
    }
}
