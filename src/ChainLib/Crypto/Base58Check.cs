namespace ChainLib.Crypto
{
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Security.Cryptography;

    /// <summary>
    /// https://github.com/adamcaudill/Base58Check
    /// Base58Check Encoding / Decoding (Bitcoin-style) 
    /// </summary>
    /// <remarks>
    /// See here for more details: https://en.bitcoin.it/wiki/Base58Check_encoding
    /// </remarks>
    public static class Base58Check
    {
        private const int CheckSumSize = 4;
        private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        /// <summary>
        /// Encodes data with a 4-byte checksum
        /// </summary>
        /// <param name="data">Data to be encoded</param>
        /// <returns></returns>
        public static string Encode(byte[] data) => EncodePlain(AddCheckSum(data));

        /// <summary>
        /// Encodes data in plain Base58, without any checksum.
        /// </summary>
        /// <param name="data">The data to be encoded</param>
        /// <returns></returns>
        public static string EncodePlain(byte[] data)
        {
            // Decode byte[] to BigInteger
            BigInteger intData = data.Aggregate<byte, BigInteger>(0, (current, t) => (current * 256) + t);

            // Encode BigInteger to Base58 string
            string result = string.Empty;
            while (intData > 0)
            {
                int remainder = (int)(intData % 58);
                intData /= 58;
                result = Digits[remainder] + result;
            }

            // Append `1` for each leading 0 byte
            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }

            return result;
        }

        /// <summary>
        /// Decodes data in Base58Check format (with 4 byte checksum)
        /// </summary>
        /// <param name="data">Data to be decoded</param>
        /// <returns>Returns decoded data if valid; throws FormatException if invalid</returns>
        public static byte[] Decode(string data)
        {
            byte[] dataWithCheckSum = DecodePlain(data);
            byte[] dataWithoutCheckSum = VerifyAndRemoveCheckSum(dataWithCheckSum);

            return dataWithoutCheckSum ?? throw new FormatException("Base58 checksum is invalid");
        }

        /// <summary>
        /// Decodes data in plain Base58, without any checksum.
        /// </summary>
        /// <param name="data">Data to be decoded</param>
        /// <returns>Returns decoded data if valid; throws FormatException if invalid</returns>
        public static byte[] DecodePlain(string data)
        {
            // Decode Base58 string to BigInteger 
            BigInteger intData = 0;
            for (int i = 0; i < data.Length; i++)
            {
                int digit = Digits.IndexOf(data[i]); //Slow

                if (digit < 0)
                {
                    throw new FormatException(string.Format("Invalid Base58 character `{0}` at position {1}", data[i], i));
                }

                intData = (intData * 58) + digit;
            }

            // Encode BigInteger to byte[]
            // Leading zero bytes get encoded as leading `1` characters
            int leadingZeroCount = data.TakeWhile(c => c == '1').Count();
            System.Collections.Generic.IEnumerable<byte> leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
            System.Collections.Generic.IEnumerable<byte> bytesWithoutLeadingZeros =
                intData.ToByteArray()
                    .Reverse()// to big endian
                    .SkipWhile(b => b == 0);//strip sign byte
            byte[] result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();

            return result;
        }

        private static byte[] AddCheckSum(byte[] data)
        {
            byte[] checkSum = GetCheckSum(data);
            byte[] dataWithCheckSum = ConcatArrays(data, checkSum);
            return dataWithCheckSum;
        }

        //Returns null if the checksum is invalid
        private static byte[] VerifyAndRemoveCheckSum(byte[] data)
        {
            byte[] result = SubArray(data, 0, data.Length - CheckSumSize);
            byte[] givenCheckSum = SubArray(data, data.Length - CheckSumSize);
            byte[] correctCheckSum = GetCheckSum(result);

            return givenCheckSum.SequenceEqual(correctCheckSum) ? result : null;
        }

        private static byte[] GetCheckSum(byte[] data)
        {
            SHA256 sha256 = new SHA256Managed();
            byte[] hash1 = sha256.ComputeHash(data);
            byte[] hash2 = sha256.ComputeHash(hash1);

            byte[] result = new byte[CheckSumSize];
            Buffer.BlockCopy(hash2, 0, result, 0, result.Length);
            return result;
        }

        public static T[] ConcatArrays<T>(params T[][] arrays)
        {
            T[] result = new T[arrays.Sum(arr => arr.Length)];
            int offset = 0;

            foreach (T[] arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            return result;
        }

        public static T[] ConcatArrays<T>(this T[] arr1, T[] arr2)
        {
            T[] result = new T[arr1.Length + arr2.Length];
            Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
            Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);
            return result;
        }

        public static T[] SubArray<T>(this T[] arr, int start, int length)
        {
            T[] result = new T[length];
            Buffer.BlockCopy(arr, start, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] arr, int start) => SubArray(arr, start, arr.Length - start);
    }
}