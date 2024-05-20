namespace ChainLib.Tests.Crypto.Fixtures;

using ChainLib.Crypto;

public class RandomStringFixture
{
    public RandomStringFixture() => this.Value = CryptoUtil.RandomString();

    public string Value { get; set; }
}