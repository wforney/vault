namespace ChainLib.Tests;

using ChainLib.Models;
using ChainLib.Models.Extended;
using ChainLib.Tests.Fixtures;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using Xunit;

public class WhenBlockDatabaseContainsOnlyGenesis :
        IClassFixture<EmptyBlockRepositoryFixture>,
        IClassFixture<ObjectHashProviderFixture>
{
    public WhenBlockDatabaseContainsOnlyGenesis(EmptyBlockRepositoryFixture blockDatabase)
    {
        this.Fixture = blockDatabase;
        this.TypeProvider = this.Fixture.TypeProvider;
        _ = this.TypeProvider.TryAdd(0, typeof(Transaction));
    }

    public EmptyBlockRepositoryFixture Fixture { get; set; }
    public IBlockObjectTypeProvider TypeProvider { get; }

    [Fact]
    public void There_are_no_migration_errors() { }

    [Fact]
    public async Task Cannot_add_unhashed_block()
    {
        _ = await Assert.ThrowsAsync<SqliteException>(async () =>
        {
            Block block = new();
            await this.Fixture.Value.AddAsync(block);
        });
    }

    [Fact]
    public async Task Cannot_add_non_unique_block()
    {
        _ = await Assert.ThrowsAsync<SqliteException>(async () =>
        {
            Block block = this.Fixture.GenesisBlock;
            await this.Fixture.Value.AddAsync(block);
        });
    }

    [Fact]
    public async Task Can_retrieve_genesis_block_by_index()
    {
        Block retrieved = await this.Fixture.Value.GetByIndexAsync(1);
        Assert.NotNull(retrieved);
        Assert.Equal(retrieved.Hash, this.Fixture.GenesisBlock.Hash);
        Assert.Equal(retrieved.PreviousHash, this.Fixture.GenesisBlock.PreviousHash);
        Assert.Equal(retrieved.MerkleRootHash, this.Fixture.GenesisBlock.MerkleRootHash);
    }

    [Fact]
    public void Can_retrieve_genesis_block_by_hash()
    {
        Block block = this.Fixture.GenesisBlock;
        Task<Block> retrieved = this.Fixture.Value.GetByHashAsync(block.Hash);
        Assert.NotNull(retrieved);
    }

    [Fact]
    public async Task Length_is_one()
    {
        long retrieved = await this.Fixture.Value.GetLengthAsync();
        Assert.Equal(1, retrieved);
    }

    [Fact]
    public async Task Last_block_is_genesis_block()
    {
        Block genesis = this.Fixture.GenesisBlock;
        Block retrieved = await this.Fixture.Value.GetLastBlockAsync();
        Assert.Equal(genesis.Timestamp, retrieved.Timestamp);
    }
}