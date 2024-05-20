namespace ChainLib.Wallets.Secrets
{
    using ChainLib.Crypto;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Uses the wallet's password hash to generate a secret.
    /// Necessary for any wallets that need determinism; should be used with care to avoid lazy passwords causing loss of funds.
    /// </summary>
    public class PasswordHashSecretProvider : IWalletSecretProvider
    {
        public byte[] GenerateSecret(params object[] args)
        {
            Wallet wallet = args.FirstOrDefault() as Wallet;
            Contract.Assert(wallet != null);
            Contract.Assert(!string.IsNullOrWhiteSpace(wallet.PasswordHash));
            wallet.Secret = PasswordUtil.FastHash(wallet.PasswordHash, "salt");
            return wallet.Secret;
        }
    }
}