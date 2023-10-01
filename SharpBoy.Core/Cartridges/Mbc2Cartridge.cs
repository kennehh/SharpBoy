using SharpBoy.Core.Memory;
using SharpBoy.Core.Utilities;

namespace SharpBoy.Core.Cartridges
{
    public class Mbc2Cartridge : Cartridge
    {
        private const int RomBankSize = 0x4000;

        private readonly CartridgeHeader header;

        private bool ramEnabled = false;
        private int currentRomBank = 1;

        public Mbc2Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram, 512)
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
                var bank = Math.Max(currentRomBank, 1) % header.RomBanks;
                var bankOffset = bank * RomBankSize;
                return Rom.Read(relativeAddress + bankOffset);
            }
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
                address -= 0xa000;
                return (byte)(Ram.Read(address) & 0x0f);
            }
            return 0xff;
        }

        public override void WriteRam(ushort address, byte value)
        {
            if (ramEnabled && Ram != null)
            {
                address -= 0xa000;
                // Only the lower 4 bits are written
                Ram.Write(address, (byte)(value & 0x0f));
            }
        }
    }
}
