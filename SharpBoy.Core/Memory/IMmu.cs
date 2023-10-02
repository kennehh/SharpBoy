using SharpBoy.Core.Cartridges.Interfaces;

namespace SharpBoy.Core.Memory
{
    public interface IMmu
    {
        bool BootRomLoaded { get; }
        void LoadBootRom(byte[] rom);
        void LoadCartridge(ICartridge cartridge);
        void Write(int address, byte value);
        byte Read(int address);
    }
}
