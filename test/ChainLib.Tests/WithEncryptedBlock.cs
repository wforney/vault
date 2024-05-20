namespace ChainLib.Tests;

using ChainLib.Crypto;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Models.Extended;
using ChainLib.Tests.Fixtures;
using System;
using Xunit;

public class WithEncryptedBlock : IClassFixture<ObjectHashProviderFixture>, IClassFixture<EncryptedBlockObjectTypeProviderFixture>
{
    private readonly ObjectHashProviderFixture _hash;
    private readonly EncryptedBlockObjectTypeProviderFixture _types;

    public WithEncryptedBlock(ObjectHashProviderFixture hash, EncryptedBlockObjectTypeProviderFixture types)
    {
        this._hash = hash;
        this._types = types;
    }

    [Fact]
    public void Can_round_trip_with_no_objects()
    {
        Block block = new()
        {
            Nonce = 1,
            PreviousHash = "rosebud".Sha256(),
            Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
        block.MerkleRootHash = block.ComputeMerkleRoot(this._hash.Value);
        block.Hash = block.ToHashBytes(this._hash.Value);

        block.RoundTripCheck(this._hash.Value, this._types.Value);
    }

    [Fact]
    public void Can_round_trip_with_transaction_objects()
    {
        _ = this._types.Value.TryAdd(0, typeof(Transaction));

        Transaction transaction = new()
        {
            Id = $"{Guid.NewGuid()}"
        };
        BlockObject blockObject = new()
        {
            Data = transaction
        };
        blockObject.Hash = blockObject.ToHashBytes(this._hash.Value);

        Block block = new()
        {
            Nonce = 1,
            PreviousHash = "rosebud".Sha256(),
            Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Objects = [blockObject]
        };
        block.MerkleRootHash = block.ComputeMerkleRoot(this._hash.Value);
        block.Hash = block.ToHashBytes(this._hash.Value);

        block.RoundTripCheck(this._hash.Value, this._types.Value);
    }
}