namespace ChainLib.Tests.Wallets;

using ChainLib.Crypto;
using ChainLib.Tests.Wallets.Fixtures;
using ChainLib.Wallets;
using Xunit;

public class WhenRandomWalletIsUsed : IClassFixture<RandomWalletProviderFixture>
{
    private readonly RandomWalletProviderFixture _provider;

    public WhenRandomWalletIsUsed(RandomWalletProviderFixture provider) => this._provider = provider;

    [Theory]
    [InlineData("purple monkey dishwasher")]
    public void The_wallet_password_hash_is_verifiable(string passphrase)
    {
        Wallet wallet1 = this._provider.Value.Create(passphrase);
        Wallet wallet2 = this._provider.Value.Create(passphrase);

        Assert.True(PasswordUtil.Verify(passphrase, wallet1.PasswordHash));
        Assert.True(PasswordUtil.Verify(passphrase, wallet2.PasswordHash));
    }
}