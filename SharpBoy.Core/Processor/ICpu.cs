namespace SharpBoy.Core.Processor
{
    public interface ICpu
    {
        CpuRegisters Registers { get; }
        int Step();
        void ResetState(bool bootRomLoaded);
    }
}
