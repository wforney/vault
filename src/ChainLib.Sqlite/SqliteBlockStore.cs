// Ignore Spelling: Sqlite

namespace ChainLib.Sqlite;

using ChainLib.Models;
using ChainLib.Serialization;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Sodium;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class SqliteBlockStore(
    string baseDirectory,
    string subDirectory,
    string databaseName,
    Block genesisBlock,
    IBlockObjectTypeProvider typeProvider,
    ILogger<SqliteBlockStore> logger) : SqliteRepository(baseDirectory, subDirectory, databaseName, logger), IBlockStore
{
    public Task<Block> GetGenesisBlockAsync() => Task.FromResult(genesisBlock);

    public async Task<long> GetLengthAsync()
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT MAX(b.'Index') FROM 'Block' b";

        return (await db.QuerySingleOrDefaultAsync<long?>(sql))
            .GetValueOrDefault(0L);
    }

    public async Task AddAsync(Block block)
    {
        byte[] data = this.SerializeObjects(block);

        using SqliteConnection db = new($"Data Source={this.DataFile}");
        await db.OpenAsync();

        using SqliteTransaction t = db.BeginTransaction();
        long index = await db.QuerySingleAsync<long>(
            "INSERT INTO 'Block' ('Version','PreviousHash','MerkleRootHash','Timestamp','Difficulty','Nonce','Hash','Data') VALUES (@Version,@PreviousHash,@MerkleRootHash,@Timestamp,@Difficulty,@Nonce,@Hash,@Data); " +
            "SELECT LAST_INSERT_ROWID();", new
            {
                block.Version,
                block.PreviousHash,
                block.MerkleRootHash,
                block.Timestamp,
                block.Difficulty,
                block.Nonce,
                block.Hash,
                Data = data
            }, t);

        t.Commit();

        block.Index = index;
    }

    public IEnumerable<BlockObject> StreamAllBlockObjects(bool forwards, long startingFrom = 0)
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT b.* " +
                           "FROM 'Block' b " +
                           "WHERE b.'Index' >= @startingFrom " +
                           "ORDER BY b.'Index' ASC";

        foreach (BlockResult block in db.Query<BlockResult>(sql, new { startingFrom }, buffered: false))
        {
            this.DeserializeObjects(block, block.Data);

            foreach (BlockObject @object in block.Objects)
            {
                yield return @object;
            }
        }
    }

    public IEnumerable<BlockHeader> StreamAllBlockHeaders(bool forwards, long startingFrom = 0)
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT b.'Version', b.'PreviousHash', b.'MerkleRootHash', b.'Timestamp', b.'Difficulty', b.'Nonce'" +
                           "FROM 'Block' b " +
                           "WHERE b.'Index' >= @startingFrom " +
                           "ORDER BY b.'Index' ASC";

        foreach (BlockHeader header in db.Query<BlockHeader>(sql, new { startingFrom }, buffered: false))
        {
            yield return header;
        }
    }

    public IEnumerable<Block> StreamAllBlocks(bool forwards, long startingFrom = 0)
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT b.* " +
                           "FROM 'Block' b " +
                           "WHERE b.'Index' >= @startingFrom " +
                           "ORDER BY b.'Index' ASC";

        foreach (BlockResult block in db.Query<BlockResult>(sql, new { startingFrom }, buffered: false))
        {
            this.DeserializeObjects(block, block.Data);

            yield return block;
        }
    }

    public async Task<Block> GetByIndexAsync(long index)
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Index' = @Index";

        BlockResult block = await db.QuerySingleOrDefaultAsync<BlockResult>(sql, new { Index = index });

        this.DeserializeObjects(block, block.Data);

        return block;
    }

    public async Task<Block> GetByHashAsync(byte[] hash)
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT b.* FROM 'Block' b WHERE b.'Hash' = @Hash";

        BlockResult block = await db.QuerySingleOrDefaultAsync<BlockResult>(sql, new { Hash = hash });

        this.DeserializeObjects(block, block.Data);

        return block;
    }

    public async Task<Block> GetLastBlockAsync()
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT COUNT(1), b.* " +
                           "FROM 'Block' b " +
                           "GROUP BY b.'Index' " +
                           "ORDER BY b.'Index' DESC LIMIT 1";

        BlockResult block = await db.QuerySingleOrDefaultAsync<BlockResult>(sql);

        this.DeserializeObjects(block, block.Data);

        return block;
    }

    public override void MigrateToLatest()
    {
        try
        {
            using SqliteConnection db = new($"Data Source={this.DataFile}");
            _ = db.Execute(@"
CREATE TABLE IF NOT EXISTS 'Block'
(
    'Index' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    'Version' INTEGER NOT NULL,
    'PreviousHash' VARCHAR(64) NOT NULL, 
    'MerkleRootHash' VARCHAR(64) NOT NULL,
    'Timestamp' INTEGER NOT NULL,
    'Difficulty' INTEGER NOT NULL,
    'Nonce' INTEGER NOT NULL,
    'Hash' VARCHAR(64) UNIQUE NOT NULL,
    'Data' BLOB NOT NULL
);");
        }
        catch (SqliteException e)
        {
            logger?.LogError(e, "Error migrating blocks database");
            throw;
        }
    }

    public class BlockResult : Block
    {
        public byte[] Data { get; set; }
    }

    private byte[] SerializeObjects(Block block)
    {
        byte[] data;
        using (MemoryStream ms = new())
        {
            using BinaryWriter bw = new(ms, Encoding.UTF8);
            // Version:
            BlockSerializeContext context = new(bw, typeProvider);

            if (context.typeProvider.SecretKey is not null)
            {
                // Nonce:
                byte[] nonce = StreamEncryption.GenerateNonceChaCha20();
                context.bw.WriteBuffer(nonce);

                // Data:
                using MemoryStream ems = new();
                using BinaryWriter ebw = new(ems, Encoding.UTF8);
                BlockSerializeContext ec = new(ebw, typeProvider, context.Version);
                block.SerializeObjects(ec);
                context.bw.WriteBuffer(StreamEncryption.EncryptChaCha20(ems.ToArray(), nonce, ec.typeProvider.SecretKey));
            }
            else
            {
                // Data:
                context.bw.Write(false);
                block.SerializeObjects(context);
            }

            data = ms.ToArray();
        }

        return data;
    }

    private void DeserializeObjects(Block block, byte[] data)
    {
        using MemoryStream ms = new(data);
        using BinaryReader br = new(ms);
        // Version:
        BlockDeserializeContext context = new(br, typeProvider);

        // Nonce:
        byte[] nonce = context.br.ReadBuffer();
        if (nonce is not null)
        {
            // Data:
            using MemoryStream dms = new(StreamEncryption.EncryptChaCha20(context.br.ReadBuffer(), nonce, typeProvider.SecretKey));
            using BinaryReader dbr = new(dms);
            BlockDeserializeContext dc = new(dbr, typeProvider);
            block.DeserializeObjects(dc);
        }
        else
        {
            // Data:
            block.DeserializeObjects(context);
        }
    }
}
