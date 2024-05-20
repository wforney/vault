namespace ChainLib.Wallets.Factories
{
    using System.Diagnostics.Contracts;

    public class FixedSaltWalletFactoryProvider : IWalletFactoryProvider
    {
        private readonly string _salt;

        public FixedSaltWalletFactoryProvider(string salt)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(salt));
            Contract.Assert(salt.Length == 16);
            this._salt = salt;
        }

        public Wallet Create(string password)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(password));
            return Wallet.FromPassword(password, this._salt);
        }

        public Wallet Create(params object[] args) => args.Length != 1 ? null : this.Create(args[0]?.ToString());
    }
}