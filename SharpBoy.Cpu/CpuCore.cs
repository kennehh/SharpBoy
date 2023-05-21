using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Cpu
{
    public class CpuCore
    {
        public bool Halted { get; private set; }
        public bool Stopped { get; private set; }
        public bool InterruptsEnabled { get; private set; }

        internal Memory memory;
        internal Registers registers;
        internal bool branchTaken = false;

        private int currentCycles;

        public CpuCore(int memorySize) 
        { 
            memory = new Memory(memorySize);
            registers = new Registers();
        }

        public int Tick()
        {
            currentCycles = 0;
            branchTaken = false;

            var opcode = ReadImmediate8Bit();
            ExecuteInstruction(opcode);

            return currentCycles;
        }

        private byte ReadImmediate8Bit()
        {
            var val = Read8Bit(registers.PC);
            registers.PC += 1;
            return val;
        }

        private ushort ReadImmediate16Bit()
        {
            var val = Read16Bit(registers.PC);
            registers.PC += 2;
            return val;
        }

        private byte Read8Bit(ushort address)
        {
            var val = memory.Read8Bit(address);
            currentCycles += 4;
            return val;
        }

        private ushort Read16Bit(ushort address)
        {
            var val = memory.Read16Bit(address);
            currentCycles += 8;
            return val;
        }

        private ushort ReadRegister16Bit(Register16Bit register)
        {
            var val = registers.ReadFromRegister(register);
            currentCycles += 4;
            return val;
        }

        private ushort ReadRegisterPC()
        {
            var val = registers.PC;
            currentCycles += 4;
            return val;
        }

        private void Write8Bit(ushort address, byte value)
        {
            memory.Write8Bit(address, value);
            currentCycles += 4;
        }

        private void Write16Bit(ushort address, ushort value)
        {
            memory.Write16Bit(address, value);
            currentCycles += 8;
        }

        private void ExecuteInstruction(byte opcode)
        {
            switch (opcode)
            {
                case 0x00: nop(); break;
                case 0x01: ld_r16_i16(Register16Bit.BC); break;
                case 0x02: ld_ra16_a(Register16Bit.BC); break;
                case 0x03: inc_r16(Register16Bit.BC); break;
                case 0x04: inc_r8(Register8Bit.B); break;
                case 0x05: dec_r8(Register8Bit.B); break;
                case 0x06: ld_r8_i8(Register8Bit.B); break;
                case 0x07: rlca(); break;
                case 0x08: ld_a16_sp(); break;
                case 0x09: add_hl_r16(Register16Bit.BC); break;
                case 0x0a: ld_a_ra16(Register16Bit.BC); break;
                case 0x0b: dec_r16(Register16Bit.BC); break;
                case 0x0c: inc_r8(Register8Bit.C); break;
                case 0x0d: dec_r8(Register8Bit.C); break;
                case 0x0e: ld_r8_i8(Register8Bit.C); break;
                case 0x0f: rrca(); break;

                case 0x10: stop(); break;
                case 0x11: ld_r16_i16(Register16Bit.DE); break;
                case 0x12: ld_ra16_a(Register16Bit.DE); break;
                case 0x13: inc_r16(Register16Bit.DE); break;
                case 0x14: inc_r8(Register8Bit.D); break;
                case 0x15: dec_r8(Register8Bit.D); break;
                case 0x16: ld_r8_i8(Register8Bit.D); break;
                case 0x17: rla(); break;
                case 0x18: jr_i8(); break;
                case 0x19: add_hl_r16(Register16Bit.DE); break;
                case 0x1a: ld_a_ra16(Register16Bit.DE); break;
                case 0x1b: dec_r16(Register16Bit.DE); break;
                case 0x1c: inc_r8(Register8Bit.E); break;
                case 0x1d: dec_r8(Register8Bit.E); break;
                case 0x1e: ld_r8_i8(Register8Bit.E); break;
                case 0x1f: rra(); break;

                case 0x20: jr_i8(Flag.Zero, false); break;
                case 0x21: ld_r16_i16(Register16Bit.HL); break;
                case 0x22: ld_hl_a(1); break;
                case 0x23: inc_r16(Register16Bit.HL); break;
                case 0x24: inc_r8(Register8Bit.H); break;
                case 0x25: dec_r8(Register8Bit.H); break;
                case 0x26: ld_r8_i8(Register8Bit.H); break;
                case 0x27: daa(); break;
                case 0x28: jr_i8(Flag.Zero, true); break;
                case 0x29: add_hl_r16(Register16Bit.HL); break;
                case 0x2a: ld_a_hl(1); break;
                case 0x2b: dec_r16(Register16Bit.HL); break;
                case 0x2c: inc_r8(Register8Bit.L); break;
                case 0x2d: dec_r8(Register8Bit.L); break;
                case 0x2e: ld_r8_i8(Register8Bit.L); break;
                case 0x2f: cpl(); break;

                case 0x30: jr_i8(Flag.Carry, false); break;
                case 0x31: ld_r16_i16(Register16Bit.SP); break;
                case 0x32: ld_hl_a(-1); break;
                case 0x33: inc_r16(Register16Bit.SP); break;
                case 0x34: inc_hla(); break;
                case 0x35: dec_hla(); break;
                case 0x36: ld_hla_i8(); break;
                case 0x37: scf(); break;
                case 0x38: jr_i8(Flag.Carry, true); break;
                case 0x39: add_hl_r16(Register16Bit.SP); break;
                case 0x3a: ld_a_hl(-1); break;
                case 0x3b: dec_r16(Register16Bit.SP); break;
                case 0x3c: inc_r8(Register8Bit.A); break;
                case 0x3d: dec_r8(Register8Bit.A); break;
                case 0x3e: ld_r8_i8(Register8Bit.A); break;
                case 0x3f: ccf(); break;

                case 0x40: ld_r8_r8(Register8Bit.B, Register8Bit.B); break;
                case 0x41: ld_r8_r8(Register8Bit.B, Register8Bit.C); break;
                case 0x42: ld_r8_r8(Register8Bit.B, Register8Bit.D); break;
                case 0x43: ld_r8_r8(Register8Bit.B, Register8Bit.E); break;
                case 0x44: ld_r8_r8(Register8Bit.B, Register8Bit.H); break;
                case 0x45: ld_r8_r8(Register8Bit.B, Register8Bit.L); break;
                case 0x46: ld_r8_hla(Register8Bit.B); break;
                case 0x47: ld_r8_r8(Register8Bit.B, Register8Bit.A); break;
                case 0x48: ld_r8_r8(Register8Bit.C, Register8Bit.B); break;
                case 0x49: ld_r8_r8(Register8Bit.C, Register8Bit.C); break;
                case 0x4a: ld_r8_r8(Register8Bit.C, Register8Bit.D); break;
                case 0x4b: ld_r8_r8(Register8Bit.C, Register8Bit.E); break;
                case 0x4c: ld_r8_r8(Register8Bit.C, Register8Bit.H); break;
                case 0x4d: ld_r8_r8(Register8Bit.C, Register8Bit.L); break;
                case 0x4e: ld_r8_hla(Register8Bit.C); break;
                case 0x4f: ld_r8_r8(Register8Bit.C, Register8Bit.A); break;

                case 0x50: ld_r8_r8(Register8Bit.D, Register8Bit.B); break;
                case 0x51: ld_r8_r8(Register8Bit.D, Register8Bit.C); break;
                case 0x52: ld_r8_r8(Register8Bit.D, Register8Bit.D); break;
                case 0x53: ld_r8_r8(Register8Bit.D, Register8Bit.E); break;
                case 0x54: ld_r8_r8(Register8Bit.D, Register8Bit.H); break;
                case 0x55: ld_r8_r8(Register8Bit.D, Register8Bit.L); break;
                case 0x56: ld_r8_hla(Register8Bit.D); break;
                case 0x57: ld_r8_r8(Register8Bit.D, Register8Bit.A); break;
                case 0x58: ld_r8_r8(Register8Bit.E, Register8Bit.B); break;
                case 0x59: ld_r8_r8(Register8Bit.E, Register8Bit.C); break;
                case 0x5a: ld_r8_r8(Register8Bit.E, Register8Bit.D); break;
                case 0x5b: ld_r8_r8(Register8Bit.E, Register8Bit.E); break;
                case 0x5c: ld_r8_r8(Register8Bit.E, Register8Bit.H); break;
                case 0x5d: ld_r8_r8(Register8Bit.E, Register8Bit.L); break;
                case 0x5e: ld_r8_hla(Register8Bit.E); break;
                case 0x5f: ld_r8_r8(Register8Bit.E, Register8Bit.A); break;

                case 0x60: ld_r8_r8(Register8Bit.H, Register8Bit.B); break;
                case 0x61: ld_r8_r8(Register8Bit.H, Register8Bit.C); break;
                case 0x62: ld_r8_r8(Register8Bit.H, Register8Bit.D); break;
                case 0x63: ld_r8_r8(Register8Bit.H, Register8Bit.E); break;
                case 0x64: ld_r8_r8(Register8Bit.H, Register8Bit.H); break;
                case 0x65: ld_r8_r8(Register8Bit.H, Register8Bit.L); break;
                case 0x66: ld_r8_hla(Register8Bit.H); break;
                case 0x67: ld_r8_r8(Register8Bit.H, Register8Bit.A); break;
                case 0x68: ld_r8_r8(Register8Bit.L, Register8Bit.B); break;
                case 0x69: ld_r8_r8(Register8Bit.L, Register8Bit.C); break;
                case 0x6a: ld_r8_r8(Register8Bit.L, Register8Bit.D); break;
                case 0x6b: ld_r8_r8(Register8Bit.L, Register8Bit.E); break;
                case 0x6c: ld_r8_r8(Register8Bit.L, Register8Bit.H); break;
                case 0x6d: ld_r8_r8(Register8Bit.L, Register8Bit.L); break;
                case 0x6e: ld_r8_hla(Register8Bit.L); break;
                case 0x6f: ld_r8_r8(Register8Bit.L, Register8Bit.A); break;

                case 0x70: ld_hla_r8(Register8Bit.B); break;
                case 0x71: ld_hla_r8(Register8Bit.C); break;
                case 0x72: ld_hla_r8(Register8Bit.D); break;
                case 0x73: ld_hla_r8(Register8Bit.E); break;
                case 0x74: ld_hla_r8(Register8Bit.H); break;
                case 0x75: ld_hla_r8(Register8Bit.L); break;
                case 0x76: halt(); break;
                case 0x77: ld_hla_r8(Register8Bit.A); break;
                case 0x78: ld_r8_r8(Register8Bit.A, Register8Bit.B); break;
                case 0x79: ld_r8_r8(Register8Bit.A, Register8Bit.C); break;
                case 0x7a: ld_r8_r8(Register8Bit.A, Register8Bit.D); break;
                case 0x7b: ld_r8_r8(Register8Bit.A, Register8Bit.E); break;
                case 0x7c: ld_r8_r8(Register8Bit.A, Register8Bit.H); break;
                case 0x7d: ld_r8_r8(Register8Bit.A, Register8Bit.L); break;
                case 0x7e: ld_r8_hla(Register8Bit.A); break;
                case 0x7f: ld_r8_r8(Register8Bit.A, Register8Bit.A); break;

                case 0x80: add_a_r8(Register8Bit.B); break;
                case 0x81: add_a_r8(Register8Bit.C); break;
                case 0x82: add_a_r8(Register8Bit.D); break;
                case 0x83: add_a_r8(Register8Bit.E); break;
                case 0x84: add_a_r8(Register8Bit.H); break;
                case 0x85: add_a_r8(Register8Bit.L); break;
                case 0x86: add_a_hla(); break;              
                case 0x87: add_a_r8(Register8Bit.A); break;
                case 0x88: adc_a_r8(Register8Bit.B); break;
                case 0x89: adc_a_r8(Register8Bit.C); break;
                case 0x8a: adc_a_r8(Register8Bit.D); break;
                case 0x8b: adc_a_r8(Register8Bit.E); break;
                case 0x8c: adc_a_r8(Register8Bit.H); break;
                case 0x8d: adc_a_r8(Register8Bit.L); break;
                case 0x8e: adc_a_hla(); break;
                case 0x8f: adc_a_r8(Register8Bit.A); break;

                case 0x90: sub_a_r8(Register8Bit.B); break;
                case 0x91: sub_a_r8(Register8Bit.C); break;
                case 0x92: sub_a_r8(Register8Bit.D); break;
                case 0x93: sub_a_r8(Register8Bit.E); break;
                case 0x94: sub_a_r8(Register8Bit.H); break;
                case 0x95: sub_a_r8(Register8Bit.L); break;
                case 0x96: sub_a_hla(); break;
                case 0x97: sub_a_r8(Register8Bit.A); break;
                case 0x98: sbc_a_r8(Register8Bit.B); break;
                case 0x99: sbc_a_r8(Register8Bit.C); break;
                case 0x9a: sbc_a_r8(Register8Bit.D); break;
                case 0x9b: sbc_a_r8(Register8Bit.E); break;
                case 0x9c: sbc_a_r8(Register8Bit.H); break;
                case 0x9d: sbc_a_r8(Register8Bit.L); break;
                case 0x9e: sbc_a_hla(); break;
                case 0x9f: sbc_a_r8(Register8Bit.A); break;

                case 0xa0: and_a_r8(Register8Bit.B); break;
                case 0xa1: and_a_r8(Register8Bit.C); break;
                case 0xa2: and_a_r8(Register8Bit.D); break;
                case 0xa3: and_a_r8(Register8Bit.E); break;
                case 0xa4: and_a_r8(Register8Bit.H); break;
                case 0xa5: and_a_r8(Register8Bit.L); break;
                case 0xa6: and_a_hla(); break;
                case 0xa7: and_a_r8(Register8Bit.A); break;
                case 0xa8: xor_a_r8(Register8Bit.B); break;
                case 0xa9: xor_a_r8(Register8Bit.C); break;
                case 0xaa: xor_a_r8(Register8Bit.D); break;
                case 0xab: xor_a_r8(Register8Bit.E); break;
                case 0xac: xor_a_r8(Register8Bit.H); break;
                case 0xad: xor_a_r8(Register8Bit.L); break;
                case 0xae: xor_a_hla(); break;
                case 0xaf: xor_a_r8(Register8Bit.A); break;

                case 0xb0: or_a_r8(Register8Bit.B); break;
                case 0xb1: or_a_r8(Register8Bit.C); break;
                case 0xb2: or_a_r8(Register8Bit.D); break;
                case 0xb3: or_a_r8(Register8Bit.E); break;
                case 0xb4: or_a_r8(Register8Bit.H); break;
                case 0xb5: or_a_r8(Register8Bit.L); break;
                case 0xb6: or_a_hla(); break;
                case 0xb7: or_a_r8(Register8Bit.A); break;
                case 0xb8: cp_a_r8(Register8Bit.B); break;
                case 0xb9: cp_a_r8(Register8Bit.C); break;
                case 0xba: cp_a_r8(Register8Bit.D); break;
                case 0xbb: cp_a_r8(Register8Bit.E); break;
                case 0xbc: cp_a_r8(Register8Bit.H); break;
                case 0xbd: cp_a_r8(Register8Bit.L); break;
                case 0xbe: cp_a_hla(); break;
                case 0xbf: cp_a_r8(Register8Bit.A); break;

                case 0xc6: add_a_i8(); break;
                case 0xce: adc_a_i8(); break;
                default:
                    throw new ArgumentException($"Unknown opcode: 0x{opcode:x2}", nameof(opcode));
            }

        }

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
            currentCycles += 4;
        }

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

#endregion

#region rl

        private void rla()
        {
            registers.A = AluOperations.rl(registers, registers.A);
        }

        private void rlca()
        {
            registers.A = AluOperations.rlc(registers, registers.A);
        }

        private void rl(Register8Bit reg)
        {
            byte value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rl(registers, value));
        }

        private void rlc(Register8Bit reg)
        {
            byte value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rlc(registers, value));
        }

#endregion

#region rr

        private void rra()
        {
            registers.A = AluOperations.rr(registers, registers.A);
        }

        private void rrca()
        {
            registers.A = AluOperations.rrc(registers, registers.A);
        }

        private void rr(Register8Bit reg)
        {
            var value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rr(registers, value));
        }

        private void rrc(Register8Bit reg)
        {
            byte value = registers.ReadFromRegister(reg);
            registers.WriteToRegister(reg, AluOperations.rr(registers, value));
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

        private void and_a(byte value) => registers.A = AluOperations.and(registers, registers.A, value);

#endregion

#region xor

        private void xor_a_r8(Register8Bit reg) => xor_a(registers.ReadFromRegister(reg));

        private void xor_a_r8() => xor_a(ReadImmediate8Bit());

        private void xor_a_hla() => xor_a(Read8Bit(registers.HL));

        private void xor_a(byte value) => registers.A = AluOperations.xor(registers, registers.A, value);

        

#endregion

#region or

        private void or_a_r8(Register8Bit reg) => or_a(registers.ReadFromRegister(reg));

        private void or_a_r8() => or_a(ReadImmediate8Bit());

        private void or_a_hla() => or_a(Read8Bit(registers.HL));

        private void or_a(byte value) => registers.A = AluOperations.or(registers, registers.A, value);

#endregion

#region cp
        private void cp_a_r8(Register8Bit reg) => cp_a(registers.ReadFromRegister(reg));

        private void cp_a_i8() => cp_a(ReadImmediate8Bit());

        private void cp_a_hla() => cp_a(Read8Bit(registers.HL));

        private void cp_a(byte val) => AluOperations.cp(registers, registers.A, val);

        #endregion

        #region daa

        private void daa() => registers.A = AluOperations.daa(registers, registers.A);

#endregion

#region ccf/scf/cpl

        private void ccf() => AluOperations.ccf(registers);

        private void scf() => AluOperations.scf(registers);

        private void cpl()
        {
            var result = ~registers.A;
            registers.SetFlag(Flag.Subtract, true);
            registers.SetFlag(Flag.HalfCarry, true);
            registers.A = (byte)result;
        }

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

#endregion

#region stack

        private void push(Register16Bit reg) => push(registers.ReadFromRegister(reg));

        private void push(ushort value)
        {
            Write16Bit(registers.SP, value);
            registers.SP -= 2;
            currentCycles += 4;
        }

        private void pop(Register16Bit reg) => registers.WriteToRegister(reg, pop());

        ushort pop()
        {
            var value = Read16Bit(registers.SP);
            registers.SP += 2;
            return value;
        }

#endregion

    }
}
