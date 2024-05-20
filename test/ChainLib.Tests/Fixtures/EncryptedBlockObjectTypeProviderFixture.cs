namespace ChainLib.Tests.Fixtures;

using ChainLib.Crypto;
using ChainLib.Models;

public class EncryptedBlockObjectTypeProviderFixture
{
    public EncryptedBlockObjectTypeProviderFixture() => this.Value = new BlockObjectTypeProvider(CryptoUtil.RandomBytes(32));

    public IBlockObjectTypeProvider Value { get; set; }
}