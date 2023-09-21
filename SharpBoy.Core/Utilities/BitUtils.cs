namespace SharpBoy.Core.Utilities
{
    internal static class BitUtils
    {
        public static byte SetBits(byte value, byte newValue, byte mask) => (byte)(value & ~mask | newValue & mask);
        public static byte SetBit(byte x, int n) => (byte)(x | 1 << n);
        public static byte ClearBit(byte x, int n) => (byte)(x & ~(1 << n));
        public static byte ToggleBit(byte x, int n, bool value) => value ? SetBit(x, n) : ClearBit(x, n);

        public static bool IsBitSet(byte x, int n) => BitValue(x, n) == 1;
        public static int BitValue(byte x, int n) => (x >>> n) & 1;

        public static int ToBit(this bool x) => x ? 1 : 0;

        public static ushort Get16BitValue(byte high, byte low) => (ushort)(high << 8 | low);
        public static byte GetHighByte(ushort value) => (byte)(value >>> 8 & 0xFF);
        public static byte GetLowByte(ushort value) => (byte)(value & 0xFF);
    }
}
