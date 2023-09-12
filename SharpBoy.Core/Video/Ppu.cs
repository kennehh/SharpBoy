using SharpBoy.Core.Processor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpBoy.Core.Video
{

    internal class Ppu : IPpu
    {
        internal PpuRegisters Registers { get; }

        private byte[] vram = new byte[0x2000];
        private byte[] oam = new byte[0xa0];
        private byte[] frameBuffer = new byte[160 * 144];

        private int cycles;
        private readonly IInterruptManager interruptManager;

        public Ppu(IInterruptManager interruptManager)
        {
            Registers = new PpuRegisters();
            this.interruptManager = interruptManager;
        }

        public void Sync(int cpuCyles)
        {
            var previousStatus = Registers.CurrentStatus;
            cycles += cpuCyles;            

            if (Registers.LY <= 143)
            {
                switch (cycles)
                {
                    case < 80:
                        Registers.CurrentStatus = PpuStatus.SearchingOam;                        
                        break;
                    case < 252:
                        // could take from 172 to 289 cycles, defaulting to 172 for now
                        Registers.CurrentStatus = PpuStatus.Drawing;
                        break;
                    case < 456:
                        // could take from 87 to 204 cycles, defaulting to 204 for now
                        Registers.CurrentStatus = PpuStatus.HorizontalBlank;
                        break;
                    default:
                        Registers.LY++;
                        cycles -= 456;
                        break;
                }
            }
            else
            {
                Registers.CurrentStatus = PpuStatus.VerticalBlank;
                if (Registers.CurrentStatus != previousStatus)
                {
                    interruptManager.RequestInterrupt(InterruptFlag.VBlank);
                }

                if (cycles >= 456)
                {
                    if (Registers.LY >= 153)
                    {
                        Registers.LY = 0;
                    }
                    else
                    {
                        Registers.LY++;
                    }
                    cycles -= 456;
                }
            }

            HandleStatInterrupt();
        }

        private void HandleStatInterrupt()
        {
            if (Registers.StatInterruptSource != StatInterruptSourceFlag.None)
            {
                StatInterruptSourceFlag? interrupt = null;

                if (Registers.CurrentStatus == PpuStatus.HorizontalBlank && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.HorizontalBlank))
                {
                    interrupt = StatInterruptSourceFlag.HorizontalBlank;
                }
                else if (Registers.CurrentStatus == PpuStatus.VerticalBlank && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.VerticalBlank))
                {
                    interrupt = StatInterruptSourceFlag.VerticalBlank;
                }
                else if (Registers.CurrentStatus == PpuStatus.SearchingOam && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.SearchingOam))
                {
                    interrupt = StatInterruptSourceFlag.SearchingOam;
                }
                else if (Registers.LyCompareFlag && Registers.StatInterruptSource.HasFlag(StatInterruptSourceFlag.LyEqualsLyc))
                {
                    interrupt = StatInterruptSourceFlag.LyEqualsLyc;
                }

                if (interrupt.HasValue)
                {
                    Registers.StatInterruptSource &= ~interrupt.Value;
                    interruptManager.RequestInterrupt(InterruptFlag.LcdStat);
                }
            }
        }

        public byte ReadVram(ushort address)
        {
            return vram[address];
        }

        public byte ReadOam(ushort address)
        {
            return oam[address];
        }

        public void WriteVram(ushort address, byte value)
        {
            vram[address] = value;
        }

        public void WriteOam(ushort address, byte value)
        {
            oam[address] = value;
        }

        public byte ReadRegister(ushort address)
        {
            return address switch
            {
                0xff40 => Registers.LCDC,
                0xff41 => Registers.STAT,
                0xff42 => Registers.SCY,
                0xff43 => Registers.SCX,
                0xff44 => Registers.LY,
                0xff45 => Registers.LYC,
                0xff46 => Registers.DMA,
                0xff47 => Registers.BGP,
                0xff48 => Registers.OBP0,
                0xff49 => Registers.OBP1,
                0xff4a => Registers.WY,
                0xff4b => Registers.WX,
                _ => throw new NotImplementedException(),
            };
        }

        public void WriteRegister(ushort address, byte value)
        {
            switch (address)
            {
                case 0xff40: Registers.LCDC = value; break;
                case 0xff41: BitUtils.SetBits(Registers.STAT, value, 0b01111000); break;
                case 0xff42: Registers.SCY = value; break;
                case 0xff43: Registers.SCX = value; break;
                case 0xff44: break;
                case 0xff45: Registers.LYC = value; break;
                case 0xff46: Registers.DMA = value; break;
                case 0xff47: Registers.BGP = value; break;
                case 0xff48: Registers.OBP0 = value; break;
                case 0xff49: Registers.OBP1 = value; break;
                case 0xff4a: Registers.WY = value; break;
                case 0xff4b: Registers.WX = value; break;
                default: throw new NotImplementedException();
            }
        }
    }

    internal enum PpuStatus : byte
    {
        HorizontalBlank = 0,
        VerticalBlank = 1,
        SearchingOam = 2,
        Drawing = 3,
    }

    [Flags]
    internal enum StatInterruptSourceFlag : byte
    { 
        None = 0,
        HorizontalBlank = 1 << 3,
        VerticalBlank = 1 << 4,
        SearchingOam = 1 << 5,
        LyEqualsLyc = 1 << 6
    }
}
