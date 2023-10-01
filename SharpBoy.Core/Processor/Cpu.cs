using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.InputHandling;
using SharpBoy.Core.Interrupts;
using SharpBoy.Core.Memory;
using SharpBoy.Core.Timing;
using SharpBoy.Core.Utilities;

namespace SharpBoy.Core.Processor
{
    public class Cpu : ICpu
    {
        internal CpuState State { get; set; }

        internal IMmu Mmu { get; }
        internal IInterruptManager InterruptManager { get; }
        internal byte Opcode { get; private set; }
        public CpuRegisters Registers { get; }

        private int cycles = 0;
        private bool haltBugTriggered = false;
        private readonly IPpu ppu;
        private readonly ITimer timer;
        private readonly IInputController inputController;

        public Cpu(IMmu mmu, IInterruptManager interruptManager, IPpu ppu, ITimer timer, IInputController inputController)
        {
            Mmu = mmu;
            InterruptManager = interruptManager;
            this.ppu = ppu;
            this.timer = timer;
            this.inputController = inputController;
            Registers = new CpuRegisters();
        }

        public int Step()
        {
            cycles = 0;

            if (State == CpuState.Running)
            {
                Opcode = ReadImmediate8Bit();
                ExecuteInstruction(Opcode);
            }
            else
            {
                CycleTick();
            }

            HandleInterrupts();
            inputController.CheckForInputs();

            return cycles;
        }

        private void HandleInterrupts()
        {
            if (InterruptManager.AnyInterruptRequested)
            {
                State = CpuState.Running;
                CycleTick();

                if (InterruptManager.IME)
                {
                    var interrupt = InterruptManager.GetRequestedInterrupt();
                    if (interrupt != null)
                    {
                        InterruptManager.IME = false;
                        InterruptManager.ClearInterrupt(interrupt.Flag);

                        CycleTick();
                        rst(interrupt.Vector);
                    }
                }
            }
        }

        private void CycleTick()
        {
            cycles += 4;
            ppu.Tick();
            timer.Tick();
        }

        private void ExecuteInstruction(byte opcode)
        {
            switch (opcode)
            {
                case 0x00: break;

                case 0x01: ld_16_16(Operand16Bit.BC, Operand16Bit.Immediate); break;
                case 0x02: ld_8_8(Operand8Bit.IndirectBC, Operand8Bit.A); break;
                case 0x03: inc_16(Operand16Bit.BC); break;
                case 0x04: inc_8(Operand8Bit.B); break;
                case 0x05: dec_8(Operand8Bit.B); break;
                case 0x06: ld_8_8(Operand8Bit.B, Operand8Bit.Immediate); break;
                case 0x07: rlca(); break;
                case 0x08: ld_16_16(Operand16Bit.IndirectImmediate, Operand16Bit.SP); break;
                case 0x09: add_hl_r16(Operand16Bit.BC); break;
                case 0x0a: ld_8_8(Operand8Bit.A, Operand8Bit.IndirectBC); break;
                case 0x0b: dec_16(Operand16Bit.BC); break;
                case 0x0c: inc_8(Operand8Bit.C); break;
                case 0x0d: dec_8(Operand8Bit.C); break;
                case 0x0e: ld_8_8(Operand8Bit.C, Operand8Bit.Immediate); break;
                case 0x0f: rrca(); break;

                case 0x10: stop(); break;
                case 0x11: ld_16_16(Operand16Bit.DE, Operand16Bit.Immediate); break;
                case 0x12: ld_8_8(Operand8Bit.IndirectDE, Operand8Bit.A); break;
                case 0x13: inc_16(Operand16Bit.DE); break;
                case 0x14: inc_8(Operand8Bit.D); break;
                case 0x15: dec_8(Operand8Bit.D); break;
                case 0x16: ld_8_8(Operand8Bit.D, Operand8Bit.Immediate); break;
                case 0x17: rla(); break;
                case 0x18: jr_i8(); break;
                case 0x19: add_hl_r16(Operand16Bit.DE); break;
                case 0x1a: ld_8_8(Operand8Bit.A, Operand8Bit.IndirectDE); break;
                case 0x1b: dec_16(Operand16Bit.DE); break;
                case 0x1c: inc_8(Operand8Bit.E); break;
                case 0x1d: dec_8(Operand8Bit.E); break;
                case 0x1e: ld_8_8(Operand8Bit.E, Operand8Bit.Immediate); break;
                case 0x1f: rra(); break;

                case 0x20: jr_i8(Flags.Zero, false); break;
                case 0x21: ld_16_16(Operand16Bit.HL, Operand16Bit.Immediate); break;
                case 0x22: ld_hl_a(1); break;
                case 0x23: inc_16(Operand16Bit.HL); break;
                case 0x24: inc_8(Operand8Bit.H); break;
                case 0x25: dec_8(Operand8Bit.H); break;
                case 0x26: ld_8_8(Operand8Bit.H, Operand8Bit.Immediate); break;
                case 0x27: daa(); break;
                case 0x28: jr_i8(Flags.Zero, true); break;
                case 0x29: add_hl_r16(Operand16Bit.HL); break;
                case 0x2a: ld_a_hl(1); break;
                case 0x2b: dec_16(Operand16Bit.HL); break;
                case 0x2c: inc_8(Operand8Bit.L); break;
                case 0x2d: dec_8(Operand8Bit.L); break;
                case 0x2e: ld_8_8(Operand8Bit.L, Operand8Bit.Immediate); break;
                case 0x2f: cpl(); break;

                case 0x30: jr_i8(Flags.Carry, false); break;
                case 0x31: ld_16_16(Operand16Bit.SP, Operand16Bit.Immediate); break;
                case 0x32: ld_hl_a(-1); break;
                case 0x33: inc_16(Operand16Bit.SP); break;
                case 0x34: inc_8(Operand8Bit.IndirectHL); break;
                case 0x35: dec_8(Operand8Bit.IndirectHL); break;
                case 0x36: ld_8_8(Operand8Bit.IndirectHL, Operand8Bit.Immediate); break;
                case 0x37: scf(); break;
                case 0x38: jr_i8(Flags.Carry, true); break;
                case 0x39: add_hl_r16(Operand16Bit.SP); break;
                case 0x3a: ld_a_hl(-1); break;
                case 0x3b: dec_16(Operand16Bit.SP); break;
                case 0x3c: inc_8(Operand8Bit.A); break;
                case 0x3d: dec_8(Operand8Bit.A); break;
                case 0x3e: ld_8_8(Operand8Bit.A, Operand8Bit.Immediate); break;
                case 0x3f: ccf(); break;

                case 0x76: halt(); break;

                case <= 0x7f: ld_8_8((Operand8Bit)(opcode >> 3 & 0x07), (Operand8Bit)(opcode & 0x07)); break;
                case <= 0x87: add_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0x8f: adc_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0x97: sub_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0x9f: sbc_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xa7: and_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xaf: xor_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xb7: or_a((Operand8Bit)(opcode & 0x07)); break;
                case <= 0xbf: cp_a((Operand8Bit)(opcode & 0x07)); break;

                case 0xc0: ret(Flags.Zero, false); break;
                case 0xc1: pop(Operand16Bit.BC); break;
                case 0xc2: jp_i16(Flags.Zero, false); break;
                case 0xc3: jp_i16(); break;
                case 0xc4: call_i16(Flags.Zero, false); break;
                case 0xc5: push(Operand16Bit.BC); break;
                case 0xc6: add_a(Operand8Bit.Immediate); break;
                case 0xc7: rst(0x00); break;
                case 0xc8: ret(Flags.Zero, true); break;
                case 0xc9: ret(); break;
                case 0xca: jp_i16(Flags.Zero, true); break;
                case 0xcb: ExecuteCBInstruction(ReadImmediate8Bit()); break;
                case 0xcc: call_i16(Flags.Zero, true); break;
                case 0xcd: call_i16(); break;
                case 0xce: adc_a(Operand8Bit.Immediate); break;
                case 0xcf: rst(0x08); break;

                case 0xd0: ret(Flags.Carry, false); break;
                case 0xd1: pop(Operand16Bit.DE); break;
                case 0xd2: jp_i16(Flags.Carry, false); break;
                // no 0xd3
                case 0xd4: call_i16(Flags.Carry, false); break;
                case 0xd5: push(Operand16Bit.DE); break;
                case 0xd6: sub_a(Operand8Bit.Immediate); break;
                case 0xd7: rst(0x10); break;
                case 0xd8: ret(Flags.Carry, true); break;
                case 0xd9: reti(); break;
                case 0xda: jp_i16(Flags.Carry, true); break;
                // no 0xdb
                case 0xdc: call_i16(Flags.Carry, true); break;
                // no 0xdd
                case 0xde: sbc_a(Operand8Bit.Immediate); break;
                case 0xdf: rst(0x18); break;

                case 0xe0: ld_8_8(Operand8Bit.IndirectImmediateByte, Operand8Bit.A); break;
                case 0xe1: pop(Operand16Bit.HL); break;
                case 0xe2: ld_8_8(Operand8Bit.IndirectC, Operand8Bit.A); break;
                // no 0xe3
                // no 0xe4
                case 0xe5: push(Operand16Bit.HL); break;
                case 0xe6: and_a(Operand8Bit.Immediate); break;
                case 0xe7: rst(0x20); break;
                case 0xe8: add_sp_i8(); break;
                case 0xe9: jp_hl(); break;
                case 0xea: ld_8_8(Operand8Bit.IndirectImmediateWord, Operand8Bit.A); break;
                // no 0xeb
                // no 0xec
                // no 0xed
                case 0xee: xor_a(Operand8Bit.Immediate); break;
                case 0xef: rst(0x28); break;

                case 0xf0: ld_8_8(Operand8Bit.A, Operand8Bit.IndirectImmediateByte); break;
                case 0xf1: pop_af(); break;
                case 0xf2: ld_8_8(Operand8Bit.A, Operand8Bit.IndirectC); break;
                case 0xf3: di(); break;
                // no 0xf4
                case 0xf5: push(Operand16Bit.AF); break;
                case 0xf6: or_a(Operand8Bit.Immediate); break;
                case 0xf7: rst(0x30); break;
                case 0xf8: ld_hl_spi8(); break;
                case 0xf9: ld_sp_hl(); break;
                case 0xfa: ld_8_8(Operand8Bit.A, Operand8Bit.IndirectImmediateWord); break;
                case 0xfb: ei(); break;
                // no 0xfc
                // no 0xfd
                case 0xfe: cp_a(Operand8Bit.Immediate); break;
                case 0xff: rst(0x38); break;

                default:
                    throw new ArgumentException($"Unknown opcode: 0x{opcode:x2}", nameof(opcode));
                    //CycleTick();
                    //break;
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
                case <= 0x7f: bit(operand, cbOpcode >> 3 & 0x07); break;
                case <= 0xbf: res(operand, cbOpcode >> 3 & 0x07); break;
                case <= 0xff: set(operand, cbOpcode >> 3 & 0x07); break;
                default: throw new ArgumentException($"Unknown CB opcode: 0x{cbOpcode:x2}", nameof(cbOpcode));
            }
        }

        private byte ReadImmediate8Bit()
        {
            var val = Read8BitValueFromMemory(Registers.PC);
            if (haltBugTriggered)
            {
                haltBugTriggered = false;
            }
            else
            {
                Registers.PC += 1;
            }
            return val;
        }

        private ushort ReadImmediate16Bit()
        {
            var low = ReadImmediate8Bit();
            var high = ReadImmediate8Bit();
            return BitUtils.Get16BitValue(high, low);
        }

        private byte Read8BitValueFromMemory(ushort address)
        {
            var val = Mmu.Read(address);
            CycleTick();
            return val;
        }

        private ushort Read16BitValueFromMemory(ushort address)
        {
            var low = Read8BitValueFromMemory(address);
            var high = Read8BitValueFromMemory((ushort)(address + 1));
            return BitUtils.Get16BitValue(high, low);
        }

        private void Write8BitValueToMemory(ushort address, byte value)
        {
            Mmu.Write(address, value);
            CycleTick();
        }

        private void Write16BitValueToMemory(ushort address, ushort value)
        {
            Write8BitValueToMemory(address, BitUtils.GetLowByte(value));
            Write8BitValueToMemory((ushort)(address + 1), BitUtils.GetHighByte(value));
        }

        private byte ReadValue(Operand8Bit operand)
        {
            return operand switch
            {
                Operand8Bit.B => Registers.B,
                Operand8Bit.C => Registers.C,
                Operand8Bit.D => Registers.D,
                Operand8Bit.E => Registers.E,
                Operand8Bit.H => Registers.H,
                Operand8Bit.L => Registers.L,
                Operand8Bit.IndirectHL => Read8BitValueFromMemory(Registers.HL),
                Operand8Bit.A => Registers.A,
                Operand8Bit.Immediate => ReadImmediate8Bit(),
                Operand8Bit.IndirectBC => Read8BitValueFromMemory(Registers.BC),
                Operand8Bit.IndirectDE => Read8BitValueFromMemory(Registers.DE),
                Operand8Bit.IndirectImmediateWord => Read8BitValueFromMemory(ReadImmediate16Bit()),
                Operand8Bit.IndirectImmediateByte => Read8BitValueFromMemory((ushort)(0xff00 | ReadImmediate8Bit())),
                Operand8Bit.IndirectC => Read8BitValueFromMemory((ushort)(0xff00 | Registers.C)),
                _ => throw new NotImplementedException()
            };
        }

        private void WriteValue(Operand8Bit operand, byte value)
        {
            switch (operand)
            {
                case Operand8Bit.B: Registers.B = value; break;
                case Operand8Bit.C: Registers.C = value; break;
                case Operand8Bit.D: Registers.D = value; break;
                case Operand8Bit.E: Registers.E = value; break;
                case Operand8Bit.H: Registers.H = value; break;
                case Operand8Bit.L: Registers.L = value; break;
                case Operand8Bit.IndirectHL: Write8BitValueToMemory(Registers.HL, value); break;
                case Operand8Bit.A: Registers.A = value; break;
                case Operand8Bit.IndirectBC: Write8BitValueToMemory(Registers.BC, value); break;
                case Operand8Bit.IndirectDE: Write8BitValueToMemory(Registers.DE, value); break;
                case Operand8Bit.IndirectImmediateWord: Write8BitValueToMemory(ReadImmediate16Bit(), value); break;
                case Operand8Bit.IndirectImmediateByte: Write8BitValueToMemory((ushort)(0xff00 | ReadImmediate8Bit()), value); break;
                case Operand8Bit.IndirectC: Write8BitValueToMemory((ushort)(0xff00 | Registers.C), value); break;
                default: throw new NotImplementedException();
            };
        }

        private void ReadWriteValue(Operand8Bit operand, Func<CpuRegisters, byte, byte> func) => WriteValue(operand, func(Registers, ReadValue(operand)));

        private void ReadWriteValue(Operand8Bit operand, byte value2, Func<byte, byte, byte> func) => WriteValue(operand, func(ReadValue(operand), value2));

        private ushort ReadValue(Operand16Bit operand)
        {
            return operand switch
            {
                Operand16Bit.SP => Registers.SP,
                Operand16Bit.AF => Registers.AF,
                Operand16Bit.PC => Registers.PC,
                Operand16Bit.HL => Registers.HL,
                Operand16Bit.BC => Registers.BC,
                Operand16Bit.DE => Registers.DE,
                Operand16Bit.Immediate => ReadImmediate16Bit(),
                //Operand16Bit.IndirectImmediate => Read16Bit(ReadImmediate16Bit()),
                _ => throw new NotImplementedException()
            };
        }

        private void WriteValue(Operand16Bit operand, ushort value)
        {
            switch (operand)
            {
                case Operand16Bit.SP: Registers.SP = value; break;
                case Operand16Bit.AF: Registers.AF = value; break;
                case Operand16Bit.PC: Registers.PC = value; break;
                case Operand16Bit.HL: Registers.HL = value; break;
                case Operand16Bit.BC: Registers.BC = value; break;
                case Operand16Bit.DE: Registers.DE = value; break;
                case Operand16Bit.IndirectImmediate: Write16BitValueToMemory(ReadImmediate16Bit(), value); break;
                default: throw new NotImplementedException();
            };
        }

        private void halt()
        {
            if (!InterruptManager.IME && InterruptManager.AnyInterruptRequested)
            {
                haltBugTriggered = true;
            }
            else
            {
                State = CpuState.Halted;
            }
            CycleTick();
            CycleTick();
        }

        private void stop()
        {
            State = CpuState.Stopped;
            CycleTick();
            CycleTick();
        }

        private void di() => InterruptManager.IME = false;
        private void ei() => InterruptManager.IME = true;

        private void reti()
        {
            ei();
            ret();
        }

        private void inc_16(Operand16Bit operand)
        {
            WriteValue(operand, (ushort)(ReadValue(operand) + 1));
            CycleTick();
        }

        private void dec_16(Operand16Bit operand)
        {
            WriteValue(operand, (ushort)(ReadValue(operand) - 1));
            CycleTick();
        }

        private void inc_8(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.inc);
        private void dec_8(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.dec);
        private void ld_8_8(Operand8Bit a, Operand8Bit b) => WriteValue(a, ReadValue(b));
        private void ld_16_16(Operand16Bit a, Operand16Bit b) => WriteValue(a, ReadValue(b));

        private void ld_sp_hl()
        {
            ld_16_16(Operand16Bit.SP, Operand16Bit.HL);
            CycleTick();
        }

        private void ld_hl_a(int increment)
        {
            Write8BitValueToMemory(Registers.HL, Registers.A);
            Registers.HL += (ushort)increment;
        }

        private void ld_a_hl(int increment)
        {
            Registers.A = Read8BitValueFromMemory(Registers.HL);
            Registers.HL += (ushort)increment;
        }

        private void ld_hl_spi8()
        {
            Registers.HL = sp_i8();
        }

        private void rla() => Registers.A = AluOperations.rl(Registers, Registers.A);
        private void rlca() => Registers.A = AluOperations.rlc(Registers, Registers.A);
        private void rl(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rl_cb);
        private void rlc(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rlc_cb);

        private void rra() => Registers.A = AluOperations.rr(Registers, Registers.A);
        private void rrca() => Registers.A = AluOperations.rrc(Registers, Registers.A);
        private void rr(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rr_cb);
        private void rrc(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.rrc_cb);

        private void sla(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.sla);
        private void sra(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.sra);
        private void srl(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.srl);

        private void add_a(Operand8Bit operand) => Registers.A = AluOperations.add(Registers, Registers.A, ReadValue(operand));
        private void adc_a(Operand8Bit operand) => Registers.A = AluOperations.adc(Registers, Registers.A, ReadValue(operand));

        private void add_hl_r16(Operand16Bit operand)
        {
            // https://stackoverflow.com/questions/57958631/game-boy-half-carry-flag-and-16-bit-instructions-especially-opcode-0xe8
            // ADD HL,rr - "Based on my testing, H is set if carry occurs from bit 11 to bit 12."

            var value = ReadValue(operand);
            var result = Registers.HL + value;
            var halfCarryResult = (Registers.HL & 0xfff) + (value & 0xfff);

            Registers.SetFlag(Flags.Subtract, false);
            Registers.SetFlag(Flags.HalfCarry, halfCarryResult > 0xfff);
            Registers.SetFlag(Flags.Carry, result > 0xffff);

            Registers.HL = (ushort)result;
            CycleTick();
        }
        private void add_sp_i8()
        {
            Registers.SP = sp_i8();
            CycleTick();
        }

        private ushort sp_i8()
        {
            // https://stackoverflow.com/questions/57958631/game-boy-half-carry-flag-and-16-bit-instructions-especially-opcode-0xe8
            // TL; DR: For ADD SP,n, the H-flag is set when carry occurs from bit 3 to bit 4.

            var value = (sbyte)ReadImmediate8Bit();
            var result = Registers.SP + value;
            var carryResult = (Registers.SP & 0xff) + (value & 0xff);
            var halfCarryResult = (Registers.SP & 0xf) + (value & 0xf);

            Registers.SetFlag(Flags.Zero, false);
            Registers.SetFlag(Flags.Subtract, false);
            Registers.SetFlag(Flags.HalfCarry, halfCarryResult > 0xf);
            Registers.SetFlag(Flags.Carry, carryResult > 0xff);

            CycleTick();

            return (ushort)result;
        }

        private void sub_a(Operand8Bit operand) => Registers.A = AluOperations.sub(Registers, Registers.A, ReadValue(operand));
        private void sbc_a(Operand8Bit operand) => Registers.A = AluOperations.sbc(Registers, Registers.A, ReadValue(operand));

        private void and_a(Operand8Bit operand) => Registers.A = AluOperations.and(Registers, Registers.A, ReadValue(operand));
        private void xor_a(Operand8Bit operand) => Registers.A = AluOperations.xor(Registers, Registers.A, ReadValue(operand));
        private void or_a(Operand8Bit operand) => Registers.A = AluOperations.or(Registers, Registers.A, ReadValue(operand));
        private void cp_a(Operand8Bit operand) => Registers.A = AluOperations.cp(Registers, Registers.A, ReadValue(operand));

        private void daa() => Registers.A = AluOperations.daa(Registers, Registers.A);

        private void ccf() => AluOperations.ccf(Registers);
        private void scf() => AluOperations.scf(Registers);
        private void cpl() => Registers.A = AluOperations.cpl(Registers, Registers.A);

        private void jr_i8()
        {
            var increment = (sbyte)ReadImmediate8Bit();
            Registers.PC = (ushort)(ReadValue(Operand16Bit.PC) + increment);
            CycleTick();
        }

        private void jr_i8(Flags flag, bool isSet)
        {
            var val = (sbyte)ReadImmediate8Bit();
            if (Registers.F.HasFlag(flag) == isSet)
            {
                Registers.PC = (ushort)(ReadValue(Operand16Bit.PC) + val);
                CycleTick();
            }
        }

        private void jp_i16()
        {
            Registers.PC = ReadImmediate16Bit();
            CycleTick();
        }

        private void jp_hl() => Registers.PC = Registers.HL;

        private void jp_i16(Flags flag, bool isSet)
        {
            ushort pc = ReadImmediate16Bit();
            if (Registers.F.HasFlag(flag) == isSet)
            {
                Registers.PC = pc;
                CycleTick();
            }
        }

        private void call_i16()
        {
            ushort pc = ReadImmediate16Bit();
            push(Registers.PC);
            Registers.PC = pc;
        }

        private void call_i16(Flags flag, bool isSet)
        {
            ushort pc = ReadImmediate16Bit();

            if (Registers.F.HasFlag(flag) == isSet)
            {
                push(Registers.PC);
                Registers.PC = pc;
            }
        }

        private void ret()
        {
            Registers.PC = pop();
            CycleTick();
        }

        private void ret(Flags flag, bool isSet)
        {
            CycleTick();
            if (Registers.F.HasFlag(flag) == isSet)
            {
                Registers.PC = pop();
                CycleTick();
            }
        }

        private void rst(ushort address)
        {
            push(Registers.PC);
            Registers.PC = address;
        }

        private void push(Operand16Bit operand) => push(ReadValue(operand));
        private void push(ushort value)
        {
            Registers.SP -= 2;
            Write16BitValueToMemory(Registers.SP, value);
            CycleTick();
        }

        private void pop(Operand16Bit operand) => WriteValue(operand, pop());
        private void pop_af() => Registers.AF = (ushort)(pop() & 0xfff0); // ensure the low nibble is cleared for F 

        ushort pop()
        {
            var value = Read16BitValueFromMemory(Registers.SP);
            Registers.SP += 2;
            return value;
        }

        private void bit(Operand8Bit operand, int bit)
        {
            AluOperations.bit(Registers, ReadValue(operand), (byte)bit);
        }

        private void res(Operand8Bit operand, int bit) => ReadWriteValue(operand, (byte)bit, AluOperations.res);
        private void set(Operand8Bit operand, int bit) => ReadWriteValue(operand, (byte)bit, AluOperations.set);
        private void swap(Operand8Bit operand) => ReadWriteValue(operand, AluOperations.swap);

        private enum Operand8Bit
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
            Immediate = 10,
            IndirectImmediateWord = 11,
            IndirectImmediateByte = 12,
            IndirectC = 13,
        }

        private enum Operand16Bit
        {
            SP = 0,
            AF = 1,
            PC = 2,
            HL = 3,
            BC = 4,
            DE = 5,
            Immediate = 6,
            IndirectImmediate = 7
        }
    }

    internal enum CpuState
    {
        Running,
        Halted,
        Stopped
    }
}
