namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Wallets;
using ChainLib.Wallets.StorageFormats;

public class KeyStoreStorageFormatFixture
{
    public KeyStoreStorageFormatFixture() => this.Value = new KeystoreFileStorageFormat();

    public IWalletAddressStorageFormat Value { get; set; }
}