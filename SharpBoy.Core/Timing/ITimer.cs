namespace SharpBoy.Core.Timing
{
    public interface ITimer
    {
        byte ReadRegister(ushort address);
        void WriteRegister(ushort address, byte value);
        void Tick();
    }
}
