using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpBoy.Core.Memory
{
    public interface IWritableMemory
    {
        void Write(int address, byte value);
    }

    public interface IReadableMemory
    {
        byte Read(int address);
    }

    public interface IReadWriteMemory : IReadableMemory, IWritableMemory { }

    internal abstract class Memory
    {
        private byte[] memory;
        private ushort addressMask;

        protected Memory(int size)
        {
            memory = new byte[size];
            addressMask = (ushort)(size - 1);
        }

        protected Memory(byte[] data) : this(data.Length)
        {
            Buffer.BlockCopy(data, 0, memory, 0, data.Length);
        }

        protected void WriteToMemory(int address, byte value)
        {
            memory[address & addressMask] = value;
        }

        protected byte ReadFromMemory(int address)
        {
            return memory[address & addressMask];
        }
    }

    internal class Ram : Memory, IReadWriteMemory
    {
        public Ram(int size) : base(size) { }
        public Ram(byte[] rom) : base(rom) { }
        public byte Read(int address) => ReadFromMemory(address);
        public void Write(int address, byte value) => WriteToMemory(address, value);
    }

    internal class Rom : Memory, IReadableMemory
    {
        public Rom(byte[] rom) : base(rom) { }
        public byte Read(int address) => ReadFromMemory(address);
    }
}
