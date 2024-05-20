namespace ChainLib.Tests.Wallets;

using ChainLib.Crypto;
using ChainLib.Tests.Wallets.Fixtures;
using ChainLib.Wallets;
using ChainLib.Wallets.Factories;
using ChainLib.Wallets.StorageFormats;
using Xunit;

public class WhenWalletIsCreatedWithTwoAddresses :
        IClassFixture<WalletWithTwoAddressesFixture>,
        IClassFixture<WifAddressStorageFormatFixture>,
        IClassFixture<KeyStoreStorageFormatFixture>
{
    private readonly WifAddressStorageFormatFixture _wif;
    private readonly KeyStoreStorageFormatFixture _keystore;

    public WhenWalletIsCreatedWithTwoAddresses(
        WalletWithTwoAddressesFixture fixture,
        WifAddressStorageFormatFixture wif,
        KeyStoreStorageFormatFixture keystore)
    {
        this._wif = wif;

        this._keystore = keystore;
        this.Fixture = fixture;
    }

    public WalletWithTwoAddressesFixture Fixture { get; set; }

    [Fact]
    public void There_are_two_keypairs_in_the_wallet()
    {
        Assert.NotNull(this.Fixture.Value);
        Assert.Equal(2, this.Fixture.Value.KeyPairs.Count);
    }

    [Fact]
    public void Both_addresses_can_be_exported_via_WIF()
    {
        Wallet wallet1 = this.Fixture.Value;
        string wif1 = this._wif.Value.Export(wallet1, wallet1.KeyPairs[0].PublicKey);
        string wif2 = this._wif.Value.Export(wallet1, wallet1.KeyPairs[1].PublicKey);

        Assert.Equal(wallet1.KeyPairs[0].PrivateKey.ToHex(), WifAddressStorageFormat.GetPrivateKeyFromImport(wif1, Constants.WifKeyLength).ToHex());
        Assert.Equal(wallet1.KeyPairs[1].PrivateKey.ToHex(), WifAddressStorageFormat.GetPrivateKeyFromImport(wif2, Constants.WifKeyLength).ToHex());
    }

    [Fact]
    public void Both_addresses_can_be_imported_via_WIF()
    {
        Wallet wallet1 = this.Fixture.Value;
        string wif1 = this._wif.Value.Export(wallet1, wallet1.KeyPairs[0].PublicKey);
        string wif2 = this._wif.Value.Export(wallet1, wallet1.KeyPairs[1].PublicKey);

        FixedSaltWalletFactoryProvider factory = new(Constants.DefaultFixedSalt16);
        Wallet wallet2 = factory.Create("rosebud");
        _ = this._wif.Value.Import(wallet2, wif1, Constants.WifKeyLength);
        _ = this._wif.Value.Import(wallet2, wif2, Constants.WifKeyLength);

        Assert.Equal(wallet1.KeyPairs.Count, wallet2.KeyPairs.Count);
        Assert.Equal(wallet1.KeyPairs[0].PublicKey.ToHex(), wallet2.KeyPairs[0].PublicKey.ToHex());
        Assert.Equal(wallet1.KeyPairs[0].PrivateKey.ToHex(), wallet2.KeyPairs[0].PrivateKey.ToHex());
        Assert.Equal(wallet1.KeyPairs[1].PublicKey.ToHex(), wallet2.KeyPairs[1].PublicKey.ToHex());
        Assert.Equal(wallet1.KeyPairs[1].PrivateKey.ToHex(), wallet2.KeyPairs[1].PrivateKey.ToHex());
    }

    [Fact]
    public void Both_addresses_can_be_imported_and_exported_via_keystore()
    {
        Wallet wallet1 = this.Fixture.Value;
        string kstore1 = this._keystore.Value.Export(wallet1, wallet1.KeyPairs[0].PublicKey);
        string kstore2 = this._keystore.Value.Export(wallet1, wallet1.KeyPairs[1].PublicKey);
        Assert.NotNull(kstore1);
        Assert.NotNull(kstore2);

        FixedSaltWalletFactoryProvider factory = new(Constants.DefaultFixedSalt16);
        Wallet wallet2 = factory.Create("rosebud");
        _ = this._keystore.Value.Import(wallet2, kstore1, Constants.KeystoreKeyLength);
        _ = this._keystore.Value.Import(wallet2, kstore2, Constants.KeystoreKeyLength);

        Assert.Equal(wallet1.KeyPairs.Count, wallet2.KeyPairs.Count);
        Assert.Equal(wallet1.KeyPairs[0].PublicKey.ToHex(), wallet2.KeyPairs[0].PublicKey.ToHex());
        Assert.Equal(wallet1.KeyPairs[0].PrivateKey.ToHex(), wallet2.KeyPairs[0].PrivateKey.ToHex());
        Assert.Equal(wallet1.KeyPairs[1].PublicKey.ToHex(), wallet2.KeyPairs[1].PublicKey.ToHex());
        Assert.Equal(wallet1.KeyPairs[1].PrivateKey.ToHex(), wallet2.KeyPairs[1].PrivateKey.ToHex());
    }
}