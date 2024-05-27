namespace FluentMigrator.Runtime;

using System;
using System.Reflection;

public static class Migrations
{
    static Migrations() => Migrator = CreateMigratorMethod();

    private static readonly Lazy<IMigrationService> Migrator;
    private static Lazy<IMigrationService> CreateMigratorMethod() => new(() => new MigrationService());

    public static void MigrateToLatest(string databaseType, string connectionString, string profile = null, Assembly assembly = null, bool trace = false) =>
        Migrator.Value.MigrateToLatest(databaseType, connectionString, profile, assembly, trace);

    public static void MigrateToVersion(string databaseType, string connectionString, long version, string profile = null, Assembly assembly = null, bool trace = false) =>
        Migrator.Value.MigrateToVersion(databaseType, connectionString, version, profile, assembly, trace);
}
