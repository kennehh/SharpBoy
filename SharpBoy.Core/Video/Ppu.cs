using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SharpBoy.Core.Video
{

    internal class Ppu : IPpu
    {
        private byte[] vram = new byte[0x2000];
        private byte[] oam = new byte[0xa0];

        private byte lcdc = 0;
        private byte ly = 0x90;
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


        public Ppu()
        {
        }

        public int Step(int cpuCyles)
        {
            var cycles = 0;
            return cycles;
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
                case 0xff41: stat = value; break;
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

    internal enum PpuMode
    {
        HorizontalBlank = 0,
        VerticalBlank = 1,
        ScanlineOam = 2,
        ScanlineVram = 3,
        OneLine,
        FullFrame
    }
}
