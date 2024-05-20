namespace ChainLib.Console;

using ChainLib.Configuration;
using ChainLib.Crypto;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Services;
using ChainLib.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

internal class Program
{
    private static Blockchain _blockchain;

    private static void Main(string[] args)
    {
        if (!Bootstrap() && Environment.UserInteractive)
        {
            _ = System.Console.ReadKey();
        }

        Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false).GetAwaiter().GetResult();

        System.Console.ForegroundColor = ConsoleColor.Gray;
        System.Console.BackgroundColor = ConsoleColor.Black;

        while (true)
        {
            string consoleInput = In();
            if (string.IsNullOrWhiteSpace(consoleInput))
            {
                continue;
            }

            try
            {
                TryExecute(consoleInput);
            }
            catch (Exception ex)
            {
                Out(ex.Message);
            }
        }
    }

    private static void TryExecute(string command)
    {

    }

    public static void Out(string message)
    {
        if (message.Length <= 0)
        {
            return;
        }

        System.Console.WriteLine(message);
    }

    private const string ReadPrompt = "console> ";
    public static string In(string prompt = "")
    {
        System.Console.Write(ReadPrompt + prompt);
        return System.Console.ReadLine();
    }

    private static void UnhandledException(ILogger logger, Exception exception) => logger.LogCritical(exception, $"Unexpected error");

    private static bool Bootstrap()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("chain.json")
            .Build();

        LoggerFactory loggerFactory = new();

        ChainSettings chain = new();
        config.Bind(chain);

        System.Console.Title = chain.Name;

        ILogger logger = loggerFactory.CreateLogger(chain.Name);

        TaskScheduler.UnobservedTaskException += (s, e) => UnhandledException(logger, e.Exception);

        AppDomain.CurrentDomain.UnhandledException += (s, e) => UnhandledException(logger, e.ExceptionObject as Exception);

        try
        {
            ObjectHashProvider hashProvider = new();
            BlockObjectTypeProvider typeProvider = new();

            if (chain.GenesisBlock == null)
            {
                logger.LogCritical("Chain genesis block is missing");
                goto failfast;
            }

            if (chain.GenesisBlock.Hash == null)
            {
                logger.LogCritical("Chain genesis block hash is missing");
                goto failfast;
            }

            byte[] genesisHash = chain.GenesisBlock.ToHashBytes(hashProvider);

#if DEBUG
            string genesisHashDebug = "[";
            foreach (byte c in genesisHash)
            {
                genesisHashDebug += c + ", ";
            }

            genesisHashDebug += "]";
            logger.LogDebug($"Expected genesisHash = {genesisHashDebug}");
#endif

            if (!chain.GenesisBlock.Hash.ConstantTimeEquals(genesisHash))
            {
                logger.LogCritical($"Chain genesis block hash '{genesisHash.ToHex()}' is invalid");
                goto failfast;
            }

            byte[] chainHash = hashProvider.ComputeHashBytes(chain);

#if DEBUG
            string chainHashDebug = "[";
            foreach (byte c in chainHash)
            {
                chainHashDebug += c + ", ";
            }

            chainHashDebug += "]";
            logger.LogDebug($"Expected chainHash = {chainHashDebug}");
#endif

            if (!chain.Hash.ConstantTimeEquals(chainHash))
            {
                logger.LogCritical("Chain configuration hash is missing or invalid");
                goto failfast;
            }

            //
            // Storage:
            //
            IBlockStore blocks = null;
            if (!string.IsNullOrWhiteSpace(chain.StorageEngine))
            {
                switch (chain.StorageEngine?.ToUpperInvariant())
                {
                    case "SQLITE":
                        {
                            string baseDirectory =
                                chain.StorageDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                            blocks = new SqliteBlockStore(baseDirectory, chain.Name, "blocks", chain.GenesisBlock,
                                typeProvider, loggerFactory.CreateLogger<SqliteBlockStore>());
                            break;
                        }
                }
            }

            //
            // Blockchain:
            //
            if (blocks == null)
            {
                logger.LogCritical("Could not find a supported storage engine for the chain.");
                goto failfast;
            }

            _blockchain = new Blockchain(blocks, new NoProofOfWork(), hashProvider,
                loggerFactory.CreateLogger<Blockchain>());
        }
        catch (Exception ex)
        {
            UnhandledException(logger, ex);
        }

        logger.LogInformation($"{chain.Name} is running.");
        return true;

    failfast:
        logger.LogInformation("Press any key to shut down.");
        return false;
    }
}
