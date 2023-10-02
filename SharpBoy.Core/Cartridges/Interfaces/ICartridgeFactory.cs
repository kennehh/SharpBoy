namespace SharpBoy.Core.Cartridges.Interfaces
{
    public interface ICartridgeFactory
    {
        ICartridge CreateCartridge(byte[] romData, byte[] ramData = null);
    }
}