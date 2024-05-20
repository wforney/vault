namespace ChainLib.Tests;

using ChainLib.Tests.Fixtures;
using System.Collections.Generic;
using Xunit;

public class WhenObjectIsHashed : IClassFixture<ObjectHashProviderFixture>
{
    private readonly ObjectHashProviderFixture _provider;

    public WhenObjectIsHashed(ObjectHashProviderFixture provider) => this._provider = provider;

    [Fact]
    public void Property_order_doesnt_matter()
    {
        var foo = new { Foo = "A", Bar = "B" };
        var bar = new { Bar = "B", Foo = "A" };

        Assert.Equal
        (
            this._provider.Value.ComputeHashString(foo),
            this._provider.Value.ComputeHashString(bar)
        );
    }

    [Fact]
    public void Properties_that_dont_equal_have_different_hashes()
    {
        var foo = new { Foo = "A", Bar = "A" };
        var bar = new { Bar = "B", Foo = "A" };

        Assert.NotEqual
        (
            this._provider.Value.ComputeHashString(foo),
            this._provider.Value.ComputeHashString(bar)
        );
    }

    [Fact]
    public void Properties_with_no_value_dont_matter()
    {
        Stub foo = new() { A = "A", B = "B" };
        Stub bar = new() { B = "B", A = "A", C = null };

        Assert.Equal
        (
            this._provider.Value.ComputeHashString(foo),
            this._provider.Value.ComputeHashString(bar)
        );
    }

    [Fact]
    public void Null_collections_are_equivalent_to_empty_collections()
    {
        Stub foo = new() { D = null };
        Stub bar = new() { D = [] };

        string expected = this._provider.Value.ComputeHashString(foo);
        string actual = this._provider.Value.ComputeHashString(bar);

        Assert.Equal
        (
            expected,
            actual
        );
    }

    private struct Stub
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public List<string> D { get; set; }
    }
}
