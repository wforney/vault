// Ignore Spelling: Interop

namespace FluentMigrator.Runtime;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using System;
using System.IO;
using System.Reflection;

public class MigrationService : IMigrationService
{
    [Obsolete]
    public void MigrateToLatest(string databaseType, string connectionString, string profile = null, Assembly assembly = null, bool trace = false) => this.MigrateToVersion(databaseType, connectionString, 0, profile, assembly);

    [Obsolete]
    public void MigrateToVersion(string databaseType, string connectionString, long version, string profile = null, Assembly assembly = null, bool trace = false)
    {
        if (databaseType == "sqlite")
        {
            CopyInteropAssemblyByPlatform();
        }

        assembly ??= Assembly.GetExecutingAssembly();
        IAnnouncer announcer = trace ? new TextWriterAnnouncer(Console.Out) : new NullAnnouncer();
        RunnerContext context = new(announcer)
        {
            Connection = connectionString,
            Database = databaseType,
            Targets = [assembly.FullName],
            Version = version,
            Profile = profile
        };
        TaskExecutor executor = new(context);
        executor.Execute();
    }

    private const string SQLiteAssembly = "SQLite.Interop.dll";
    public static void CopyInteropAssemblyByPlatform()
    {
        string baseDir = WhereAmI();
        string destination = Path.Combine(baseDir, SQLiteAssembly);
        if (File.Exists(destination))
        {
            return;
        }

        string arch = Environment.Is64BitProcess ? "x64" : "x86";
        string path = Path.Combine(arch, SQLiteAssembly);
        string source = Path.Combine(baseDir, path);
        File.Copy(source, destination, true);
    }
    internal static string WhereAmI()
    {
        Uri dir = new(Assembly.GetExecutingAssembly().Location);
        FileInfo fi = new(dir.AbsolutePath);
        return fi.Directory?.FullName;
    }
}