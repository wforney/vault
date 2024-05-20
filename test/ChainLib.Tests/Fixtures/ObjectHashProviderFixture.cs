namespace ChainLib.Tests.Fixtures;

using ChainLib.Models;

public class ObjectHashProviderFixture
{
    public ObjectHashProviderFixture() => this.Value = new ObjectHashProvider();

    public IHashProvider Value { get; set; }
}