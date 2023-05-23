using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SharpBoy.Core
{
    public class Mmu
    {
        public bool BiosLoaded { get; set; }


        private byte[] bios = new byte[0x100];
        private byte[] wram = new byte[0x3e00];        
        private byte[] zram = new byte[0x80];

        private Ppu ppu;
        private Cartridge cartridge;

        public Mmu(Ppu ppu, Cartridge cartridge, bool skipBios = false)
        {
            this.cartridge = cartridge;
            BiosLoaded = !skipBios;
        }

        public byte Read8Bit(ushort address)
        {
            return 0;
            //return address switch
            //{
            //    <= 0x1000 => 
            //};


        }

        public ushort Read16Bit(ushort address)
        {
            var low = Read8Bit(address);
            var high = Read8Bit((ushort)(address + 1));
            return Utils.Get16BitValue(high, low);
        }

        public void Write8Bit(ushort address, byte value)
        {

        }

        public void Write16Bit(ushort address, ushort value)
        {
            Write8Bit(address, Utils.GetLowByte(value));
            Write8Bit((ushort)(address + 1), Utils.GetHighByte(value));
        }
    }
}
