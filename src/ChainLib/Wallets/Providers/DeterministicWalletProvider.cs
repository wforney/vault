﻿namespace ChainLib.Wallets.Providers
{
    using ChainLib.Wallets.Addresses;
    using ChainLib.Wallets.Factories;
    using ChainLib.Wallets.Secrets;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DeterministicWalletProvider : IWalletProvider
    {
        private readonly IWalletRepository _repository;
        private readonly IWalletSecretProvider _secrets;
        private readonly IWalletAddressProvider _addresses;
        private readonly IWalletFactoryProvider _factory;

        public DeterministicWalletProvider(IWalletRepository repository, ushort bitsOfEntropy = 256)
        {
            this._repository = repository;
            this._secrets = new RandomWalletSecretProvider(bitsOfEntropy);
            this._addresses = new DeterministicWalletAddressProvider(this._secrets);
            this._factory = new SaltedWalletFactoryProvider();
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