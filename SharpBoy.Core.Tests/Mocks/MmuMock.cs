using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void LoadBootRom(byte[] rom) { }

        public byte Read(int address) => memory[address];

        public void Write(int address, byte value) => memory[address] = value;
    }
}
