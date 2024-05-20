namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Wallets;
using ChainLib.Wallets.Addresses;

public class WalletAddressProviderFixture
{
    public WalletAddressProviderFixture()
    {
        WalletSecretProviderFixture secrets = new();

        this.Value = new DeterministicWalletAddressProvider(secrets.Value);
    }

    public IWalletAddressProvider Value { get; set; }
}