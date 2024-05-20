namespace ChainLib.Tests.Fixtures;

using ChainLib.Models;

public class BlockObjectTypeProviderFixture
{
    public BlockObjectTypeProviderFixture() => this.Value = new BlockObjectTypeProvider();

    public IBlockObjectTypeProvider Value { get; set; }
}