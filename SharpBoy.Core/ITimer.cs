namespace SharpBoy.Core
{
    public interface ITimer : ITicker
    {
        byte ReadRegister(ushort address);
        void WriteRegister(ushort address, byte value);
    }
}
