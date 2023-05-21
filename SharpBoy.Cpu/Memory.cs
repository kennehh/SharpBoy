using System;
using System.Collections.Generic;
using System.Text;

namespace SharpBoy.Cpu
{
    internal class Memory
    {
        private byte[] memory;

        public Memory(int size)
        {
            memory = new byte[size];
        }

        public byte Read8Bit(ushort address) => memory[address];

        public ushort Read16Bit(ushort address)
        {
            var low = memory[address];
            var high = memory[(ushort)(address + 1)];
            return Utils.Get16BitValue(high, low);
        }

        public void Write8Bit(ushort address, byte value) => memory[address] = value;

        public void Write16Bit(ushort address, ushort value)
        {
            memory[address] = Utils.GetLowByte(value);
            memory[(ushort)(address + 1)] = Utils.GetHighByte(value);
        }
    }
}
