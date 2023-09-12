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
        protected byte Stat
        {
            get => (byte)((byte)statInterruptSource | (byte)currentStatus | (lyEqualsLyc.ToBit() << 2));
            private set
            {
                statInterruptSource = (StatInterruptSourceFlag)(value & 0b01111000);
                lyEqualsLyc = BitUtils.IsBitSet(value, 2);
                currentStatus = (PpuStatus)(value & 0b00000011);
            }
        }

        private StatInterruptSourceFlag statInterruptSource = StatInterruptSourceFlag.None;
        private bool lyEqualsLyc = false;
        private PpuStatus currentStatus = PpuStatus.HorizontalBlank;

        private byte[] vram = new byte[0x2000];
        private byte[] oam = new byte[0xa0];

        private byte[] frameBuffer = new byte[160 * 144];

        private byte lcdc = 0;
        private byte ly = 0;
        private byte lyc = 0;
        private byte stat = 0;

        private byte scy = 0;
        private byte scx = 0;

        private byte wy = 0;
        private byte wx = 0;

        private byte bgp = 0;

        private byte obp0 = 0;
        private byte obp1 = 0;

        private byte dma = 0;


        private int cycles;
        private readonly IInterruptManager interruptManager;

        public Ppu(IInterruptManager interruptManager)
        {
            this.interruptManager = interruptManager;
        }

        public void Sync(int cpuCyles)
        {
            var previousStatus = currentStatus;
            cycles += cpuCyles;            

            if (ly <= 143)
            {
                switch (cycles)
                {
                    case < 80:
                        currentStatus = PpuStatus.SearchingOam;                        
                        break;
                    case < 252:
                        // could take from 172 to 289 cycles, defaulting to 172 for now
                        currentStatus = PpuStatus.Drawing;
                        break;
                    case < 456:
                        // could take from 87 to 204 cycles, defaulting to 204 for now
                        currentStatus = PpuStatus.HorizontalBlank;
                        break;
                    default:
                        ly++;
                        cycles -= 456;
                        break;
                }
            }
            else
            {
                currentStatus = PpuStatus.VerticalBlank;
                if (currentStatus != previousStatus)
                {
                    interruptManager.RequestInterrupt(InterruptFlag.VBlank);
                }

                if (cycles >= 456)
                {
                    if (ly >= 153)
                    {
                        ly = 0;
                    }
                    else
                    {
                        ly++;
                    }
                    cycles -= 456;
                }
            }

            lyEqualsLyc = ly == lyc;
            HandleStatInterrupt();
        }

        private void HandleStatInterrupt()
        {
            if (statInterruptSource != StatInterruptSourceFlag.None)
            {
                StatInterruptSourceFlag? interrupt = null;

                if (currentStatus == PpuStatus.HorizontalBlank && statInterruptSource.HasFlag(StatInterruptSourceFlag.HorizontalBlank))
                {
                    interrupt = StatInterruptSourceFlag.HorizontalBlank;
                }
                else if (currentStatus == PpuStatus.VerticalBlank && statInterruptSource.HasFlag(StatInterruptSourceFlag.VerticalBlank))
                {
                    interrupt = StatInterruptSourceFlag.VerticalBlank;
                }
                else if (currentStatus == PpuStatus.SearchingOam && statInterruptSource.HasFlag(StatInterruptSourceFlag.SearchingOam))
                {
                    interrupt = StatInterruptSourceFlag.SearchingOam;
                }
                else if (lyEqualsLyc && statInterruptSource.HasFlag(StatInterruptSourceFlag.LyEqualsLyc))
                {
                    interrupt = StatInterruptSourceFlag.LyEqualsLyc;
                }

                if (interrupt.HasValue)
                {
                    statInterruptSource &= ~interrupt.Value;
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
                0xff40 => lcdc,
                0xff41 => stat,
                0xff42 => scy,
                0xff43 => scx,
                0xff44 => ly,
                0xff45 => lyc,
                0xff46 => dma,
                0xff47 => bgp,
                0xff48 => obp0,
                0xff49 => obp1,
                0xff4a => wy,
                0xff4b => wx,
                _ => throw new NotImplementedException(),
            };
        }

        public void WriteRegister(ushort address, byte value)
        {
            switch (address)
            {
                case 0xff40: lcdc = value; break;
                case 0xff41: BitUtils.SetBits(stat, value, 0b01111000); break;
                case 0xff42: scy = value; break;
                case 0xff43: scx = value; break;
                case 0xff44: break;
                case 0xff45: lyc = value; break;
                case 0xff46: dma = value; break;
                case 0xff47: bgp = value; break;
                case 0xff48: obp0 = value; break;
                case 0xff49: obp1 = value; break;
                case 0xff4a: wy = value; break;
                case 0xff4b: wx = value; break;
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
