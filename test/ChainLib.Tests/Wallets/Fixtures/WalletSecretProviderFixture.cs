namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Wallets;
using ChainLib.Wallets.Secrets;

public class WalletSecretProviderFixture
{
    public WalletSecretProviderFixture() => this.Value = new PasswordHashSecretProvider();

    public IWalletSecretProvider Value { get; set; }
}