using SharpBoy.Core.Processor;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace SharpBoy.Core.Memory
{
    public class Mmu : IMmu
    {
        private GameBoy gameboy;
        private byte[] bootRom = new byte[0x100];
        private byte[] wram = new byte[0x2000];
        private byte[] hram = new byte[0x80];

        private byte[] ioRegisters = new byte[0x10000];

        private bool inBootRom = false;

        public Mmu(GameBoy gameboy)
        {
            this.gameboy = gameboy;
        }

        public void LoadBootRom(byte[] rom)
        {
            Buffer.BlockCopy(rom, 0, bootRom, 0, rom.Length);
            inBootRom = true;
        }

        public byte ReadValue(ushort address)
        {
            return address switch
            {
                <= 0x7fff => ReadRom(address),
                <= 0x9fff => gameboy.Ppu.ReadVram((ushort)(address & 0x1fff)),
                <= 0xbfff => gameboy.Cartridge.ReadERam((ushort)(address & 0x1fff)),
                <= 0xcfff => wram[(ushort)(address & 0x1fff)],
                <= 0xdfff => wram[(ushort)(address & 0x1fff)], // In CGB mode, switchable bank 1~7 ;
                <= 0xfdff => wram[(ushort)(address & 0x1fff)], // copy of wram (echo ram) - use of this area should be prohibited
                <= 0xfe9f => gameboy.Ppu.ReadOam((byte)(address & 0xff)),
                <= 0xfeff => 0, // use of this area should be prohibited
                <= 0xff03 => ioRegisters[address], // handle I/O registers here
                <= 0xff07 => gameboy.Timer.ReadRegister(address),
                <= 0xff0e => ioRegisters[address], // handle I/O registers here
                <= 0xff0f => (byte)gameboy.InterruptManager.IF,
                <= 0xff3f => ioRegisters[address], // handle I/O registers here
                <= 0xff4b => gameboy.Ppu.ReadRegister(address),
                <= 0xff7f => ioRegisters[address], // handle I/O registers here
                <= 0xfffe => hram[(byte)(address & 0x7f)],
                <= 0xffff => (byte)gameboy.InterruptManager.IE
            };
        }

        public void WriteValue(ushort address, byte value)
        {
            switch (address)
            {
                case <= 0x7fff:
                    break;
                case <= 0x9fff:
                    // In CGB mode, switchable bank 0/1
                    gameboy.Ppu.WriteVram((ushort)(address & 0x1fff), value);
                    break;
                case <= 0xbfff:
                    gameboy.Cartridge.WriteERam((ushort)(address & 0x1fff), value);
                    break;
                case <= 0xcfff:
                    wram[(ushort)(address & 0x1fff)] = value;
                    break;
                case <= 0xdfff:
                    // In CGB mode, switchable bank 1~7
                    wram[(ushort)(address & 0x1fff)] = value;
                    break;
                case <= 0xfdff:
                    // copy of wram (echo ram) - use of this area should be prohibited
                    wram[(ushort)(address & 0x1fff)] = value;
                    break;
                case <= 0xfe9f:
                    gameboy.Ppu.WriteOam((byte)(address & 0xff), value);
                    break;
                case <= 0xfeff:
                    // use of this area should be prohibited
                    break;
                case <= 0xff03:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xff07:
                    gameboy.Timer.WriteRegister(address, value);
                    break;
                case <= 0xff0e:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xff0f:
                    gameboy.InterruptManager.IF = (InterruptFlag)value;
                    break;
                case <= 0xff3f:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xff4b:
                    gameboy.Ppu.WriteRegister(address, value);
                    break;
                case 0xff50:
                    inBootRom = false;
                    break;
                case <= 0xff7f:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xfffe:
                    hram[(byte)(address & 0x7f)] = value;
                    break;
                case 0xffff:
                    gameboy.InterruptManager.IE = (InterruptFlag)value;
                    break;
            }
        }

        private byte ReadRom(ushort address)
        {
            if (inBootRom && address < 0x100)
            {
                return bootRom[address];
            }
            return gameboy.Cartridge.ReadRom(address);
        }
    }
}
