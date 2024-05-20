namespace ChainLib.Wallets.Providers
{
    using ChainLib.Wallets.Addresses;
    using ChainLib.Wallets.Factories;
    using ChainLib.Wallets.Secrets;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// "Brain wallets" are wallets whose private key is generated through human memory.
    /// These wallets are not secure, because the private key can be obtained by anyone who
    /// memorizes the passphrase, or any machine that guesses the passphrase correctly.
    /// 
    /// They are more convenient for humans, though, since all that is required to gain
    /// access to a wallet address is knowledge of the passphrase, rather than also requiring
    /// a valid copy of the private key.
    /// </summary>
    public class BrainWalletProvider : IWalletProvider
    {
        private readonly IWalletRepository _repository;
        private readonly IWalletSecretProvider _secrets;
        private readonly IWalletAddressProvider _addresses;
        private readonly IWalletFactoryProvider _factory;

        public BrainWalletProvider(IWalletRepository repository, string salt = Constants.DefaultFixedSalt16)
        {
            this._repository = repository;
            this._secrets = new PasswordHashSecretProvider();
            this._addresses = new DeterministicWalletAddressProvider(this._secrets);
            this._factory = new FixedSaltWalletFactoryProvider(salt);
        }

        public string GenerateAddress(Wallet wallet) => this._addresses.GenerateAddress(wallet);

        public byte[] GenerateSecret(params object[] args) => this._secrets.GenerateSecret(args);

        public Wallet Create(params object[] args) => this._factory.Create(args);

        public Task<IEnumerable<Wallet>> GetAllAsync() => this._repository.GetAllAsync();

        public Task<Wallet> GetByIdAsync(string id) => this._repository.GetByIdAsync(id);

        public Task<Wallet> AddAsync(Wallet wallet) => this._repository.AddAsync(wallet);

        public Task SaveAddressesAsync(Wallet wallet) => this._repository.SaveAddressesAsync(wallet);
    }
}