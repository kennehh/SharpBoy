using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Cartridges
{
    public class Mbc3Cartridge : Cartridge
    {
        private const int RomBankSize = 0x4000;
        private const int RamBankSize = 0x2000;

        private readonly CartridgeHeader header;

        private bool ramEnabled = false;
        private bool rtcSelected = false;

        private int currentRomBank = 1;
        private int currentRamBank = 0;
        private RtcRegister currentRtcRegister = 0;

        private Dictionary<RtcRegister, byte> rtcRegisters = new();

        public Mbc3Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram)
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
                var bank = currentRomBank % header.RomBanks;
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
                    currentRomBank = Math.Max(value & 0x7f, 1);
                    break;
                case <= 0x5fff:
                    if (value >= 0x00 && value <= 0x03)
                    {
                        // set RAM bank
                        currentRamBank = value;
                        rtcSelected = false;
                    }
                    else if (value >= 0x08 && value <= 0x0c)
                    {
                        // set RTC register to RAM
                        currentRtcRegister = (RtcRegister)value;
                        rtcSelected = true;
                    }
                    break;
                case <= 0x7fff:
                    var dateTimeNow = DateTime.Now;
                    int totalDays = dateTimeNow.DayOfYear;

                    rtcRegisters[RtcRegister.Seconds] = (byte)dateTimeNow.Second;
                    rtcRegisters[RtcRegister.Minutes] = (byte)dateTimeNow.Minute;
                    rtcRegisters[RtcRegister.Hours] = (byte)dateTimeNow.Hour;
                    rtcRegisters[RtcRegister.Days] = (byte)(totalDays & 0xFF);

                    // Reset the Control register (you might want to preserve other bits if they are set elsewhere)
                    rtcRegisters[RtcRegister.Control] = 0;

                    // Set the 9th bit (Bit 0 of Control register) if needed
                    if ((totalDays & 0x100) != 0) // Checks the 9th bit
                    {
                        rtcRegisters[RtcRegister.Control] |= 0x01; // Sets Bit 0
                    }

                    // Optional: Set the overflow bit (Bit 7 of Control register) if days exceed 511
                    if (totalDays > 511)
                    {
                        rtcRegisters[RtcRegister.Control] |= 0x80; // Sets Bit 7
                    }
                    break;
            }
        }

        public override byte ReadERam(ushort address)
        {
            if (ramEnabled)
            {
                if (rtcSelected)
                {
                    return rtcRegisters[currentRtcRegister];
                }
                else if (ERam != null)
                {
                    return ERam.Read(GetERamAddress(address));
                }
            }
            return 0xff;
        }

        public override void WriteERam(ushort address, byte value)
        {
            if (ramEnabled)
            {
                if (rtcSelected)
                {
                    rtcRegisters[currentRtcRegister] = value;
                }
                else if (ERam != null)
                {
                    ERam.Write(GetERamAddress(address), value);
                }
            }
        }

        private int GetERamAddress(ushort address)
        {
            var bankOffset = currentRamBank * RamBankSize;
            return address + bankOffset;
        }

        private enum RtcRegister
        {
            None = 0,
            Seconds = 0x08,
            Minutes = 0x09,
            Hours = 0x0a,
            Days = 0x0b,
            Control = 0x0c
        }
    }
}
