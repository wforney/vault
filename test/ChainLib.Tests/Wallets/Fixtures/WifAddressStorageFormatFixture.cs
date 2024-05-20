namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Wallets;
using ChainLib.Wallets.StorageFormats;

public class WifAddressStorageFormatFixture
{
    public WifAddressStorageFormatFixture() => this.Value = new WifAddressStorageFormat();

    public IWalletAddressStorageFormat Value { get; set; }
}