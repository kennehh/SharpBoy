namespace SharpBoy.Core.Cartridges
{
    public interface ICartridge
    {
        CartridgeHeader Header { get; }
        byte ReadRom(ushort address);
        void WriteRom(ushort address, byte value);
        byte ReadRam(ushort address);
        void WriteRam(ushort address, byte value);
    }
}
