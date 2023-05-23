using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SharpBoy.Core
{
    public partial class Cpu
    {

        #region misc

        private void nop() { }
        private void halt() => Halted = true;
        private void stop()
        {
            Stopped = true;
            registers.PC++;
        }

        private void di() => InterruptsEnabled = false;
        private void ei() => InterruptsEnabled = true;

        #endregion

        #region inc/dec

        private void inc_r16(Register16Bit reg)
        {
            var value = (ushort)(ReadRegister16Bit(reg) + 1);
            registers.WriteToRegister(reg, value);
        }

        private void inc_r8(Register8Bit reg)
        {
            var result = AluOperations.inc(registers, registers.ReadFromRegister(reg));
            registers.WriteToRegister(reg, result);
        }

        private void inc_hla()
        {
            var result = AluOperations.inc(registers, Read8Bit(registers.HL));
            Write8Bit(registers.HL, result);
        }

        private void dec_r16(Register16Bit reg)
        {
            var value = (ushort)(ReadRegister16Bit(reg) - 1);
            registers.WriteToRegister(reg, value);
        }

        private void dec_r8(Register8Bit reg)
        {
            var result = AluOperations.dec(registers, registers.ReadFromRegister(reg));
            registers.WriteToRegister(reg, result);
        }

        private void dec_hla()
        {
            var result = AluOperations.dec(registers, Read8Bit(registers.HL));
            Write8Bit(registers.HL, result);
        }

        #endregion

        #region ld
        private void ld_r8_r8(Register8Bit a, Register8Bit b)
        {
            var value = registers.ReadFromRegister(b);
            registers.WriteToRegister(a, value);
        }

        private void ld_r8_hla(Register8Bit reg)
        {
            var value = Read8Bit(registers.HL);
            registers.WriteToRegister(reg, value);
        }

        private void ld_hla_r8(Register8Bit reg)
        {
            var value = registers.ReadFromRegister(reg);
            Write8Bit(registers.HL, value);
        }

        private void ld_hla_i8()
        {
            var value = ReadImmediate8Bit();
            Write8Bit(registers.HL, value);
        }

        private void ld_ia8_a()
        {
            var address = (ushort)(0xff00 | ReadImmediate8Bit());
            Write8Bit(address, registers.A);
        }

        private void ld_a_ia8()
        {
            var address = (ushort)(0xff00 | ReadImmediate8Bit());
            registers.A = Read8Bit(address);
        }

        private void ld_ca_a()
        {
            var address = (ushort)(0xff00 | registers.C);
            Write8Bit(address, registers.A);
        }

        private void ld_a_ca()
        {
            var address = (ushort)(0xff00 | registers.C);
            registers.A = Read8Bit(address);
        }

        private void ld_r16_i16(Register16Bit reg)
        {
            var value = ReadImmediate16Bit();
            registers.WriteToRegister(reg, value);
        }

        private void ld_ra16_a(Register16Bit reg)
        {
            var address = registers.ReadFromRegister(reg);
            Write8Bit(address, registers.A);
        }

        private void ld_a16_sp() => Write16Bit(ReadImmediate16Bit(), registers.SP);

        private void ld_hl_a(int increment)
        {
            Write8Bit(registers.HL, registers.A);
            registers.HL += (ushort)increment;
        }

        private void ld_r8_i8(Register8Bit reg)
        {
            var value = ReadImmediate8Bit();
            registers.WriteToRegister(reg, value);
        }

        private void ld_a_ra16(Register16Bit reg)
        {
            var address = registers.ReadFromRegister(reg);
            var value = Read8Bit(address);
            registers.A = value;
        }

        private void ld_a_hl(int increment)
        {
            registers.A = Read8Bit(registers.HL);
            registers.HL += (ushort)increment;
        }

        private void ld_ia16_a()
        {
            var address = ReadImmediate16Bit();
            Write8Bit(address, registers.A);
        }

        private void ld_a_ia16()
        {
            var address = ReadImmediate16Bit();
            registers.A = Read8Bit(address);
        }

        private void ld_hl_spi8() => WriteRegisterHL(sp_i8());

        private void ld_sp_hl() => registers.SP = ReadRegisterHL();
        #endregion

        #region rl

        private void rla() => registers.A = AluOperations.rl(registers, registers.A);
        private void rlca() => registers.A = AluOperations.rlc(registers, registers.A);
        private void rl(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rl_cb);
        private void rlc(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rlc_cb);

        #endregion

        #region rr

        private void rra() => registers.A = AluOperations.rr(registers, registers.A);
        private void rrca() => registers.A = AluOperations.rrc(registers, registers.A);
        private void rr(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rr_cb);
        private void rrc(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rrc_cb);

        #endregion

        #region sla/sra/srl

        private void sla(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.sla);
        private void sra(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.sra);
        private void srl(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.srl);

        #endregion

        #region add
        private void add_a(Operand8Bit operand) => registers.A = AluOperations.add(registers, registers.A, ReadValue(operand));
        private void adc_a(Operand8Bit operand) => registers.A = AluOperations.adc(registers, registers.A, ReadValue(operand));

        private void add_hl_r16(Register16Bit reg)
        {
            // https://stackoverflow.com/questions/57958631/game-boy-half-carry-flag-and-16-bit-instructions-especially-opcode-0xe8
            // ADD HL,rr - "Based on my testing, H is set if carry occurs from bit 11 to bit 12."

            var value = ReadRegister16Bit(reg);
            var result = registers.HL + value;
            var halfCarryResult = (registers.HL & 0xfff) + (value & 0xfff);

            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, halfCarryResult > 0xfff);
            registers.SetFlag(Flag.Carry, result > 0xffff);

            registers.HL = (ushort)result;
        }

        private void add_sp_i8()
        {
            // https://stackoverflow.com/questions/57958631/game-boy-half-carry-flag-and-16-bit-instructions-especially-opcode-0xe8
            // TL; DR: For ADD SP,n, the H-flag is set when carry occurs from bit 3 to bit 4.

            var value = (sbyte)ReadImmediate8Bit();
            var result = registers.SP + value;
            var carryResult = (registers.SP & 0xff) + (value & 0xff);
            var halfCarryResult = (registers.SP & 0xf) + (value & 0xf);

            registers.SetFlag(Flag.Zero, false);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, halfCarryResult > 0xf);
            registers.SetFlag(Flag.Carry, carryResult > 0xff);

            WriteRegisterSP((ushort)result);
        }

        private ushort sp_i8()
        {
            // https://stackoverflow.com/questions/57958631/game-boy-half-carry-flag-and-16-bit-instructions-especially-opcode-0xe8
            // TL; DR: For ADD SP,n, the H-flag is set when carry occurs from bit 3 to bit 4.

            var value = (sbyte)ReadImmediate8Bit();
            var result = registers.SP + value;
            var carryResult = (registers.SP & 0xff) + (value & 0xff);
            var halfCarryResult = (registers.SP & 0xf) + (value & 0xf);

            registers.SetFlag(Flag.Zero, false);
            registers.SetFlag(Flag.Subtract, false);
            registers.SetFlag(Flag.HalfCarry, halfCarryResult > 0xf);
            registers.SetFlag(Flag.Carry, carryResult > 0xff);

            return (ushort)result;
        }

        #endregion

        #region sub
        private void sub_a(Operand8Bit operand) => registers.A = AluOperations.sub(registers, registers.A, ReadValue(operand));
        private void sbc_a(Operand8Bit operand) => registers.A = AluOperations.sbc(registers, registers.A, ReadValue(operand));

        #endregion

        #region bitwise ops
        private void and_a(Operand8Bit operand) => registers.A = AluOperations.and(registers, registers.A, ReadValue(operand));

        private void xor_a(Operand8Bit operand) => registers.A = AluOperations.xor(registers, registers.A, ReadValue(operand));

        private void or_a(Operand8Bit operand) => registers.A = AluOperations.or(registers, registers.A, ReadValue(operand));

        private void cp_a(Operand8Bit operand) => registers.A = AluOperations.cp(registers, registers.A, ReadValue(operand));
        #endregion

        #region daa

        private void daa() => registers.A = AluOperations.daa(registers, registers.A);

        #endregion

        #region ccf/scf/cpl

        private void ccf() => AluOperations.ccf(registers);

        private void scf() => AluOperations.scf(registers);

        private void cpl() => registers.A = AluOperations.cpl(registers, registers.A);

        #endregion

        #region jump

        private void jr_i8()
        {
            var increment = (sbyte)ReadImmediate8Bit();
            registers.PC = (ushort)(ReadRegisterPC() + increment);
            branchTaken = true;
        }

        private void jr_i8(Flag flag, bool isSet)
        {
            var val = (sbyte)ReadImmediate8Bit();
            if (registers.GetFlag(flag) == isSet)
            {
                registers.PC = (ushort)(ReadRegisterPC() + val);
                branchTaken = true;
            }
        }

        private void jp_i16()
        {
            registers.PC = ReadImmediate16Bit();
            currentCycles += 4;
        }

        private void jp_hl() => registers.PC = registers.HL;

        private void jp_i16(Flag flag, bool isSet)
        {
            ushort pc = ReadImmediate16Bit();
            if (registers.GetFlag(flag) == isSet)
            {
                registers.PC = pc;
                currentCycles += 4;
                branchTaken = true;
            }
        }

        private void call_i16()
        {
            ushort pc = ReadImmediate16Bit();
            push(registers.PC);
            registers.PC = pc;
        }

        private void call_i16(Flag flag, bool isSet)
        {
            ushort pc = ReadImmediate16Bit();

            if (registers.GetFlag(flag) == isSet)
            {
                push(registers.PC);
                registers.PC = pc;
                branchTaken = true;
            }
        }

        private void ret()
        {
            registers.PC = pop();
            currentCycles += 4;
        }

        private void ret(Flag flag, bool isSet)
        {
            currentCycles += 4;
            if (registers.GetFlag(flag) == isSet)
            {
                registers.PC = pop();
                currentCycles += 4;
                branchTaken = true;
            }
        }

        private void rst(byte address)
        {
            push(registers.PC);
            registers.PC = address;
        }

        #endregion

        #region stack

        private void push(Register16Bit reg) => push(registers.ReadFromRegister(reg));

        private void push(ushort value)
        {
            registers.SP -= 2;
            Write16Bit(registers.SP, value);
            currentCycles += 4;
        }

        private void pop(Register16Bit reg) => registers.WriteToRegister(reg, pop());

        private void pop_af() => registers.AF = (ushort)(pop() & 0xfff0); // ensure the low nibble is cleared for F 

        ushort pop()
        {
            var value = Read16Bit(registers.SP);
            registers.SP += 2;
            return value;
        }

        #endregion

        #region bit/res/set

        private void bit(Operand8Bit operand, int bit)
        {
            AluOperations.bit(registers, ReadValue(operand), (byte)bit);
            if (operand == Operand8Bit.IndirectHL)
            {
                currentCycles += 4;
            }
        }

        private void res(Operand8Bit operand, int bit) => ReadWriteValue(operand, (byte)bit, AluOperations.res);
        private void set(Operand8Bit operand, int bit) => ReadWriteValue(operand, (byte)bit, AluOperations.set);

        #endregion

        #region swap
        private void swap(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.swap);
        #endregion

        private byte ReadValue(Operand8Bit operand)
        {
            return operand switch
            {
                Operand8Bit.B => registers.B,
                Operand8Bit.C => registers.C,
                Operand8Bit.D => registers.D,
                Operand8Bit.E => registers.E,
                Operand8Bit.H => registers.H,
                Operand8Bit.L => registers.L,
                Operand8Bit.IndirectHL => Read8Bit(registers.HL),
                Operand8Bit.A => registers.A,
                Operand8Bit.Immediate => ReadImmediate8Bit()
            };
        }

        private void WriteValue(Operand8Bit operand, byte value)
        {
            switch (operand)
            {                
                case Operand8Bit.B: registers.B = value; break;
                case Operand8Bit.C: registers.C = value; break;
                case Operand8Bit.D: registers.D = value; break;
                case Operand8Bit.E: registers.E = value; break;
                case Operand8Bit.H: registers.H = value; break;
                case Operand8Bit.L: registers.L = value; break;
                case Operand8Bit.IndirectHL: Write8Bit(registers.HL, value); break;
                case Operand8Bit.A: registers.A = value; break;
                case Operand8Bit.Immediate: throw new ArgumentException();
            };
        }

        private void ReadWriteValue(Operand8Bit operand, Func<Registers, byte, byte> func)
        {
            WriteValue(operand, func(registers, ReadValue(operand)));
        }

        private void ReadWriteValue(Operand8Bit operand, byte value2, Func<Registers, byte, byte, byte> func)
        {
            WriteValue(operand, func(registers, ReadValue(operand), value2));
        }
    }

    internal enum Operand8Bit
    {
        B = 0,
        C = 1,
        D = 2,
        E = 3,
        H = 4,
        L = 5,
        IndirectHL = 6,
        A = 7,
        IndirectBC = 8,
        IndirectDE = 9,        
        Immediate = 10
    }
}