using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core.Cpu
{
    internal static class AluOperations
    {
        internal static byte cp(Registers registers, byte a, byte b)
        {
            var result = a - b;
            var halfCarryResult = (a & 0xf) - (b & 0xf);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, true);
            registers.SetFlag(Flag.Carry, result < 0);
            registers.SetFlag(Flag.HalfCarry, halfCarryResult < 0);
            return a;
        }

        internal static byte or(Registers registers, byte a, byte b)
        {
            var result = a | b;
            var byteResult = (byte)result;
            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, false);
            return byteResult;
        }

        internal static byte xor(Registers registers, byte a, byte b)
        {
            var result = a ^ b;
            var byteResult = (byte)result;
            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, false);
            return byteResult;
        }

        internal static byte and(Registers registers, byte a, byte b)
        {
            var result = a & b;
            var byteResult = (byte)result;
            registers.SetFlag(Flag.Zero, byteResult == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, true);
            registers.SetFlag(Flag.Carry, false);
            return byteResult;
        }

        internal static byte add(Registers registers, byte a, byte b) => add(registers, a, b, false, true);

        internal static byte adc(Registers registers, byte a, byte b) => add(registers, a, b, true, true);

        internal static byte sub(Registers registers, byte a, byte b) => sub(registers, a, b, false, true);

        internal static byte sbc(Registers registers, byte a, byte b) => sub(registers, a, b, true, true);

        internal static byte inc(Registers registers, byte a) => add(registers, a, 1, false, false);

        internal static byte dec(Registers registers, byte a) => sub(registers, a, 1, false, false);

        internal static byte daa(Registers registers, byte a)
        {
            // Check the condition flags to determine the adjustment needed
            var carryFlag = registers.GetFlag(Flag.Carry);
            var halfCarryFlag = registers.GetFlag(Flag.HalfCarry);

            // Perform the DAA adjustment
            if (!registers.GetFlag(Flag.Subtract))
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

        internal static void ccf(Registers registers)
        {
            registers.ToggleFlag(Flag.Carry);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
        }

        internal static void scf(Registers registers)
        {
            registers.SetFlag(Flag.Carry, true);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
        }

        internal static byte cpl(Registers registers, byte value)
        {
            var result = ~value;
            registers.SetFlag(Flag.Subtract, true);
            registers.SetFlag(Flag.HalfCarry, true);
            return (byte)result;
        }

        internal static byte swap(Registers registers, byte value)
        {
            var result = value >> 4 | value << 4;
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, false);
            return (byte)result;
        }

        internal static byte bit(Registers registers, byte value, byte bitPosition)
        {
            var result = Utils.BitValue(value, bitPosition);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, true);
            return value;
        }

        internal static byte set(Registers registers, byte value, byte bitPosition) => Utils.SetBit(value, bitPosition);

        internal static byte res(Registers registers, byte value, byte bitPosition) => Utils.ClearBit(value, bitPosition);

        internal static byte sla(Registers registers, byte value)
        {
            var result = (byte)(value << 1);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, Utils.IsBitSet(value, 7));
            return result;
        }

        internal static byte sra(Registers registers, byte value)
        {
            var result = (byte)(value >> 1 | value & 0x80);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, Utils.IsBitSet(value, 0));
            return result;
        }

        internal static byte srl(Registers registers, byte value)
        {
            var result = (byte)(value >> 1 | 0 << 7);
            registers.SetFlag(Flag.Zero, result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);
            registers.SetFlag(Flag.Carry, Utils.IsBitSet(value, 0));
            return result;
        }

        internal static byte rl(Registers registers, byte value) => rl(registers, value, true);
        internal static byte rl_cb(Registers registers, byte value) => rl(registers, value, false);

        internal static byte rlc(Registers registers, byte value) => rlc(registers, value, true);
        internal static byte rlc_cb(Registers registers, byte value) => rlc(registers, value, false);

        internal static byte rr(Registers registers, byte value) => rr(registers, value, true);
        internal static byte rr_cb(Registers registers, byte value) => rr(registers, value, false);

        internal static byte rrc(Registers registers, byte value) => rrc(registers, value, true);
        internal static byte rrc_cb(Registers registers, byte value) => rrc(registers, value, false);

        private static byte add(Registers registers, byte a, byte b, bool isCarry, bool setCarry)
        {
            var cy = (registers.GetFlag(Flag.Carry) && isCarry).ToBit();
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
            var cy = (registers.GetFlag(Flag.Carry) && isBorrow).ToBit();
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
            var result = (byte)(value << 1 | registers.GetFlag(Flag.Carry).ToBit());

            registers.SetFlag(Flag.Carry, Utils.IsBitSet(value, 7));
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }

        private static byte rlc(Registers registers, byte value, bool clearZero)
        {
            var bit7 = Utils.BitValue(value, 7);
            var result = (byte)(value << 1 | bit7);

            registers.SetFlag(Flag.Carry, bit7 == 1);
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }

        internal static byte rr(Registers registers, byte value, bool clearZero)
        {
            var result = (byte)(value >> 1 | registers.GetFlag(Flag.Carry).ToBit() << 7);

            registers.SetFlag(Flag.Carry, Utils.IsBitSet(value, 0));
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }

        internal static byte rrc(Registers registers, byte value, bool clearZero)
        {
            var bit0 = Utils.BitValue(value, 0);
            var result = (byte)(value >> 1 | bit0 << 7);

            registers.SetFlag(Flag.Carry, bit0 == 1);
            registers.SetFlag(Flag.Zero, !clearZero && result == 0);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, false);

            return result;
        }
    }
}
