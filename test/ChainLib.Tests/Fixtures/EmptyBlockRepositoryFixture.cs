namespace ChainLib.Tests.Fixtures;

using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Sqlite;
using Microsoft.Extensions.Logging;
using System;

public class EmptyBlockRepositoryFixture : IDisposable
{
    public EmptyBlockRepositoryFixture() : this(
        $"{Guid.NewGuid()}",
        new ObjectHashProviderFixture().Value,
        new BlockObjectTypeProviderFixture().Value)
    { }

    protected EmptyBlockRepositoryFixture(string subDirectory, IHashProvider hashProvider, IBlockObjectTypeProvider typeProvider)
    {
        LoggerFactory factory = new();

        Block genesisBlock = new()
        {
            Index = 0L,
            PreviousHash = new byte[] { 0 },
            MerkleRootHash = new byte[] { 0 },
            Timestamp = 1465154705U,
            Nonce = 0L,
            Objects = new BlockObject[] { },
        };

        string baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        this.Value = new SqliteBlockStore(
                baseDirectory,
                subDirectory,
                "blockchain",
                genesisBlock,
                typeProvider,
                factory.CreateLogger<SqliteBlockStore>());

        genesisBlock.Index = 1;

        genesisBlock.Hash = genesisBlock.ToHashBytes(hashProvider);

        this.Value.AddAsync(genesisBlock).ConfigureAwait(false).GetAwaiter().GetResult();

        this.GenesisBlock = genesisBlock;

        this.TypeProvider = typeProvider;
    }

    public SqliteBlockStore Value { get; set; }

    public Block GenesisBlock { get; }

    public IBlockObjectTypeProvider TypeProvider { get; }

    public void Dispose() => this.Value.Purge();
}