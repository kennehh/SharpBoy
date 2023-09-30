namespace SharpBoy.Core.Cartridges
{
    public interface ICartridgeFactory
    {
        ICartridge CreateCartridge(byte[] data);
    }
}