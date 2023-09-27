﻿using System;
using System.Collections.Generic;
using System.Text;
using SharpBoy.Core.Utilities;

namespace SharpBoy.Core.Processor
{
    internal static class AluOperations
    {
        public static byte cp(CpuRegisters registers, byte a, byte b)
        {
            var result = a - b;
            var halfCarryResult = (a & 0xf) - (b & 0xf);
            registers.SetFlag(Flags.Zero, result == 0);
            registers.SetFlag(Flags.Subtract, true);
            registers.SetFlag(Flags.Carry, result < 0);
            registers.SetFlag(Flags.HalfCarry, halfCarryResult < 0);
            return a;
        }

        public static byte or(CpuRegisters registers, byte a, byte b)
        {
            var result = a | b;
            var byteResult = (byte)result;
            registers.SetFlag(Flags.Zero, byteResult == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
            registers.SetFlag(Flags.Carry, false);
            return byteResult;
        }

        public static byte xor(CpuRegisters registers, byte a, byte b)
        {
            var result = a ^ b;
            var byteResult = (byte)result;
            registers.SetFlag(Flags.Zero, byteResult == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
            registers.SetFlag(Flags.Carry, false);
            return byteResult;
        }

        public static byte and(CpuRegisters registers, byte a, byte b)
        {
            var result = a & b;
            var byteResult = (byte)result;
            registers.SetFlag(Flags.Zero, byteResult == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, true);
            registers.SetFlag(Flags.Carry, false);
            return byteResult;
        }

        public static byte add(CpuRegisters registers, byte a, byte b) => add(registers, a, b, false, true);

        public static byte adc(CpuRegisters registers, byte a, byte b) => add(registers, a, b, true, true);

        public static byte sub(CpuRegisters registers, byte a, byte b) => sub(registers, a, b, false, true);

        public static byte sbc(CpuRegisters registers, byte a, byte b) => sub(registers, a, b, true, true);

        public static byte inc(CpuRegisters registers, byte a) => add(registers, a, 1, false, false);

        public static byte dec(CpuRegisters registers, byte a) => sub(registers, a, 1, false, false);

        public static byte daa(CpuRegisters registers, byte a)
        {
            // Check the condition flags to determine the adjustment needed
            var carryFlag = registers.F.HasFlag(Flags.Carry);
            var halfCarryFlag = registers.F.HasFlag(Flags.HalfCarry);

            // Perform the DAA adjustment
            if (!registers.F.HasFlag(Flags.Subtract))
            {
                if (carryFlag || a > 0x99)
                {
                    a += 0x60;
                    registers.SetFlag(Flags.Carry, true);
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
                    registers.SetFlag(Flags.Carry, true);
                }
                if (halfCarryFlag)
                {
                    a -= 0x06;
                }
            }

            // Update the zero flag, clear half carry flag
            registers.SetFlag(Flags.Zero, a == 0);
            registers.SetFlag(Flags.HalfCarry, false);

            return a;
        }

        public static void ccf(CpuRegisters registers)
        {
            registers.F ^= Flags.Carry;
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
        }

        public static void scf(CpuRegisters registers)
        {
            registers.SetFlag(Flags.Carry, true);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
        }

        public static byte cpl(CpuRegisters registers, byte value)
        {
            var result = ~value;
            registers.SetFlag(Flags.Subtract, true);
            registers.SetFlag(Flags.HalfCarry, true);
            return (byte)result;
        }

        public static byte swap(CpuRegisters registers, byte value)
        {
            var result = value >> 4 | value << 4;
            registers.SetFlag(Flags.Zero, result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
            registers.SetFlag(Flags.Carry, false);
            return (byte)result;
        }

        public static byte bit(CpuRegisters registers, byte value, byte bitPosition)
        {
            var result = BitUtils.BitValue(value, bitPosition);
            registers.SetFlag(Flags.Zero, result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, true);
            return value;
        }

        public static byte set(byte value, byte bitPosition) => BitUtils.SetBit(value, bitPosition);

        public static byte res(byte value, byte bitPosition) => BitUtils.ClearBit(value, bitPosition);

        public static byte sla(CpuRegisters registers, byte value)
        {
            var result = (byte)(value << 1);
            registers.SetFlag(Flags.Zero, result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
            registers.SetFlag(Flags.Carry, BitUtils.IsBitSet(value, 7));
            return result;
        }

        public static byte sra(CpuRegisters registers, byte value)
        {
            var result = (byte)(value >> 1 | value & 0x80);
            registers.SetFlag(Flags.Zero, result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
            registers.SetFlag(Flags.Carry, BitUtils.IsBitSet(value, 0));
            return result;
        }

        public static byte srl(CpuRegisters registers, byte value)
        {
            var result = (byte)(value >> 1 | 0 << 7);
            registers.SetFlag(Flags.Zero, result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);
            registers.SetFlag(Flags.Carry, BitUtils.IsBitSet(value, 0));
            return result;
        }

        public static byte rl(CpuRegisters registers, byte value) => rl(registers, value, true);
        public static byte rl_cb(CpuRegisters registers, byte value) => rl(registers, value, false);

        public static byte rlc(CpuRegisters registers, byte value) => rlc(registers, value, true);
        public static byte rlc_cb(CpuRegisters registers, byte value) => rlc(registers, value, false);

        public static byte rr(CpuRegisters registers, byte value) => rr(registers, value, true);
        public static byte rr_cb(CpuRegisters registers, byte value) => rr(registers, value, false);

        public static byte rrc(CpuRegisters registers, byte value) => rrc(registers, value, true);
        public static byte rrc_cb(CpuRegisters registers, byte value) => rrc(registers, value, false);

        private static byte add(CpuRegisters registers, byte a, byte b, bool isCarry, bool setCarry)
        {
            var cy = (registers.F.HasFlag(Flags.Carry) && isCarry).ToBit();
            var result = a + b + cy;
            var halfCarryResult = (a & 0xf) + (b & 0xf) + cy;
            var byteResult = (byte)result;

            registers.SetFlag(Flags.Zero, byteResult == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, halfCarryResult > 0xf);

            if (setCarry)
            {
                registers.SetFlag(Flags.Carry, result > 0xff);
            }

            return byteResult;
        }

        private static byte sub(CpuRegisters registers, byte a, byte b, bool isBorrow, bool setCarry)
        {
            var cy = (registers.F.HasFlag(Flags.Carry) && isBorrow).ToBit();
            var result = a - b - cy;
            var halfCarryResult = (a & 0xf) - (b & 0xf) - cy;
            var byteResult = (byte)result;

            registers.SetFlag(Flags.Zero, byteResult == 0);
            registers.SetFlag(Flags.Subtract, true);
            registers.SetFlag(Flags.HalfCarry, halfCarryResult < 0);

            if (setCarry)
            {
                registers.SetFlag(Flags.Carry, result < 0);
            }

            return byteResult;
        }

        private static byte rl(CpuRegisters registers, byte value, bool clearZero)
        {
            var result = (byte)(value << 1 | registers.F.HasFlag(Flags.Carry).ToBit());

            registers.SetFlag(Flags.Carry, BitUtils.IsBitSet(value, 7));
            registers.SetFlag(Flags.Zero, !clearZero && result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);

            return result;
        }

        private static byte rlc(CpuRegisters registers, byte value, bool clearZero)
        {
            var bit7 = BitUtils.BitValue(value, 7);
            var result = (byte)(value << 1 | bit7);

            registers.SetFlag(Flags.Carry, bit7 == 1);
            registers.SetFlag(Flags.Zero, !clearZero && result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);

            return result;
        }

        internal static byte rr(CpuRegisters registers, byte value, bool clearZero)
        {
            var result = (byte)(value >> 1 | registers.F.HasFlag(Flags.Carry).ToBit() << 7);

            registers.SetFlag(Flags.Carry, BitUtils.IsBitSet(value, 0));
            registers.SetFlag(Flags.Zero, !clearZero && result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);

            return result;
        }

        internal static byte rrc(CpuRegisters registers, byte value, bool clearZero)
        {
            var bit0 = BitUtils.BitValue(value, 0);
            var result = (byte)(value >> 1 | bit0 << 7);

            registers.SetFlag(Flags.Carry, bit0 == 1);
            registers.SetFlag(Flags.Zero, !clearZero && result == 0);
            registers.SetFlag(Flags.Subtract, false);
            registers.SetFlag(Flags.HalfCarry, false);

            return result;
        }
    }
}
