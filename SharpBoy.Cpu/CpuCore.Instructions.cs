using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Cpu
{
    public partial class CpuCore
    {

        #region misc

        private void nop() { }

        private void halt()
        {
            Halted = true;
        }

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
        private void rl_r8(Register8Bit reg)
        {
            byte value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rl_cb(registers, value));
        }

        private void rlc_r8(Register8Bit reg)
        {
            byte value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rlc_cb(registers, value));
        }

        private void rl_hla()
        {
            byte value = Read8Bit(registers.HL);
            Write8Bit(registers.HL, AluOperations.rl_cb(registers, value));
        }
        private void rlc_hla()
        {
            byte value = Read8Bit(registers.HL);
            Write8Bit(registers.HL, AluOperations.rlc_cb(registers, value));
        }

        #endregion

        #region rr

        private void rra() => registers.A = AluOperations.rr(registers, registers.A);

        private void rrca() => registers.A = AluOperations.rrc(registers, registers.A);

        private void rr_r8(Register8Bit reg)
        {
            var value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rr_cb(registers, value));
        }

        private void rrc_r8(Register8Bit reg)
        {
            byte value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rrc_cb(registers, value));
        }

        private void rr_hla()
        {
            byte value = Read8Bit(registers.HL);
            Write8Bit(registers.HL, AluOperations.rr_cb(registers, value));
        }
        private void rrc_hla()
        {
            byte value = Read8Bit(registers.HL);
            Write8Bit(registers.HL, AluOperations.rrc_cb(registers, value));
        }

        #endregion

        #region sla/sra/srl

        private void sla_r8(Register8Bit reg)
        {
            var value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.sla(registers, value));
        }
        private void sla_hla()
        {
            var value = Read8Bit(registers.HL);
            Write8Bit(registers.HL, AluOperations.sla(registers, value));
        }

        private void sra_r8(Register8Bit reg)
        {
            var value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.sra(registers, value));
        }
        private void sra_hla()
        {
            var value = Read8Bit(registers.HL);
            Write8Bit(registers.HL, AluOperations.sra(registers, value));
        }

        private void srl_r8(Register8Bit reg)
        {
            var value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.srl(registers, value));
        }
        private void srl_hla()
        {
            var value = Read8Bit(registers.HL);
            Write8Bit(registers.HL, AluOperations.srl(registers, value));
        }


        #endregion

        #region add
        private void add_a_r8(Register8Bit reg) => add_a(registers.ReadFromRegister(reg));

        private void add_a_i8() => add_a(ReadImmediate8Bit());

        private void add_a_hla() => add_a(Read8Bit(registers.HL));

        private void add_a(byte val) => registers.A = AluOperations.add(registers, registers.A, val);

        private void adc_a_r8(Register8Bit reg) => adc_a(registers.ReadFromRegister(reg));

        private void adc_a_i8() => adc_a(ReadImmediate8Bit());

        private void adc_a_hla() => adc_a(Read8Bit(registers.HL));

        private void adc_a(byte val) => registers.A = AluOperations.adc(registers, registers.A, val);

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
        private void sub_a_r8(Register8Bit reg) => sub_a(registers.ReadFromRegister(reg));

        private void sub_a_i8() => sub_a(ReadImmediate8Bit());

        private void sub_a_hla() => sub_a(Read8Bit(registers.HL));

        private void sub_a(byte val) => registers.A = AluOperations.sub(registers, registers.A, val);

        private void sbc_a_r8(Register8Bit reg) => sbc_a(registers.ReadFromRegister(reg));

        private void sbc_a_i8() => sbc_a(ReadImmediate8Bit());

        private void sbc_a_hla() => sbc_a(Read8Bit(registers.HL));

        private void sbc_a(byte val) => registers.A = AluOperations.sbc(registers, registers.A, val);

        #endregion

        #region and

        private void and_a_r8(Register8Bit reg) => and_a(registers.ReadFromRegister(reg));

        private void and_a_r8() => and_a(ReadImmediate8Bit());

        private void and_a_hla() => and_a(Read8Bit(registers.HL));

        private void and_a_i8() => and_a(ReadImmediate8Bit());

        private void and_a(byte value) => registers.A = AluOperations.and(registers, registers.A, value);

        #endregion

        #region xor

        private void xor_a_r8(Register8Bit reg) => xor_a(registers.ReadFromRegister(reg));

        private void xor_a_r8() => xor_a(ReadImmediate8Bit());

        private void xor_a_hla() => xor_a(Read8Bit(registers.HL));

        private void xor_a_i8() => xor_a(ReadImmediate8Bit());

        private void xor_a(byte value) => registers.A = AluOperations.xor(registers, registers.A, value);



        #endregion

        #region or

        private void or_a_r8(Register8Bit reg) => or_a(registers.ReadFromRegister(reg));

        private void or_a_r8() => or_a(ReadImmediate8Bit());

        private void or_a_hla() => or_a(Read8Bit(registers.HL));

        private void or_a_i8() => or_a(ReadImmediate8Bit());

        private void or_a(byte value) => registers.A = AluOperations.or(registers, registers.A, value);

        #endregion

        #region cp
        private void cp_a_r8(Register8Bit reg) => cp_a(registers.ReadFromRegister(reg));

        private void cp_a_hla() => cp_a(Read8Bit(registers.HL));

        private void cp_a_i8() => cp_a(ReadImmediate8Bit());

        private void cp_a(byte val) => AluOperations.cp(registers, registers.A, val);

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


    }
}
