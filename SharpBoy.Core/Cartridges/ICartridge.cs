namespace SharpBoy.Core.Cartridges
{
    public interface ICartridge
    {
        CartridgeHeader Header { get; }
        byte ReadRom(ushort address);
        void WriteRom(ushort address, byte value);
        byte ReadERam(ushort address);
        void WriteERam(ushort address, byte value);
    }
}
