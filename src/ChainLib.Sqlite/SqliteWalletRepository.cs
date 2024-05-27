// Ignore Spelling: Sqlite

namespace ChainLib.Sqlite;

using ChainLib.Crypto;
using ChainLib.Wallets;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class SqliteWalletRepository(string baseDirectory, string subDirectory, string databaseName, ILogger<SqliteWalletRepository> logger)
    : SqliteRepository(baseDirectory, subDirectory, databaseName, logger), IWalletRepository
{
    public async Task<IEnumerable<Wallet>> GetAllAsync()
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT w.*, a.* FROM 'Wallet' w " +
                           "LEFT JOIN 'Address' a ON a.'WalletId' = w.'Id' " +
                           "ORDER BY a.'Index' ASC";

        Dictionary<string, Wallet> wallets = [];

        _ = await db.QueryAsync<Wallet, KeyPair, Wallet>(sql, (parent, child) =>
        {
            if (!wallets.TryGetValue(parent.Id, out Wallet value))
            {
                value = parent;
                wallets.Add(parent.Id, value);
            }

            value.KeyPairs.Add(child);
            return value;
        }, splitOn: "WalletId");

        return wallets.Values;
    }

    public async Task<Wallet> GetByIdAsync(string id)
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        const string sql = "SELECT w.*, a.* FROM 'Wallet' w " +
                           "LEFT JOIN 'Address' a ON a.'WalletId' = w.'Id' " +
                           "WHERE w.'Id' = @Id " +
                           "ORDER BY a.'Index' ASC";

        Wallet wallet = null;

        _ = await db.QueryAsync<Wallet, KeyPair, Wallet>(sql, (parent, child) =>
        {
            wallet ??= parent;
            wallet.KeyPairs.Add(child);
            return wallet;
        }, new { Id = id }, splitOn: "WalletId");

        return wallet;
    }

    public async Task<Wallet> AddAsync(Wallet wallet)
    {
        if (wallet.KeyPairs.Count == 0)
        {
            throw new ArgumentException("Wallet contains no key pairs", nameof(wallet));
        }

        using (SqliteConnection db = new($"Data Source={this.DataFile}"))
        {
            await db.OpenAsync();

            using SqliteTransaction t = db.BeginTransaction();
            _ = await db.ExecuteAsync("INSERT INTO Wallet (Id,PasswordHash,Secret) VALUES (@Id,@PasswordHash,@Secret)", wallet, t);

            await SaveAddressesInTransactionAsync(wallet, db, t);

            t.Commit();
        }

        return wallet;
    }

    public async Task SaveAddressesAsync(Wallet wallet)
    {
        using SqliteConnection db = new($"Data Source={this.DataFile}");
        await db.OpenAsync();

        using SqliteTransaction t = db.BeginTransaction();
        await SaveAddressesInTransactionAsync(wallet, db, t);
    }

    private static async Task SaveAddressesInTransactionAsync(Wallet wallet, IDbConnection db, IDbTransaction t)
    {
        _ = await db.ExecuteAsync("DELETE FROM 'Address' WHERE 'WalletId' = @Id", wallet, t);

        foreach (KeyPair keyPair in wallet.KeyPairs)
        {
            _ = await db.ExecuteAsync("INSERT INTO Address ('WalletId','Index','PrivateKey','PublicKey') VALUES (@WalletId,@Index,@PrivateKey,@PublicKey)",
            new
            {
                WalletId = wallet.Id,
                keyPair.Index,
                keyPair.PrivateKey,
                keyPair.PublicKey
            }, t);
        }
    }

    public override void MigrateToLatest()
    {
        try
        {
            using SqliteConnection db = new($"Data Source={this.DataFile}");
            _ = db.Execute(@"
CREATE TABLE IF NOT EXISTS 'Wallet'
(
    'Id' VARCHAR(64) NOT NULL PRIMARY KEY,
    'PasswordHash' VARCHAR(64) NOT NULL,
    'Secret' VARCHAR(1024) NOT NULL
);

CREATE TABLE IF NOT EXISTS 'Address'
(
    'WalletId' VARCHAR(64) NOT NULL,
    'Index' INTEGER NOT NULL, 
    'PrivateKey' VARCHAR(1024) NOT NULL,
    'PublicKey' VARCHAR(64) NOT NULL,

    FOREIGN KEY('WalletId') REFERENCES Wallet('Id')
);");
        }
        catch (SqliteException e)
        {
            logger?.LogError(e, "Error migrating wallets database");
            throw;
        }
    }
}
