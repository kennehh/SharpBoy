using SharpBoy.Core.Processor;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SharpBoy.Core.Memory
{
    public class Mmu : IMmu
    {
        private GameBoy gameboy;
        private byte[] bios = new byte[0x100];
        private byte[] wram = new byte[0x2000];
        private byte[] hram = new byte[0x80];

        private byte[] ioRegisters = new byte[0x10000];

        public Mmu(GameBoy gameboy)
        {
            this.gameboy = gameboy;
        }

        public byte Read8Bit(ushort address)
        {
            return address switch
            {
                <= 0x7fff => gameboy.Cartridge.ReadRom(address),
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
                <= 0xff7f => ioRegisters[address], // handle I/O registers here
                <= 0xfffe => hram[(byte)(address & 0x7f)],
                <= 0xffff => (byte)gameboy.InterruptManager.IE
            };
        }

        public ushort Read16Bit(ushort address)
        {
            var low = Read8Bit(address);
            var high = Read8Bit((ushort)(address + 1));
            return Utils.Get16BitValue(high, low);
        }

        public void Write8Bit(ushort address, byte value)
        {
            switch (address)
            {
                case <= 0x7fff:
                    gameboy.Cartridge.WriteRom(address, value);
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
                    gameboy.InterruptManager.IF = (Interrupt)value;
                    break;
                case <= 0xff7f:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xfffe:
                    hram[(byte)(address & 0x7f)] = value;
                    break;
                case 0xffff:
                    gameboy.InterruptManager.IE = (Interrupt)value;
                    break;
            }
        }

        public void Write16Bit(ushort address, ushort value)
        {
            Write8Bit(address, Utils.GetLowByte(value));
            Write8Bit((ushort)(address + 1), Utils.GetHighByte(value));
        }
    }

    public enum MemoryMappedRegister : ushort
    {
        IF = 0xff0f,
        IE = 0xffff
    }
}
