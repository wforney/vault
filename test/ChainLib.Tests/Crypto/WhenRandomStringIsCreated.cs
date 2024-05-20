namespace ChainLib.Tests.Crypto;

using ChainLib.Tests.Crypto.Fixtures;
using Xunit;

public class WhenRandomStringIsCreated : IClassFixture<RandomStringFixture>
{
    private readonly RandomStringFixture _fixture;

    public WhenRandomStringIsCreated(RandomStringFixture fixture) => this._fixture = fixture;

    [Fact]
    public void It_is_the_correct_length() => Assert.Equal(64, this._fixture.Value.Length);
}
