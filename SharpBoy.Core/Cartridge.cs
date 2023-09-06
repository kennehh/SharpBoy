using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Core
{
    public class Cartridge
    {
        private byte[] rom = new byte[0x8000];
        private byte[] eram = new byte[0x2000];

        public Cartridge(byte[] rom)
        {
            Buffer.BlockCopy(rom, 0, this.rom, 0, rom.Length);
        }

        public byte ReadRom(ushort address)
        {
            return rom[address];
        }

        internal void WriteRom(ushort address, byte value)
        {
            rom[address] = value;
        }

        public byte ReadERam(ushort address)
        {
            return eram[address];
        }

        public void WriteERam(ushort address, byte value)
        {
            eram[address] = value;
        }
    }
}
