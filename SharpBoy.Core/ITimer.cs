namespace SharpBoy.Core
{
    internal interface ITimer
    {
        void Sync(int cycles);
        byte ReadRegister(ushort address);
        void WriteRegister(ushort address, byte value);
    }
}
