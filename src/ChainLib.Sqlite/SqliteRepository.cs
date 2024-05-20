namespace ChainLib.Sqlite;

using Microsoft.Extensions.Logging;
using System.IO;

public abstract class SqliteRepository
{
    private readonly string _baseDirectory;
    private readonly ILogger _logger;

    private const string DataSubFolder = "Data";

    protected SqliteRepository(string baseDirectory, string subDirectory, string databaseName, ILogger logger)
    {
        this._baseDirectory = baseDirectory;
        this._logger = logger;
        this.CreateIfNotExists(subDirectory, databaseName);
    }

    protected void CreateIfNotExists(string @namespace, string name)
    {
        string dataDirectory = Path.Combine(this._baseDirectory, DataSubFolder);

        if (!Directory.Exists(dataDirectory))
        {
            _ = Directory.CreateDirectory(dataDirectory);
        }

        if (!Directory.Exists(Path.Combine(dataDirectory, @namespace)))
        {
            _ = Directory.CreateDirectory(Path.Combine(dataDirectory, @namespace));
        }

        this.DataFile = Path.Combine(dataDirectory, @namespace, $"{name}.db3");

        if (File.Exists(this.DataFile))
        {
            return;
        }

        this._logger?.LogInformation($"Creating and migrating database at '{this.DataFile}'");
        this.MigrateToLatest();
    }

    protected string DataFile { get; private set; }

    public abstract void MigrateToLatest();

    public void Purge()
    {
        this._logger?.LogInformation($"Deleting database at '{this.DataFile}'");
        File.Delete(this.DataFile);

        string directoryName = Path.GetDirectoryName(this.DataFile);
        if (Directory.GetFiles(directoryName, "*.*", SearchOption.AllDirectories).Length > 0)
        {
            this._logger?.LogInformation($"Deleting database directory '{directoryName}' as it is no longer in use");
            Directory.Delete(directoryName, true);
        }
    }
}