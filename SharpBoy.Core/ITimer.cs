namespace SharpBoy.Core
{
    internal interface ITimer
    {
        void Update(int cycles);
        byte ReadRegister(ushort address);
        void WriteRegister(ushort address, byte value);
    }
}
