using SharpBoy.Core.Memory;
using SharpBoy.Core.Utilities;
using System.Diagnostics;

namespace SharpBoy.Core.Cartridges
{
    public class Mbc3Cartridge : MbcCartridge
    {
        protected override int CurrentRomBank => currentRomBank;
        protected override int CurrentRamBank => currentRamBank;
        protected override bool RamEnabled => ramEnabled;

        private readonly Mbc3RtcController rtcController;

        private bool ramEnabled = false;
        private bool rtcSelected = false;
        private bool rtcLatchPrepared = false;

        private int currentRomBank = 1;
        private int currentRamBank = 0;
        private RtcRegister currentRtcRegister = 0;

        public Mbc3Cartridge(CartridgeHeader header, IReadableMemory rom, IReadWriteMemory ram) : base(header, rom, ram)
        {
            rtcController = new Mbc3RtcController();
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
                    if (value == 0x00)
                    {
                        rtcLatchPrepared = true;
                    }
                    else if (value == 0x01 && rtcLatchPrepared)
                    {
                        rtcController.Latch();
                        rtcLatchPrepared = false;
                    }
                    else
                    {
                        rtcLatchPrepared = false;
                    }
                    break;
            }
        }

        public override byte ReadRam(ushort address)
        {
            if (ramEnabled && rtcSelected)
            {
                return rtcController.ReadFromRegister(currentRtcRegister);
            }
            else
            {
                return base.ReadRam(address);
            }
        }

        public override void WriteRam(ushort address, byte value)
        {
            if (ramEnabled && rtcSelected)
            {
                rtcController.WriteToRegister(currentRtcRegister, value);
            }
            else
            {
                base.WriteRam(address, value);
            }
        }
    }
}
