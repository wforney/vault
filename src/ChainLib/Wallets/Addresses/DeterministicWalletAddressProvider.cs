namespace ChainLib.Wallets.Addresses
{
    using ChainLib.Crypto;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Uses Ed25519 to provide a deterministic address for a given wallet.
    /// 
    /// This means that given the same private key, the same addresses will be generated in the same order.
    /// This is critical for recovering funds in addresses that are part of the original private key, as 
    /// you would not be able to recover a wallet using a single private key WIF without it.
    /// </summary>
    public class DeterministicWalletAddressProvider : IWalletAddressProvider
    {
        private readonly IWalletSecretProvider _secrets;

        public DeterministicWalletAddressProvider(IWalletSecretProvider secrets) => this._secrets = secrets;

        public string GenerateAddress(Wallet wallet)
        {
            if (wallet.Secret == null || wallet.Secret.Length == 0)
            {
                wallet.Secret = this._secrets.GenerateSecret(wallet);
            }

            // Generate next seed based on the first secret or a new secret from the last key pair
            KeyPair lastKeyPair = wallet.KeyPairs.LastOrDefault();
            byte[] seed = lastKeyPair == null
                ? wallet.Secret
                : PasswordUtil.FastHash(
                    Encoding.UTF8.GetString(lastKeyPair.PrivateKey),
                    Constants.DefaultFixedSalt16);

            System.Tuple<byte[], byte[]> keyPair = Ed25519.GenerateKeyPairFromSecret(seed);

            KeyPair newKeyPair = new KeyPair(
                wallet.KeyPairs.Count + 1,
                keyPair.Item1,
                keyPair.Item2
            );

            wallet.KeyPairs.Add(newKeyPair);
            return newKeyPair.PublicKey.ToHex();
        }
    }
}