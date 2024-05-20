namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Wallets;
using ChainLib.Wallets.Providers;

public class BrainWalletProviderFixture
{
    public BrainWalletProviderFixture()
    {
        WalletRepositoryFixture repository = new();

        this.Value = new BrainWalletProvider(repository.Value);
    }

    public IWalletProvider Value { get; set; }
}