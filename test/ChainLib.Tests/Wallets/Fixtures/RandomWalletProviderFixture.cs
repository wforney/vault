namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Wallets;
using ChainLib.Wallets.Providers;

public class RandomWalletProviderFixture
{
    public RandomWalletProviderFixture()
    {
        WalletRepositoryFixture repository = new();

        this.Value = new RandomWalletProvider(repository.Value);
    }

    public IWalletProvider Value { get; set; }
}