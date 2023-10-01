﻿namespace SharpBoy.Core.Memory
{
    public interface IWritableMemory
    {
        void Write(int address, byte value);
        void Copy(byte[] source);
    }

    public interface IReadableMemory
    {
        byte Read(int address);
    }

    public interface IReadWriteMemory : IReadableMemory, IWritableMemory { }

    internal abstract class Memory
    {
        private readonly byte[] memory;
        private readonly bool useBitwiseAnd;

        protected Memory(int size)
        {
            memory = new byte[size];
            useBitwiseAnd = IsPowerOfTwo(size);
        }

        protected Memory(byte[] data) : this(data.Length)
        {
            CopyToMemory(data);
        }

        protected void WriteToMemory(int address, byte value)
        {
            memory[MapAddress(address)] = value;
        }

        protected byte ReadFromMemory(int address)
        {
            return memory[MapAddress(address)];
        }

        protected void CopyToMemory(byte[] source)
        {
            Buffer.BlockCopy(source, 0, memory, 0, source.Length);
        }

        private static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        private int MapAddress(int address)
        {
            if (useBitwiseAnd)
            {
                return address & (memory.Length - 1);
            }
            else
            {
                return address % memory.Length;
            }
        }
    }

    internal sealed class Ram : Memory, IReadWriteMemory
    {
        public Ram(int size) : base(size) { }
        public Ram(byte[] rom) : base(rom) { }
        public byte Read(int address) => ReadFromMemory(address);
        public void Write(int address, byte value) => WriteToMemory(address, value);
        public void Copy(byte[] source) => CopyToMemory(source);
    }

    internal sealed class Rom : Memory, IReadableMemory
    {
        public Rom(byte[] rom) : base(rom) { }
        public byte Read(int address) => ReadFromMemory(address);
    }
}
