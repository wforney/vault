namespace ChainLib.Wallets
{
    using ChainLib.Crypto;
    using System.Collections.Generic;
    using System.Linq;

    public class Wallet
    {
        public string Id { get; internal set; }
        public string PasswordHash { get; internal set; }
        public byte[] Secret { get; set; }
        public IList<KeyPair> KeyPairs { get; } = new List<KeyPair>();

        internal Wallet() { /* Required for serialization */ }

        public byte[] GetAddressByIndex(int index) => this.KeyPairs.SingleOrDefault(x => x.Index == index)?.PublicKey;

        public byte[] GetAddressByPublicKey(byte[] publicKey) => this.KeyPairs.SingleOrDefault(x => x.PublicKey.ConstantTimeEquals(publicKey))?.PublicKey;

        public byte[] GetPrivateKeyByAddress(byte[] publicKey) => this.KeyPairs.SingleOrDefault(x => x.PublicKey.ConstantTimeEquals(publicKey))?.PrivateKey;

        public IEnumerable<byte[]> GetAddresses() => this.KeyPairs.Select(x => x.PublicKey);

        internal static Wallet FromPassword(string password, string salt = null)
        {
            Wallet wallet = new Wallet
            {
                Id = CryptoUtil.RandomString(),
                PasswordHash = PasswordUtil.StorageHash(password, salt)
            };
            return wallet;
        }

        internal static Wallet FromPasswordHash(string passwordHash)
        {
            Wallet wallet = new Wallet
            {
                Id = CryptoUtil.RandomString(),
                PasswordHash = passwordHash
            };
            return wallet;
        }
    }
}