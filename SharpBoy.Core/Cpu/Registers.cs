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
        public Interrupt IE;

        [FieldOffset(13)]
        public Interrupt IF;

        [FieldOffset(14)]
        public bool IME;

        public bool InterruptRequested => IF != 0;

        public void SetFlag(Flag flag, bool val) => F = (val ? F | flag : F & ~flag);

        public void SetInterruptFlag(Interrupt flag, bool val) => IF = (val ? IF | flag : IF & ~flag);

        public bool InterruptAllowed(Interrupt flag) => IME && IE.HasFlag(flag) && IF.HasFlag(flag);
    }

    [Flags]
    internal enum Flag : byte
    {
        Zero = 1 << 7,
        Subtract = 1 << 6,
        HalfCarry = 1 << 5,
        Carry = 1 << 4
    }

    [Flags]
    internal enum Interrupt : byte
    {
        VBlank = 1 << 0,
        LcdStat = 1 << 1,
        Timer = 1 << 2,
        Serial = 1 << 3,
        Joypad = 1 << 4,
    }
}
