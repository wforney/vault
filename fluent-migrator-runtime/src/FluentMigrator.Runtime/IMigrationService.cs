namespace FluentMigrator.Runtime;

using System.Reflection;

public interface IMigrationService
{
    void MigrateToLatest(string databaseType, string connectionString, string profile = null, Assembly assembly = null, bool trace = false);
    void MigrateToVersion(string databaseType, string connectionString, long version, string profile = null, Assembly assembly = null, bool trace = false);
}
