using SharpBoy.Core.Cartridge;
using SharpBoy.Core.Graphics;
using SharpBoy.Core.Interrupts;
using SharpBoy.Core.Timing;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace SharpBoy.Core.Memory
{
    public class Mmu : IMmu
    {
        private readonly IPpu ppu;
        private readonly ITimer timer;
        private readonly ICartridgeReader cartridgeReader;
        private readonly IInterruptManager interruptManager;
        private IReadableMemory bootRom = null;
        private IReadWriteMemory wram = new Ram(0x2000);
        private IReadWriteMemory hram = new Ram(0x80);
        
        // temporary
        private byte[] ioRegisters = new byte[0x10000];

        private bool bootRomLoaded = false;
        public bool BootRomLoaded => bootRomLoaded;

        public Mmu(IPpu ppu, ITimer timer, ICartridgeReader cartridgeReader, IInterruptManager interruptManager)
        {
            this.ppu = ppu;
            this.timer = timer;
            this.cartridgeReader = cartridgeReader;
            this.interruptManager = interruptManager;

            Array.Fill<byte>(ioRegisters, 0xff);
        }

        public void LoadBootRom(byte[] rom)
        {
            bootRom = new Rom(rom);
            bootRomLoaded = true;
        }

        public byte Read(int addr)
        {
            var address = (ushort)addr;
            return address switch
            {
                <= 0x7fff => ReadRom(address),
                <= 0x9fff => ppu.ReadVram(address),
                <= 0xbfff => cartridgeReader.ReadERam(address),
                <= 0xcfff => wram.Read(address),
                <= 0xdfff => wram.Read(address), // In CGB mode, switchable bank 1~7 ;
                <= 0xfdff => wram.Read(address), // copy of wram (echo ram) - use of this area should be prohibited
                <= 0xfe9f => ppu.ReadOam(address),
                <= 0xfeff => 0, // use of this area should be prohibited
                <= 0xff03 => ioRegisters[address], // handle I/O registers here
                <= 0xff07 => timer.ReadRegister(address),
                <= 0xff0e => ioRegisters[address], // handle I/O registers here
                <= 0xff0f => (byte)interruptManager.IF,
                <= 0xff3f => ioRegisters[address], // handle I/O registers here
                <= 0xff4b => ppu.ReadRegister(address),
                <= 0xff7f => ioRegisters[address], // handle I/O registers here
                <= 0xfffe => hram.Read(address),
                <= 0xffff => (byte)interruptManager.IE
            };
        }

        public void Write(int addr, byte value)
        {
            var address = (ushort)addr;
            switch (address)
            {
                case <= 0x7fff:
                    break;
                case <= 0x9fff:
                    // In CGB mode, switchable bank 0/1
                    ppu.WriteVram(address, value);
                    break;
                case <= 0xbfff:
                    cartridgeReader.WriteERam(address, value);
                    break;
                case <= 0xcfff:
                    wram.Write(address, value);
                    break;
                case <= 0xdfff:
                    // In CGB mode, switchable bank 1~7
                    wram.Write(address, value);
                    break;
                case <= 0xfdff:
                    // copy of wram (echo ram) - use of this area should be prohibited
                    wram.Write(address, value);
                    break;
                case <= 0xfe9f:
                    ppu.WriteOam(address, value);
                    break;
                case <= 0xfeff:
                    // use of this area should be prohibited
                    break;
                case <= 0xff03:
                    if (address != 0xff00)
                    {
                        ioRegisters[address] = value;
                    }
                    break;
                case <= 0xff07:
                    timer.WriteRegister(address, value);
                    break;
                case <= 0xff0e:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xff0f:
                    interruptManager.IF = (InterruptFlag)value;
                    break;
                case <= 0xff3f:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xff4b:
                    ppu.WriteRegister(address, value);
                    break;
                case 0xff50:
                    bootRomLoaded = false;
                    break;
                case <= 0xff7f:
                    // handle I/O registers here
                    ioRegisters[address] = value;
                    break;
                case <= 0xfffe:
                    hram.Write(address, value);
                    break;
                case 0xffff:
                    interruptManager.IE = (InterruptFlag)value;
                    break;
            }
        }

        private byte ReadRom(ushort address)
        {
            if (bootRomLoaded && address < 0x100)
            {
                return bootRom.Read(address);
            }
            return cartridgeReader.ReadRom(address);
        }
    }
}
