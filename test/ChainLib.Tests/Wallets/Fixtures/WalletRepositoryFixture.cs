namespace ChainLib.Tests.Wallets.Fixtures;

using ChainLib.Sqlite;
using ChainLib.Wallets.Addresses;
using ChainLib.Wallets.Secrets;
using Microsoft.Extensions.Logging;
using System;

public class WalletRepositoryFixture
{
    public WalletRepositoryFixture()
    {
        PasswordHashSecretProvider secrets = new();
        _ = new DeterministicWalletAddressProvider(secrets);

        LoggerFactory factory = new();

        string baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        this.Value = new SqliteWalletRepository(
                baseDirectory,
                $"{Guid.NewGuid()}",
                "wallets",
                factory.CreateLogger<SqliteWalletRepository>());
    }

    public SqliteWalletRepository Value { get; set; }
}