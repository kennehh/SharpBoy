using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;

namespace SharpBoy.Core
{
    public partial class Cpu
    {
        public bool Halted { get; private set; }
        public bool Stopped { get; private set; }
        public bool InterruptsEnabled { get; private set; }

        internal IMmu memory;
        internal Registers registers;
        internal bool branchTaken = false;

        private int currentCycles;

        public Cpu(IMmu mmu) 
        { 
            memory = mmu;
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

                case <= 0x87: add_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0x8f: adc_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0x97: sub_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0x9f: sbc_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xa7: and_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xaf: xor_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xb7: or_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xbf: cp_a((Operand8Bit)(opcode & 0x07)); break;

                case 0xc0: ret(Flag.Zero, false); break;
                case 0xc1: pop(Register16Bit.BC); break;
                case 0xc2: jp_i16(Flag.Zero, false); break;
                case 0xc3: jp_i16(); break;
                case 0xc4: call_i16(Flag.Zero, false); break;
                case 0xc5: push(Register16Bit.BC); break;
                case 0xc6: add_a(Operand8Bit.Immediate); break;
                case 0xc7: rst(0x00); break;
                case 0xc8: ret(Flag.Zero, true); break;
                case 0xc9: ret(); break;
                case 0xca: jp_i16(Flag.Zero, true); break;
                case 0xcb: ExecuteCBInstruction(ReadImmediate8Bit()); break;
                case 0xcc: call_i16(Flag.Zero, true); break;
                case 0xcd: call_i16(); break;
                case 0xce: adc_a(Operand8Bit.Immediate); break;
                case 0xcf: rst(0x08); break;

                case 0xd0: ret(Flag.Carry, false); break;
                case 0xd1: pop(Register16Bit.DE); break;
                case 0xd2: jp_i16(Flag.Carry, false); break;
                // no 0xd3
                case 0xd4: call_i16(Flag.Carry, false); break;
                case 0xd5: push(Register16Bit.DE); break;
                case 0xd6: sub_a(Operand8Bit.Immediate); break;
                case 0xd7: rst(0x10); break;
                case 0xd8: ret(Flag.Carry, true); break;
                case 0xd9: ret(); break;
                case 0xda: jp_i16(Flag.Carry, true); break;
                // no 0xdb
                case 0xdc: call_i16(Flag.Carry, true); break;
                // no 0xdd
                case 0xde: sbc_a(Operand8Bit.Immediate); break;
                case 0xdf: rst(0x18); break;

                case 0xe0: ld_ia8_a(); break;
                case 0xe1: pop(Register16Bit.HL); break;
                case 0xe2: ld_ca_a(); break;
                // no 0xe3
                // no 0xe4
                case 0xe5: push(Register16Bit.HL); break;
                case 0xe6: and_a(Operand8Bit.Immediate); break;
                case 0xe7: rst(0x20); break;
                case 0xe8: add_sp_i8(); break;
                case 0xe9: jp_hl(); break;
                case 0xea: ld_ia16_a(); break;
                // no 0xeb
                // no 0xec
                // no 0xed
                case 0xee: xor_a(Operand8Bit.Immediate); break;
                case 0xef: rst(0x28); break;

                case 0xf0: ld_a_ia8(); break;
                case 0xf1: pop_af(); break;
                case 0xf2: ld_a_ca(); break;
                case 0xf3: di(); break;
                // no 0xf4
                case 0xf5: push(Register16Bit.AF); break;
                case 0xf6: or_a(Operand8Bit.Immediate); break;
                case 0xf7: rst(0x30); break;
                case 0xf8: ld_hl_spi8(); break;
                case 0xf9: ld_sp_hl(); break;
                case 0xfa: ld_a_ia16(); break;
                case 0xfb: ei(); break;
                // no 0xfc
                // no 0xfd
                case 0xfe: cp_a(Operand8Bit.Immediate); break;
                case 0xff: rst(0x38); break;


                default:
                    throw new ArgumentException($"Unknown opcode: 0x{opcode:x2}", nameof(opcode));
            }

        }

        private void ExecuteCBInstruction(byte cbOpcode)
        {
            var operand = (Operand8Bit)(cbOpcode & 0x07);

            switch (cbOpcode)
            {
                case <= 0x07: rlc(operand); break;
                case <= 0x0f: rrc(operand); break;
                case <= 0x17: rl(operand); break;
                case <= 0x1f: rr(operand); break;
                case <= 0x27: sla(operand); break;
                case <= 0x2f: sra(operand); break;
                case <= 0x37: swap(operand); break;
                case <= 0x3f: srl(operand); break;
                case <= 0x7f: bit(operand, (cbOpcode >> 3) & 0x07); break;
                case <= 0xbf: res(operand, (cbOpcode >> 3) & 0x07); break;
                case <= 0xff: set(operand, (cbOpcode >> 3) & 0x07); break;
                default: throw new ArgumentException($"Unknown CB opcode: 0x{cbOpcode:x2}", nameof(cbOpcode));
            }
        }
    }
}
