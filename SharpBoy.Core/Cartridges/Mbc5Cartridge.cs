using SharpBoy.Core.Memory;
using SharpBoy.Core.Utilities;
using System.Data;
using System.Diagnostics;

namespace SharpBoy.Core.Cartridges
{
    public class Mbc5Cartridge : MbcCartridge
    {
        protected override int CurrentRomBank => (currentUpperRomBank << 8) + currentLowerRomBank;
        protected override int CurrentRamBank => currentRamBank;
        protected override bool RamEnabled => ramEnabled;

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
    }
}
