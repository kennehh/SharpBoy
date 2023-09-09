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
        private byte[] vram = new byte[0x2000];
        private byte[] oam = new byte[0xa0];

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

            if (address < 0x1800)
            {

            }
        }

        public void WriteOam(byte address, byte value)
        {
            oam[address] = value;
        }

        public void WriteOam(ushort address, byte value)
        {
            throw new NotImplementedException();
        }
    }
}
