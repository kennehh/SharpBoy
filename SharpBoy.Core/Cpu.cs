using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace SharpBoy.Core
{
    public partial class Cpu
    {
        public bool Halted { get; private set; }
        public bool Stopped { get; private set; }
        public bool InterruptsEnabled { get; private set; }

        internal Memory memory;
        internal Registers registers;
        internal bool branchTaken = false;

        private int currentCycles;

        public Cpu(int memorySize) 
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

        private ushort ReadRegisterHL()
        {
            var val = registers.HL;
            currentCycles += 4;
            return val;
        }

        private void WriteRegisterSP(ushort value)
        {
            registers.SP = value;
            currentCycles += 8;
        }

        private void WriteRegisterHL(ushort value)
        {
            registers.HL = value;
            currentCycles += 4;
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

                case 0xc0: ret(Flag.Zero, false); break;
                case 0xc1: pop(Register16Bit.BC); break;
                case 0xc2: jp_i16(Flag.Zero, false); break;
                case 0xc3: jp_i16(); break;
                case 0xc4: call_i16(Flag.Zero, false); break;
                case 0xc5: push(Register16Bit.BC); break;
                case 0xc6: add_a_i8(); break;
                case 0xc7: rst(0x00); break;
                case 0xc8: ret(Flag.Zero, true); break;
                case 0xc9: ret(); break;
                case 0xca: jp_i16(Flag.Zero, true); break;
                case 0xcb: ExecuteCBInstruction(ReadImmediate8Bit()); break;
                case 0xcc: call_i16(Flag.Zero, true); break;
                case 0xcd: call_i16(); break;
                case 0xce: adc_a_i8(); break;
                case 0xcf: rst(0x08); break;

                case 0xd0: ret(Flag.Carry, false); break;
                case 0xd1: pop(Register16Bit.DE); break;
                case 0xd2: jp_i16(Flag.Carry, false); break;
                // no 0xd3
                case 0xd4: call_i16(Flag.Carry, false); break;
                case 0xd5: push(Register16Bit.DE); break;
                case 0xd6: sub_a_i8(); break;
                case 0xd7: rst(0x10); break;
                case 0xd8: ret(Flag.Carry, true); break;
                case 0xd9: ret(); break;
                case 0xda: jp_i16(Flag.Carry, true); break;
                // no 0xdb
                case 0xdc: call_i16(Flag.Carry, true); break;
                // no 0xdd
                case 0xde: sbc_a_i8(); break;
                case 0xdf: rst(0x18); break;

                case 0xe0: ld_ia8_a(); break;
                case 0xe1: pop(Register16Bit.HL); break;
                case 0xe2: ld_ca_a(); break;
                // no 0xe3
                // no 0xe4
                case 0xe5: push(Register16Bit.HL); break;
                case 0xe6: and_a_i8(); break;
                case 0xe7: rst(0x20); break;
                case 0xe8: add_sp_i8(); break;
                case 0xe9: jp_hl(); break;
                case 0xea: ld_ia16_a(); break;
                // no 0xeb
                // no 0xec
                // no 0xed
                case 0xee: xor_a_i8(); break;
                case 0xef: rst(0x28); break;

                case 0xf0: ld_a_ia8(); break;
                case 0xf1: pop_af(); break;
                case 0xf2: ld_a_ca(); break;
                case 0xf3: di(); break;
                // no 0xf4
                case 0xf5: push(Register16Bit.AF); break;
                case 0xf6: or_a_i8(); break;
                case 0xf7: rst(0x30); break;
                case 0xf8: ld_hl_spi8(); break;
                case 0xf9: ld_sp_hl(); break;
                case 0xfa: ld_a_ia16(); break;
                case 0xfb: ei(); break;
                // no 0xfc
                // no 0xfd
                case 0xfe: cp_a_i8(); break;
                case 0xff: rst(0x38); break;


                default:
                    throw new ArgumentException($"Unknown opcode: 0x{opcode:x2}", nameof(opcode));
            }

        }

        private void ExecuteCBInstruction(byte cbOpcode)
        {
            switch (cbOpcode)
            {
                case 0x00: rlc_r8(Register8Bit.B); break;
                case 0x01: rlc_r8(Register8Bit.C); break;
                case 0x02: rlc_r8(Register8Bit.D); break;
                case 0x03: rlc_r8(Register8Bit.E); break;
                case 0x04: rlc_r8(Register8Bit.H); break;
                case 0x05: rlc_r8(Register8Bit.L); break;
                case 0x06: rlc_hla(); break;
                case 0x07: rlc_r8(Register8Bit.A); break;
                case 0x08: rrc_r8(Register8Bit.B); break;
                case 0x09: rrc_r8(Register8Bit.C); break;
                case 0x0a: rrc_r8(Register8Bit.D); break;
                case 0x0b: rrc_r8(Register8Bit.E); break;
                case 0x0c: rrc_r8(Register8Bit.H); break;
                case 0x0d: rrc_r8(Register8Bit.L); break;
                case 0x0e: rrc_hla(); break;
                case 0x0f: rrc_r8(Register8Bit.A); break;

                case 0x10: rl_r8(Register8Bit.B); break;
                case 0x11: rl_r8(Register8Bit.C); break;
                case 0x12: rl_r8(Register8Bit.D); break;
                case 0x13: rl_r8(Register8Bit.E); break;
                case 0x14: rl_r8(Register8Bit.H); break;
                case 0x15: rl_r8(Register8Bit.L); break;
                case 0x16: rl_hla(); break;
                case 0x17: rl_r8(Register8Bit.A); break;
                case 0x18: rr_r8(Register8Bit.B); break;
                case 0x19: rr_r8(Register8Bit.C); break;
                case 0x1a: rr_r8(Register8Bit.D); break;
                case 0x1b: rr_r8(Register8Bit.E); break;
                case 0x1c: rr_r8(Register8Bit.H); break;
                case 0x1d: rr_r8(Register8Bit.L); break;
                case 0x1e: rr_hla(); break;
                case 0x1f: rr_r8(Register8Bit.A); break;

                case 0x20: sla_r8(Register8Bit.B); break;
                case 0x21: sla_r8(Register8Bit.C); break;
                case 0x22: sla_r8(Register8Bit.D); break;
                case 0x23: sla_r8(Register8Bit.E); break;
                case 0x24: sla_r8(Register8Bit.H); break;
                case 0x25: sla_r8(Register8Bit.L); break;
                case 0x26: sla_hla(); break;
                case 0x27: sla_r8(Register8Bit.A); break;
                case 0x28: sra_r8(Register8Bit.B); break;
                case 0x29: sra_r8(Register8Bit.C); break;
                case 0x2a: sra_r8(Register8Bit.D); break;
                case 0x2b: sra_r8(Register8Bit.E); break;
                case 0x2c: sra_r8(Register8Bit.H); break;
                case 0x2d: sra_r8(Register8Bit.L); break;
                case 0x2e: sra_hla(); break;
                case 0x2f: sra_r8(Register8Bit.A); break;

                case 0x30: swap_r8(Register8Bit.B); break;
                case 0x31: swap_r8(Register8Bit.C); break;
                case 0x32: swap_r8(Register8Bit.D); break;
                case 0x33: swap_r8(Register8Bit.E); break;
                case 0x34: swap_r8(Register8Bit.H); break;
                case 0x35: swap_r8(Register8Bit.L); break;
                case 0x36: swap_hla(); break;
                case 0x37: swap_r8(Register8Bit.A); break;
                case 0x38: srl_r8(Register8Bit.B); break;
                case 0x39: srl_r8(Register8Bit.C); break;
                case 0x3a: srl_r8(Register8Bit.D); break;
                case 0x3b: srl_r8(Register8Bit.E); break;
                case 0x3c: srl_r8(Register8Bit.H); break;
                case 0x3d: srl_r8(Register8Bit.L); break;
                case 0x3e: srl_hla(); break;
                case 0x3f: srl_r8(Register8Bit.A); break;

                case 0x40: bit_r8(Register8Bit.B, 0); break;
                case 0x41: bit_r8(Register8Bit.C, 0); break;
                case 0x42: bit_r8(Register8Bit.D, 0); break;
                case 0x43: bit_r8(Register8Bit.E, 0); break;
                case 0x44: bit_r8(Register8Bit.H, 0); break;
                case 0x45: bit_r8(Register8Bit.L, 0); break;
                case 0x46: bit_hla(0); break;
                case 0x47: bit_r8(Register8Bit.A, 0); break;
                case 0x48: bit_r8(Register8Bit.B, 1); break;
                case 0x49: bit_r8(Register8Bit.C, 1); break;
                case 0x4a: bit_r8(Register8Bit.D, 1); break;
                case 0x4b: bit_r8(Register8Bit.E, 1); break;
                case 0x4c: bit_r8(Register8Bit.H, 1); break;
                case 0x4d: bit_r8(Register8Bit.L, 1); break;
                case 0x4e: bit_hla(1); break;
                case 0x4f: bit_r8(Register8Bit.A, 1); break;

                case 0x50: bit_r8(Register8Bit.B, 2); break;
                case 0x51: bit_r8(Register8Bit.C, 2); break;
                case 0x52: bit_r8(Register8Bit.D, 2); break;
                case 0x53: bit_r8(Register8Bit.E, 2); break;
                case 0x54: bit_r8(Register8Bit.H, 2); break;
                case 0x55: bit_r8(Register8Bit.L, 2); break;
                case 0x56: bit_hla(2); break;
                case 0x57: bit_r8(Register8Bit.A, 2); break;
                case 0x58: bit_r8(Register8Bit.B, 3); break;
                case 0x59: bit_r8(Register8Bit.C, 3); break;
                case 0x5a: bit_r8(Register8Bit.D, 3); break;
                case 0x5b: bit_r8(Register8Bit.E, 3); break;
                case 0x5c: bit_r8(Register8Bit.H, 3); break;
                case 0x5d: bit_r8(Register8Bit.L, 3); break;
                case 0x5e: bit_hla(3); break;
                case 0x5f: bit_r8(Register8Bit.A, 3); break;

                case 0x60: bit_r8(Register8Bit.B, 4); break;
                case 0x61: bit_r8(Register8Bit.C, 4); break;
                case 0x62: bit_r8(Register8Bit.D, 4); break;
                case 0x63: bit_r8(Register8Bit.E, 4); break;
                case 0x64: bit_r8(Register8Bit.H, 4); break;
                case 0x65: bit_r8(Register8Bit.L, 4); break;
                case 0x66: bit_hla(4); break;
                case 0x67: bit_r8(Register8Bit.A, 4); break;
                case 0x68: bit_r8(Register8Bit.B, 5); break;
                case 0x69: bit_r8(Register8Bit.C, 5); break;
                case 0x6a: bit_r8(Register8Bit.D, 5); break;
                case 0x6b: bit_r8(Register8Bit.E, 5); break;
                case 0x6c: bit_r8(Register8Bit.H, 5); break;
                case 0x6d: bit_r8(Register8Bit.L, 5); break;
                case 0x6e: bit_hla(5); break;
                case 0x6f: bit_r8(Register8Bit.A, 5); break;

                case 0x70: bit_r8(Register8Bit.B, 6); break;
                case 0x71: bit_r8(Register8Bit.C, 6); break;
                case 0x72: bit_r8(Register8Bit.D, 6); break;
                case 0x73: bit_r8(Register8Bit.E, 6); break;
                case 0x74: bit_r8(Register8Bit.H, 6); break;
                case 0x75: bit_r8(Register8Bit.L, 6); break;
                case 0x76: bit_hla(6); break;
                case 0x77: bit_r8(Register8Bit.A, 6); break;
                case 0x78: bit_r8(Register8Bit.B, 7); break;
                case 0x79: bit_r8(Register8Bit.C, 7); break;
                case 0x7a: bit_r8(Register8Bit.D, 7); break;
                case 0x7b: bit_r8(Register8Bit.E, 7); break;
                case 0x7c: bit_r8(Register8Bit.H, 7); break;
                case 0x7d: bit_r8(Register8Bit.L, 7); break;
                case 0x7e: bit_hla(7); break;
                case 0x7f: bit_r8(Register8Bit.A, 7); break;

                case 0x80: res_r8(Register8Bit.B, 0); break;
                case 0x81: res_r8(Register8Bit.C, 0); break;
                case 0x82: res_r8(Register8Bit.D, 0); break;
                case 0x83: res_r8(Register8Bit.E, 0); break;
                case 0x84: res_r8(Register8Bit.H, 0); break;
                case 0x85: res_r8(Register8Bit.L, 0); break;
                case 0x86: res_hla(0); break;
                case 0x87: res_r8(Register8Bit.A, 0); break;
                case 0x88: res_r8(Register8Bit.B, 1); break;
                case 0x89: res_r8(Register8Bit.C, 1); break;
                case 0x8a: res_r8(Register8Bit.D, 1); break;
                case 0x8b: res_r8(Register8Bit.E, 1); break;
                case 0x8c: res_r8(Register8Bit.H, 1); break;
                case 0x8d: res_r8(Register8Bit.L, 1); break;
                case 0x8e: res_hla(1); break;
                case 0x8f: res_r8(Register8Bit.A, 1); break;

                case 0x90: res_r8(Register8Bit.B, 2); break;
                case 0x91: res_r8(Register8Bit.C, 2); break;
                case 0x92: res_r8(Register8Bit.D, 2); break;
                case 0x93: res_r8(Register8Bit.E, 2); break;
                case 0x94: res_r8(Register8Bit.H, 2); break;
                case 0x95: res_r8(Register8Bit.L, 2); break;
                case 0x96: res_hla(2); break;
                case 0x97: res_r8(Register8Bit.A, 2); break;
                case 0x98: res_r8(Register8Bit.B, 3); break;
                case 0x99: res_r8(Register8Bit.C, 3); break;
                case 0x9a: res_r8(Register8Bit.D, 3); break;
                case 0x9b: res_r8(Register8Bit.E, 3); break;
                case 0x9c: res_r8(Register8Bit.H, 3); break;
                case 0x9d: res_r8(Register8Bit.L, 3); break;
                case 0x9e: res_hla(3); break;
                case 0x9f: res_r8(Register8Bit.A, 3); break;

                case 0xa0: res_r8(Register8Bit.B, 4); break;
                case 0xa1: res_r8(Register8Bit.C, 4); break;
                case 0xa2: res_r8(Register8Bit.D, 4); break;
                case 0xa3: res_r8(Register8Bit.E, 4); break;
                case 0xa4: res_r8(Register8Bit.H, 4); break;
                case 0xa5: res_r8(Register8Bit.L, 4); break;
                case 0xa6: res_hla(4); break;
                case 0xa7: res_r8(Register8Bit.A, 4); break;
                case 0xa8: res_r8(Register8Bit.B, 5); break;
                case 0xa9: res_r8(Register8Bit.C, 5); break;
                case 0xaa: res_r8(Register8Bit.D, 5); break;
                case 0xab: res_r8(Register8Bit.E, 5); break;
                case 0xac: res_r8(Register8Bit.H, 5); break;
                case 0xad: res_r8(Register8Bit.L, 5); break;
                case 0xae: res_hla(5); break;
                case 0xaf: res_r8(Register8Bit.A, 5); break;

                case 0xb0: res_r8(Register8Bit.B, 6); break;
                case 0xb1: res_r8(Register8Bit.C, 6); break;
                case 0xb2: res_r8(Register8Bit.D, 6); break;
                case 0xb3: res_r8(Register8Bit.E, 6); break;
                case 0xb4: res_r8(Register8Bit.H, 6); break;
                case 0xb5: res_r8(Register8Bit.L, 6); break;
                case 0xb6: res_hla(6); break;
                case 0xb7: res_r8(Register8Bit.A, 6); break;
                case 0xb8: res_r8(Register8Bit.B, 7); break;
                case 0xb9: res_r8(Register8Bit.C, 7); break;
                case 0xba: res_r8(Register8Bit.D, 7); break;
                case 0xbb: res_r8(Register8Bit.E, 7); break;
                case 0xbc: res_r8(Register8Bit.H, 7); break;
                case 0xbd: res_r8(Register8Bit.L, 7); break;
                case 0xbe: res_hla(7); break;
                case 0xbf: res_r8(Register8Bit.A, 7); break;

                case 0xc0: set_r8(Register8Bit.B, 0); break;
                case 0xc1: set_r8(Register8Bit.C, 0); break;
                case 0xc2: set_r8(Register8Bit.D, 0); break;
                case 0xc3: set_r8(Register8Bit.E, 0); break;
                case 0xc4: set_r8(Register8Bit.H, 0); break;
                case 0xc5: set_r8(Register8Bit.L, 0); break;
                case 0xc6: set_hla(0); break;
                case 0xc7: set_r8(Register8Bit.A, 0); break;
                case 0xc8: set_r8(Register8Bit.B, 1); break;
                case 0xc9: set_r8(Register8Bit.C, 1); break;
                case 0xca: set_r8(Register8Bit.D, 1); break;
                case 0xcb: set_r8(Register8Bit.E, 1); break;
                case 0xcc: set_r8(Register8Bit.H, 1); break;
                case 0xcd: set_r8(Register8Bit.L, 1); break;
                case 0xce: set_hla(1); break;
                case 0xcf: set_r8(Register8Bit.A, 1); break;

                case 0xd0: set_r8(Register8Bit.B, 2); break;
                case 0xd1: set_r8(Register8Bit.C, 2); break;
                case 0xd2: set_r8(Register8Bit.D, 2); break;
                case 0xd3: set_r8(Register8Bit.E, 2); break;
                case 0xd4: set_r8(Register8Bit.H, 2); break;
                case 0xd5: set_r8(Register8Bit.L, 2); break;
                case 0xd6: set_hla(2); break;
                case 0xd7: set_r8(Register8Bit.A, 2); break;
                case 0xd8: set_r8(Register8Bit.B, 3); break;
                case 0xd9: set_r8(Register8Bit.C, 3); break;
                case 0xda: set_r8(Register8Bit.D, 3); break;
                case 0xdb: set_r8(Register8Bit.E, 3); break;
                case 0xdc: set_r8(Register8Bit.H, 3); break;
                case 0xdd: set_r8(Register8Bit.L, 3); break;
                case 0xde: set_hla(3); break;
                case 0xdf: set_r8(Register8Bit.A, 3); break;

                case 0xe0: set_r8(Register8Bit.B, 4); break;
                case 0xe1: set_r8(Register8Bit.C, 4); break;
                case 0xe2: set_r8(Register8Bit.D, 4); break;
                case 0xe3: set_r8(Register8Bit.E, 4); break;
                case 0xe4: set_r8(Register8Bit.H, 4); break;
                case 0xe5: set_r8(Register8Bit.L, 4); break;
                case 0xe6: set_hla(4); break;
                case 0xe7: set_r8(Register8Bit.A, 4); break;
                case 0xe8: set_r8(Register8Bit.B, 5); break;
                case 0xe9: set_r8(Register8Bit.C, 5); break;
                case 0xea: set_r8(Register8Bit.D, 5); break;
                case 0xeb: set_r8(Register8Bit.E, 5); break;
                case 0xec: set_r8(Register8Bit.H, 5); break;
                case 0xed: set_r8(Register8Bit.L, 5); break;
                case 0xee: set_hla(5); break;
                case 0xef: set_r8(Register8Bit.A, 5); break;

                case 0xf0: set_r8(Register8Bit.B, 6); break;
                case 0xf1: set_r8(Register8Bit.C, 6); break;
                case 0xf2: set_r8(Register8Bit.D, 6); break;
                case 0xf3: set_r8(Register8Bit.E, 6); break;
                case 0xf4: set_r8(Register8Bit.H, 6); break;
                case 0xf5: set_r8(Register8Bit.L, 6); break;
                case 0xf6: set_hla(6); break;
                case 0xf7: set_r8(Register8Bit.A, 6); break;
                case 0xf8: set_r8(Register8Bit.B, 7); break;
                case 0xf9: set_r8(Register8Bit.C, 7); break;
                case 0xfa: set_r8(Register8Bit.D, 7); break;
                case 0xfb: set_r8(Register8Bit.E, 7); break;
                case 0xfc: set_r8(Register8Bit.H, 7); break;
                case 0xfd: set_r8(Register8Bit.L, 7); break;
                case 0xfe: set_hla(7); break;
                case 0xff: set_r8(Register8Bit.A, 7); break;
                default:
                    throw new ArgumentException($"Unknown CB opcode: 0x{cbOpcode:x2}", nameof(cbOpcode));
            }
        }
    }
}
