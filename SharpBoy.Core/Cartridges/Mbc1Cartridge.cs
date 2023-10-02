using SharpBoy.Core.Memory;
using SharpBoy.Core.Utilities;

namespace SharpBoy.Core.Cartridges
{
    public class Mbc1Cartridge : Cartridge
    {
        private const int RomBankSize = 0x4000;
        private const int RamBankSize = 0x2000;

        private readonly CartridgeHeader header;

        private bool ramEnabled = false;
        private bool bankingMode = false;

        private int currentLowerRomBank = 1;
        private int currentUpperRomBank = 0;

        private int currentRamBank = 0;

        public Mbc1Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram)
        {
            this.header = header;
        }

        public override byte ReadRom(ushort address)
        {
            if (address <= 0x3fff)
            {
                if (bankingMode && header.RomSize >= MemorySizes.Bytes1MB)
                {
                    var bank = (currentUpperRomBank << 5) % header.RomBanks;
                    var bankOffset = bank * RomBankSize;
                    return Rom.Read(address + bankOffset);
                }
                return Rom.Read(address);
            }
            else
            {
                var relativeAddress = address - RomBankSize;
                var bank = (currentUpperRomBank << 5) + currentLowerRomBank;
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
                case <= 0x3fff:
                    currentLowerRomBank = Math.Max(value & 0x1f, 1);
                    break;
                case <= 0x5fff:
                    if (header.RomSize >= MemorySizes.Bytes1MB)
                    {
                        currentUpperRomBank = value & 0x03;
                    }
                    else if (header.RamSize >= MemorySizes.Bytes32KB)
                    {
                        currentRamBank = value & 0x03;
                    }
                    break;
                case <= 0x7fff:
                    if (header.RamSize > MemorySizes.Bytes8KB || header.RomSize > MemorySizes.Bytes512KB)
                    {
                        // 00 = Simple Banking Mode (default): 0000–3FFF and A000–BFFF locked to bank 0 of ROM/ RAM
                        // 01 = RAM Banking Mode / Advanced ROM Banking Mode: 0000–3FFF and A000–BFFF can be bank-switched via the 4000–5FFF bank register
                        bankingMode = (value & 0x01) == 1;
                    }
                    break;
            }
        }

        public override byte ReadRam(ushort address)
        {
            if (ramEnabled && Ram != null)
            {
                return Ram.Read(GetERamAddress(address));
            }
            return 0xff;
        }

        public override void WriteRam(ushort address, byte value)
        {
            if (ramEnabled && Ram != null)
            {
                Ram.Write(GetERamAddress(address), value);
            }
        }

        private int GetERamAddress(ushort address)
        {
            var bankOffset = bankingMode ? currentRamBank * RamBankSize : 0;
            return address + bankOffset;
        }
    }
}
