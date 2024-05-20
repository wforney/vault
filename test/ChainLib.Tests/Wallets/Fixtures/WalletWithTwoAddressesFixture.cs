namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Wallets;
using ChainLib.Wallets.Factories;

public class WalletWithTwoAddressesFixture
{
    public WalletWithTwoAddressesFixture()
    {
        FixedSaltWalletFactoryProvider factory = new(Constants.DefaultFixedSalt16);
        WalletAddressProviderFixture provider = new();
        Wallet wallet = factory.Create("rosebud");

        _ = provider.Value.GenerateAddress(wallet);
        _ = provider.Value.GenerateAddress(wallet);

        this.Value = wallet;
    }

    public Wallet Value { get; set; }
}