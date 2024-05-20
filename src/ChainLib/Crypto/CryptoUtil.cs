namespace ChainLib.Crypto
{
    using Sodium;
    using System.Text;

    public static class CryptoUtil
    {
        public static string RandomString(int size = 64) => RandomBytes(size / 2).ToHex();

        public static byte[] RandomBytes(int size) => SodiumCore.GetRandomBytes(size);

        public static string ToHex(this byte[] input) => Utilities.BinaryToHex(input);

        public static byte[] FromHex(this string input) => Utilities.HexToBinary(input);

        public static byte[] Sha256(this string input) => Encoding.UTF8.GetBytes(input).Sha256();

        public static byte[] Sha256(this byte[] input) => CryptoHash.Sha256(input);

        public static bool ConstantTimeEquals(this byte[] a, byte[] b) => Utilities.Compare(a, b);
    }
}