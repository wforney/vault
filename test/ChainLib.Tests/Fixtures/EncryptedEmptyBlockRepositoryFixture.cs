namespace ChainLib.Tests.Fixtures;

using System;

public class EncryptedEmptyBlockRepositoryFixture : EmptyBlockRepositoryFixture
{
    public EncryptedEmptyBlockRepositoryFixture() : base(
        $"{Guid.NewGuid()}",
        new ObjectHashProviderFixture().Value,
        new EncryptedBlockObjectTypeProviderFixture().Value)
    { }
}