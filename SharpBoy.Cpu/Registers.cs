using System;
using System.Runtime.InteropServices;

namespace SharpBoy.Cpu
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

        public void WriteToRegister(Register8Bit reg, byte val)
        {
            switch (reg)
            {
                case Register8Bit.A: A = val; break;
                case Register8Bit.B: B = val; break;
                case Register8Bit.C: C = val; break;
                case Register8Bit.D: D = val; break;
                case Register8Bit.E: E = val; break;
                case Register8Bit.H: H = val; break;
                case Register8Bit.L: L = val; break;
                default: throw new ArgumentException($"Unknown register: {reg}", nameof(reg));
            }
        }

        public void WriteToRegister(Register16Bit reg, ushort val)
        {
            switch (reg)
            {
                case Register16Bit.AF: AF = val; break;
                case Register16Bit.BC: BC = val; break;
                case Register16Bit.DE: DE = val; break;
                case Register16Bit.HL: HL = val; break;
                case Register16Bit.PC: PC = val; break;
                case Register16Bit.SP: SP = val; break;
                default: throw new ArgumentException($"Unknown register: {reg}", nameof(reg));
            }
        }

        public byte ReadFromRegister(Register8Bit reg)
        {
            switch (reg)
            {
                case Register8Bit.A: return A;
                case Register8Bit.B: return B;
                case Register8Bit.C: return C;
                case Register8Bit.D: return D;
                case Register8Bit.E: return E;
                case Register8Bit.H: return H;
                case Register8Bit.L: return L;
                default: throw new ArgumentException($"Unknown register: {reg}", nameof(reg));
            }
        }

        public ushort ReadFromRegister(Register16Bit reg)
        {
            switch (reg)
            {
                case Register16Bit.AF: return AF;
                case Register16Bit.BC: return BC;
                case Register16Bit.DE: return DE;
                case Register16Bit.HL: return HL;
                case Register16Bit.PC: return PC;
                case Register16Bit.SP: return SP;
                default: throw new ArgumentException($"Unknown register: {reg}", nameof(reg));
            }
        }
    }

    internal enum Flag
    {
        Zero = 7,
        Subtract = 6,
        HalfCarry = 5,
        Carry = 4
    }

    internal enum Register8Bit
    {
        B = 0,
	    C = 1,
	    D = 2,
	    E = 3,
	    H = 4,
	    L = 5,
	    A = 7
    };

    internal enum Register16Bit
    {
        BC = 0, 
	    DE = 1,
	    HL = 2,
        SP = 3,
	    AF = 4,	    
	    PC = 5,
    };
}
