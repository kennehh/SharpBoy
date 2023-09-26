using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.CartridgeHandling
{
    public class CartridgeHeader
    {
        public string GameTitle { get; private set; }
        public string Licensee { get; private set; }
        public CgbFlag CgbFlag { get; private set; }
        public bool IsSgb { get; private set; }
        public CartridgeType Type { get; private set; }
        public CartridgeHardware HardwareFeatures { get; private set; }
        public int RomSize { get; private set; }
        public int RomBanks { get; private set; }
        public int RamSize { get; private set; }
        public int RamBanks { get; private set; }
        public DestinationCode Destination { get; private set; }

        public void ReadRom(IReadableMemory rom)
        {
            ReadCartridgeType(rom);
            ReadRomSize(rom);
            ReadRamSize(rom);
            ReadTitle(rom);
            ReadCgbFlag(rom);
            ReadSgbFlag(rom);
            ReadDestinationCode(rom);
            ReadLicenseeCode(rom);
        }

        private void ReadTitle(IReadableMemory rom)
        {
            var bytes = new List<byte>();

            for (int i = 0x134; i <= 0x143; i++)
            {
                var value = rom.Read(i);
                if (value == 0)
                {
                    break;
                }
                bytes.Add(value);
            }

            GameTitle = Encoding.ASCII.GetString(bytes.ToArray()).Trim();
        }

        private void ReadCgbFlag(IReadableMemory rom)
        {
            var flag = (CgbFlag)rom.Read(0x143);
            if (!Enum.IsDefined(flag))
            {
                flag = CgbFlag.NotCgb;
            }
            CgbFlag = flag;
        }

        private void ReadSgbFlag(IReadableMemory rom)
        {
            IsSgb = rom.Read(0x146) == 0x03;
        }

        private void ReadCartridgeType(IReadableMemory rom)
        {
            var cartridgeTypeByte = rom.Read(0x0147);
            if (CartridgeHeaderMappings.CartridgeTypes.TryGetValue(cartridgeTypeByte, out var type))
            {
                Type = type.Item1;
                HardwareFeatures = type.Item2;
            }
            else
            {
                throw new Exception($"Unknown cartridge type value: 0x{cartridgeTypeByte:X}");
            }
        }

        private void ReadRomSize(IReadableMemory rom)
        {
            var romSizeByte = rom.Read(0x0148);
            if (CartridgeHeaderMappings.RomSizes.TryGetValue(romSizeByte, out var romSize))
            {
                RomSize = romSize.Item1;
                RomBanks = romSize.Item2;
            }
            else
            {
                throw new Exception($"Unknown rom size value: 0x{romSizeByte:X}");
            }
        }

        private void ReadRamSize(IReadableMemory rom)
        {
            var ramSizeByte = rom.Read(0x0149);
            if (CartridgeHeaderMappings.RomSizes.TryGetValue(ramSizeByte, out var ramSize))
            {
                RamSize = ramSize.Item1;
                RamBanks = ramSize.Item2;
            }
            else
            {
                throw new Exception($"Unknown rom size value: 0x{ramSizeByte:X}");
            }
        }

        private void ReadDestinationCode(IReadableMemory rom)
        {
            Destination = (DestinationCode)rom.Read(0x014A);
            if (!Enum.IsDefined(Destination))
            {
                throw new Exception($"Unknown destination code value: 0x{(byte)Destination:X}");
            }
        }

        private void ReadLicenseeCode(IReadableMemory rom)
        {
            var oldLicenseeCodeByte = rom.Read(0x014B);
            if (oldLicenseeCodeByte == 0x33)
            {
                var newLicenseeBytes = new[] { rom.Read(0x0144), rom.Read(0x0145) };
                var newLicenseeCode = Encoding.ASCII.GetString(newLicenseeBytes);
                if (CartridgeHeaderMappings.NewLicenseeCodes.TryGetValue(newLicenseeCode, out var newLicensee))
                {
                    Licensee = newLicensee;
                }
                else
                {
                    throw new Exception($"Unknown new licensee code value: {newLicenseeCode}");
                }
            }
            else if (CartridgeHeaderMappings.OldLicenseeCodes.TryGetValue(oldLicenseeCodeByte, out var oldLicensee))
            {
                Licensee = oldLicensee;
            }
            else
            {
                throw new Exception($"Unknown old licensee code value: 0x{oldLicenseeCodeByte:X}");
            }
        }
    }

    public enum CartridgeType
    {
        Rom,
        Mbc1,
        Mbc2,
        Mmm01,
        Mbc3,
        Mbc5,
        Mbc6,
        Mbc7,
        PocketCamera,
        BandaiTama5,
        Huc3,
        Huc1
    }

    [Flags]
    public enum CartridgeHardware
    {
        None = 0,
        Ram = 1 << 0,
        Battery = 1 << 1,
        Timer = 1 << 2,
        Rumble = 1 << 3,
        Sensor = 1 << 4
    }

    public enum DestinationCode : byte
    {
        JapanOrOverseas = 0x00,
        OverseasOnly = 0x01
    }

    public enum CgbFlag
    {
        NotCgb = 0,
        CgbBackwardsCompatible = 0x80,
        CgbOnly =  0xC0
    }
}
