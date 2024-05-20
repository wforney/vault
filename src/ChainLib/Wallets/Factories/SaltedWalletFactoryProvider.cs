namespace ChainLib.Wallets.Factories
{
    using ChainLib.Crypto;

    public class SaltedWalletFactoryProvider : IWalletFactoryProvider
    {
        public Wallet Create(string password) => Wallet.FromPassword(password);

        public Wallet Create(params object[] args)
        {
            return args.Length == 0
                ? new Wallet
                {
                    Id = CryptoUtil.RandomString()
                }
                : args.Length != 1 ? null : this.Create(args[0]?.ToString());
        }
    }
}