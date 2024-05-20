namespace ChainLib.Tests;

using ChainLib.Crypto;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Models.Extended;
using ChainLib.Streaming;
using ChainLib.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class WithUnencryptedBlockDatabase : WithBlockDatabase<EmptyBlockRepositoryFixture>,
        IClassFixture<ObjectHashProviderFixture>, IClassFixture<EmptyBlockRepositoryFixture>
{
    public WithUnencryptedBlockDatabase(EmptyBlockRepositoryFixture blockDatabase, ObjectHashProviderFixture hashProvider) : base(blockDatabase, hashProvider)
    {

    }
}

public class WithEncryptedBlockDatabase :
    WithBlockDatabase<EncryptedEmptyBlockRepositoryFixture>,
    IClassFixture<ObjectHashProviderFixture>, IClassFixture<EncryptedEmptyBlockRepositoryFixture>
{
    public WithEncryptedBlockDatabase(
        EncryptedEmptyBlockRepositoryFixture blockDatabase,
        ObjectHashProviderFixture hashProvider) : base(blockDatabase, hashProvider)
    {

    }
}

public abstract class WithBlockDatabase<T>
    where T : EmptyBlockRepositoryFixture
{
    protected WithBlockDatabase(EmptyBlockRepositoryFixture blockDatabase, ObjectHashProviderFixture hashProvider)
    {
        this.Fixture = blockDatabase;
        this.TypeProvider = this.Fixture.TypeProvider;
        this.HashProvider = hashProvider.Value;

        _ = this.TypeProvider.TryAdd(0, typeof(Transaction));
    }

    public IHashProvider HashProvider { get; set; }
    public EmptyBlockRepositoryFixture Fixture { get; set; }
    public IBlockObjectTypeProvider TypeProvider { get; }

    [Fact]
    public async Task Can_stream_typed_objects_and_headers()
    {
        await this.Fixture.Value.AddAsync(this.CreateBlock());

        BlockObjectProjection objectProjection = new(this.Fixture.Value, this.TypeProvider);
        IEnumerable<Transaction> objectStream = objectProjection.Stream<Transaction>();

        Assert.NotNull(objectStream);
        _ = Assert.Single(objectStream);

        IEnumerable<BlockHeader> headerStream = this.Fixture.Value.StreamAllBlockHeaders(true, 2);
        Assert.NotNull(headerStream);
        _ = Assert.Single(headerStream);
    }

    private Block CreateBlock()
    {
        Transaction transaction = new()
        {
            Id = $"{Guid.NewGuid()}"
        };
        BlockObject blockObject = new()
        {
            Data = transaction
        };
        blockObject.Hash = blockObject.ToHashBytes(this.HashProvider);

        Block block = new()
        {
            Nonce = 1,
            PreviousHash = "rosebud".Sha256(),
            MerkleRootHash = this.HashProvider.DoubleHash(blockObject.Hash),
            Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Objects = [blockObject]
        };
        block.MerkleRootHash = block.ComputeMerkleRoot(this.HashProvider);
        block.Hash = block.ToHashBytes(this.HashProvider);
        return block;
    }
}