namespace SharpBoy.Core.Cartridges
{
    public interface ICartridgeFactory
    {
        ICartridge CreateCartridge(byte[] romData, byte[] ramData = null);
    }
}