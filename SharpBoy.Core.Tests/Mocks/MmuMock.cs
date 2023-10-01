using SharpBoy.Core.Cartridges;
using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Tests.Mocks
{
    internal class MmuMock : IMmu
    {
        private byte[] memory;

        public MmuMock(byte[] memory = null)
        {
            this.memory = memory ?? new byte[0x10000];
        }

        public bool BootRomLoaded => false;

        public void LoadBootRom(byte[] rom) { }

        public void LoadCartridge(ICartridge cartridge) { }

        public byte Read(int address) => memory[address];

        public void Write(int address, byte value) => memory[address] = value;
    }
}
