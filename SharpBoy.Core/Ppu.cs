using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core
{
    public class Ppu
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
    }
}
