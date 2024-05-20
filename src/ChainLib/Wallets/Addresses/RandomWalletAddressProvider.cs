namespace ChainLib.Wallets.Addresses
{
    using ChainLib.Crypto;
    using System.Security.Cryptography;

    public class RandomWalletAddressProvider : IWalletAddressProvider
    {
        private readonly ushort _buffer;

        public RandomWalletAddressProvider(ushort bitsOfEntropy = 256) => this._buffer = (ushort)(bitsOfEntropy / 8);

        public string GenerateAddress(Wallet wallet)
        {
            byte[] randomBytes = new byte[this._buffer];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return randomBytes.ToHex();
        }
    }
}