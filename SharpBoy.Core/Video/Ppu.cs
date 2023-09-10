using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SharpBoy.Core.Graphics;

namespace SharpBoy.Core.Graphics
{

    internal class Ppu : IPpu
    {
        private TileBlock tileBlock1 = new TileBlock();
        private TileBlock tileBlock2 = new TileBlock();
        private TileBlock tileBlock3 = new TileBlock();

        private TileMap tileMap1 = new TileMap();
        private TileMap tileMap2 = new TileMap();

        private byte[] vram = new byte[0x2000];
        private byte[] oam = new byte[0xa0];

        public Ppu()
        {
            for (int i = 0; i < 32; i++)
            {
                tileMap1[i] = tileBlock1[0];
                tileMap2[i] = tileBlock1[0];
            }
        }

        public int Step()
        {
            var cycles = 0;
            return cycles;
        }

        public byte ReadVram(ushort address)
        {
            return vram[address];
        }

        public byte ReadOam(byte address)
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
