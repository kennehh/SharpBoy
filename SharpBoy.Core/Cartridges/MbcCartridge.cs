using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Cartridges
{
    public abstract class MbcCartridge : Cartridge
    {
        protected abstract int CurrentRomBank { get; }
        protected abstract int CurrentRamBank { get; }
        protected abstract bool RamEnabled { get; }

        protected const int RomBankSize = 0x4000;
        protected const int RamBankSize = 0x2000;

        protected MbcCartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram)
        {
        }

        protected MbcCartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram, int ramSize) : base(header, rom, ram, ramSize)
        {
        }

        public override byte ReadRom(ushort address)
        {
            if (address <= 0x3fff)
            {
                return ReadFixedRom(address);
            }
            else
            {
                return ReadBankedRom(address);
            }
        }

        public override byte ReadRam(ushort address)
        {
            if (RamEnabled && Ram != null)
            {
                return Ram.Read(GetERamAddress(address));
            }
            return 0xff;
        }

        public override void WriteRam(ushort address, byte value)
        {
            if (RamEnabled && Ram != null)
            {
                Ram.Write(GetERamAddress(address), value);
            }
        }

        protected virtual byte ReadFixedRom(ushort address)
        {
            return Rom.Read(address);
        }

        protected virtual byte ReadBankedRom(ushort address)
        {
            var relativeAddress = address - RomBankSize;
            var bankOffset = CurrentRomBank * RomBankSize;
            return Rom.Read(relativeAddress + bankOffset);
        }

        private int GetERamAddress(ushort address)
        {
            var bankOffset = CurrentRamBank * RamBankSize;
            return address + bankOffset;
        }
    }
}
