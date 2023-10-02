using SharpBoy.Core.Memory;
using SharpBoy.Core.Utilities;
using System.Data;
using System.Diagnostics;

namespace SharpBoy.Core.Cartridges
{
    public class Mbc5Cartridge : Cartridge
    {
        private const int RomBankSize = 0x4000;
        private const int RamBankSize = 0x2000;
        private readonly CartridgeHeader header;

        private bool ramEnabled = false;

        private int currentLowerRomBank = 1;
        private int currentUpperRomBank = 0;
        private int currentRamBank = 0;

        private bool rumble = false;

        public Mbc5Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram)
        {
            this.header = header;
        }

        public override byte ReadRom(ushort address)
        {
            if (address <= 0x3fff)
            {
                return Rom.Read(address);
            }
            else
            {
                var relativeAddress = address - RomBankSize;
                var bank = (currentUpperRomBank << 8) + currentLowerRomBank;
                var bankOffset = bank * RomBankSize;
                return Rom.Read(relativeAddress + bankOffset);
            }
        }

        public override void WriteRom(ushort address, byte value)
        {
            switch (address)
            {
                case <= 0x1fff:
                    ramEnabled = (value & 0x0f) == 0x0a;
                    break;
                case <= 0x2fff:
                    currentLowerRomBank = value;
                    break;
                case <= 0x3fff:
                    currentUpperRomBank = value & 0x01;
                    break;
                case <= 0x5fff:
                    if (header.HardwareFeatures.HasFlag(CartridgeHardware.Rumble))
                    {
                        currentRamBank = value & 0x07;
                        rumble = (value & 0x8) != 0;
                    }
                    else
                    {
                        currentRamBank = value & 0x0f;
                    }
                    break;
            }
        }

        public override byte ReadRam(ushort address)
        {
            if (ramEnabled)
            {
                return Ram.Read(GetERamAddress(address));
            }
            return 0xff;
        }

        public override void WriteRam(ushort address, byte value)
        {
            if (ramEnabled)
            {
                Ram.Write(GetERamAddress(address), value);
            }
        }

        private int GetERamAddress(ushort address)
        {
            var bankOffset = currentRamBank * RamBankSize;
            return address + bankOffset;
        }
    }
}
