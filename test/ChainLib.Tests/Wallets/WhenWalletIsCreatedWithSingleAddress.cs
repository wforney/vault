namespace ChainLib.Tests.Wallets;

using ChainLib.Crypto;
using ChainLib.Tests.Wallets.Fixtures;
using ChainLib.Wallets;
using ChainLib.Wallets.Factories;
using ChainLib.Wallets.StorageFormats;
using System.IO;
using Xunit;

public class WhenWalletIsCreatedWithSingleAddress :
        IClassFixture<WalletWithOneAddressFixture>,
        IClassFixture<KeyStoreStorageFormatFixture>
{
    private readonly KeyStoreStorageFormatFixture _keystore;

    public WhenWalletIsCreatedWithSingleAddress(
        WalletWithOneAddressFixture fixture,
        KeyStoreStorageFormatFixture keystore)
    {
        this._keystore = keystore;
        this.Fixture = fixture;
    }

    public WalletWithOneAddressFixture Fixture { get; set; }

    [Fact]
    public void There_is_one_keypair_in_the_wallet()
    {
        Assert.NotNull(this.Fixture.Value);
        _ = Assert.Single(this.Fixture.Value.KeyPairs);
    }

    [Fact]
    public void The_wallet_can_be_exported_to_keystore_on_disk()
    {
        Wallet wallet1 = this.Fixture.Value;
        string filename = KeystoreFileStorageFormat.WriteToFile(Path.GetTempPath(), wallet1, KeystoreFileStorageFormat.KdfType.Scrypt);
        string keystore = File.ReadAllText(filename);

        FixedSaltWalletFactoryProvider factory = new(Constants.DefaultFixedSalt16);
        Wallet wallet2 = factory.Create("rosebud");
        _ = this._keystore.Value.Import(wallet2, keystore, Constants.KeystoreKeyLength);

        Assert.Equal(wallet1.KeyPairs.Count, wallet2.KeyPairs.Count);
        Assert.Equal(wallet1.KeyPairs[0].PublicKey.ToHex(), wallet2.KeyPairs[0].PublicKey.ToHex());
        Assert.Equal(wallet1.KeyPairs[0].PrivateKey.ToHex(), wallet2.KeyPairs[0].PrivateKey.ToHex());
    }
}