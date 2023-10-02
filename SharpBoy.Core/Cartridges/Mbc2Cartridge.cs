using SharpBoy.Core.Memory;
using SharpBoy.Core.Utilities;

namespace SharpBoy.Core.Cartridges
{
    public class Mbc2Cartridge : MbcCartridge
    {
        protected override int CurrentRomBank => currentRomBank;
        protected override int CurrentRamBank => 0;
        protected override bool RamEnabled => ramEnabled;

        private bool ramEnabled = false;
        private int currentRomBank = 1;

        public Mbc2Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram, 512)
        {
        }

        public override void WriteRom(ushort address, byte value)
        {
            if (address <= 0x3fff)
            {
                if ((address & 0x0100) == 0)
                {
                    ramEnabled = (value & 0x0f) == 0x0a;
                }
                else
                {
                    currentRomBank = Math.Max(value & 0x0f, 1);
                }
            }
        }

        public override byte ReadRam(ushort address)
        {
            if (ramEnabled && Ram != null)
            {
                return ConvertTo4BitValue(Ram.Read(address));
            }
            return 0xff;
        }

        public override void WriteRam(ushort address, byte value)
        {
            if (ramEnabled && Ram != null)
            {
                Ram.Write(address, ConvertTo4BitValue(value));
            }
        }

        private static byte ConvertTo4BitValue(byte value)
        {
            var correctValue = 0xf0 + (value & 0x0f);
            return (byte)correctValue;
        }
    }
}
