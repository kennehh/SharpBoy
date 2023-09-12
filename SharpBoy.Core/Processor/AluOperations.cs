using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core.Processor
{
    internal static class AluOperations
    {
        public static byte cp(Registers registers, byte a, byte b)
        {
            var result = a - b;
            var halfCarryResult = (a & 0xf) - (b & 0xf);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, true);
            registers.SetFlag(Flag.Carry, result < 0);
            registers.SetFlag(Flag.HalfCarry, halfCarryResult < 0);
            return a;
        }

        public static byte or(Registers registers, byte a, byte b)
        {
            var result = a | b;
            var byteResult = (byte)result;
            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, false);
            return byteResult;
        }

        public static byte xor(Registers registers, byte a, byte b)
        {
            var result = a ^ b;
            var byteResult = (byte)result;
            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, false);
            return byteResult;
        }

        public static byte and(Registers registers, byte a, byte b)
        {
            var result = a & b;
            var byteResult = (byte)result;
            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, true);
            registers.SetFlag(Flag.Carry, false);
            return byteResult;
        }

        public static byte add(Registers registers, byte a, byte b) => add(registers, a, b, false, true);

        public static byte adc(Registers registers, byte a, byte b) => add(registers, a, b, true, true);

        public static byte sub(Registers registers, byte a, byte b) => sub(registers, a, b, false, true);

        public static byte sbc(Registers registers, byte a, byte b) => sub(registers, a, b, true, true);

        public static byte inc(Registers registers, byte a) => add(registers, a, 1, false, false);

        public static byte dec(Registers registers, byte a) => sub(registers, a, 1, false, false);

        public static byte daa(Registers registers, byte a)
        {
            // Check the condition flags to determine the adjustment needed
            var carryFlag = registers.F.HasFlag(Flag.Carry);
            var halfCarryFlag = registers.F.HasFlag(Flag.HalfCarry);

            // Perform the DAA adjustment
            if (!registers.F.HasFlag(Flag.Subtract))
            {
                if (carryFlag || a > 0x99)
                {
                    a += 0x60;
                    registers.SetFlag(Flag.Carry, true);
                }
                if (halfCarryFlag || (a & 0x0F) > 0x09)
                {
                    a += 0x06;
                }
            }
            else
            {
                if (carryFlag)
                {
                    a -= 0x60;
                    registers.SetFlag(Flag.Carry, true);
                }
                if (halfCarryFlag)
                {
                    a -= 0x06;
                }
            }

            // Update the zero flag, clear half carry flag
            registers.SetFlag(Flag.Zero, a == 0);
            registers.SetFlag(Flag.HalfCarry, false);

            return a;
        }

        public static void ccf(Registers registers)
        {
            registers.F ^= Flag.Carry;
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
        }

        public static void scf(Registers registers)
        {
            registers.SetFlag(Flag.Carry, true);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
        }

        public static byte cpl(Registers registers, byte value)
        {
            var result = ~value;
            registers.SetFlag(Flag.Subtract, true);
            registers.SetFlag(Flag.HalfCarry, true);
            return (byte)result;
        }

        public static byte swap(Registers registers, byte value)
        {
            var result = value >> 4 | value << 4;
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, false);
            return (byte)result;
        }

        public static byte bit(Registers registers, byte value, byte bitPosition)
        {
            var result = BitUtils.BitValue(value, bitPosition);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, true);
            return value;
        }

        public static byte set(byte value, byte bitPosition) => BitUtils.SetBit(value, bitPosition);

        public static byte res(byte value, byte bitPosition) => BitUtils.ClearBit(value, bitPosition);

        public static byte sla(Registers registers, byte value)
        {
            var result = (byte)(value << 1);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, BitUtils.IsBitSet(value, 7));
            return result;
        }

        public static byte sra(Registers registers, byte value)
        {
            var result = (byte)(value >> 1 | value & 0x80);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, BitUtils.IsBitSet(value, 0));
            return result;
        }

        public static byte srl(Registers registers, byte value)
        {
            var result = (byte)(value >> 1 | 0 << 7);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, BitUtils.IsBitSet(value, 0));
            return result;
        }

        public static byte rl(Registers registers, byte value) => rl(registers, value, true);
        public static byte rl_cb(Registers registers, byte value) => rl(registers, value, false);

        public static byte rlc(Registers registers, byte value) => rlc(registers, value, true);
        public static byte rlc_cb(Registers registers, byte value) => rlc(registers, value, false);

        public static byte rr(Registers registers, byte value) => rr(registers, value, true);
        public static byte rr_cb(Registers registers, byte value) => rr(registers, value, false);

        public static byte rrc(Registers registers, byte value) => rrc(registers, value, true);
        public static byte rrc_cb(Registers registers, byte value) => rrc(registers, value, false);

        private static byte add(Registers registers, byte a, byte b, bool isCarry, bool setCarry)
        {
            var cy = (registers.F.HasFlag(Flag.Carry) && isCarry).ToBit();
            var result = a + b + cy;
            var halfCarryResult = (a & 0xf) + (b & 0xf) + cy;
            var byteResult = (byte)result;

            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, halfCarryResult > 0xf);

            if (setCarry)
            {
                registers.SetFlag(Flag.Carry, result > 0xff);
            }

            return byteResult;
        }

        private static byte sub(Registers registers, byte a, byte b, bool isBorrow, bool setCarry)
        {
            var cy = (registers.F.HasFlag(Flag.Carry) && isBorrow).ToBit();
            var result = a - b - cy;
            var halfCarryResult = (a & 0xf) - (b & 0xf) - cy;
            var byteResult = (byte)result;

            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, true);
            registers.SetFlag(Flag.HalfCarry, halfCarryResult < 0);

            if (setCarry)
            {
                registers.SetFlag(Flag.Carry, result < 0);
            }

            return byteResult;
        }

        private static byte rl(Registers registers, byte value, bool clearZero)
        {
            var result = (byte)(value << 1 | registers.F.HasFlag(Flag.Carry).ToBit());

            registers.SetFlag(Flag.Carry, BitUtils.IsBitSet(value, 7));
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }

        private static byte rlc(Registers registers, byte value, bool clearZero)
        {
            var bit7 = BitUtils.BitValue(value, 7);
            var result = (byte)(value << 1 | bit7);

            registers.SetFlag(Flag.Carry, bit7 == 1);
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }

        internal static byte rr(Registers registers, byte value, bool clearZero)
        {
            var result = (byte)(value >> 1 | registers.F.HasFlag(Flag.Carry).ToBit() << 7);

            registers.SetFlag(Flag.Carry, BitUtils.IsBitSet(value, 0));
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }

        internal static byte rrc(Registers registers, byte value, bool clearZero)
        {
            var bit0 = BitUtils.BitValue(value, 0);
            var result = (byte)(value >> 1 | bit0 << 7);

            registers.SetFlag(Flag.Carry, bit0 == 1);
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }
    }
}
