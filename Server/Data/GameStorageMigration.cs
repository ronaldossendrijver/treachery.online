using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Treachery.Server;

public static class GameStorageMigration
{
    private const int ProgressInterval = 100;

    public static int Migrate(TreacheryContext context, ILogger? logger = null)
    {
        logger ??= Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        LogDataSource(context, logger);

        logger.LogInformation("Applying database migrations...");
        var stopwatch = Stopwatch.StartNew();
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied in {Elapsed}.", stopwatch.Elapsed);

        stopwatch.Restart();
        logger.LogInformation("Compressing legacy game JSON...");
        var count = CompressLegacyGames(context, logger);
        logger.LogInformation("Legacy game JSON compression finished: {RowCount} rows compressed in {Elapsed}.", count, stopwatch.Elapsed);
        return count;
    }

    private static void LogDataSource(TreacheryContext context, ILogger logger)
    {
        try
        {
            var builder = new SqliteConnectionStringBuilder(context.Database.GetConnectionString());
            var dataSource = builder.DataSource;
            var fullPath = string.IsNullOrEmpty(dataSource) || dataSource == ":memory:" ? dataSource : Path.GetFullPath(dataSource);
            var exists = !string.IsNullOrEmpty(fullPath) && File.Exists(fullPath);
            var size = exists ? new FileInfo(fullPath).Length : 0;
            logger.LogInformation("Using SQLite data source '{DataSource}' (full path: '{FullPath}', exists: {Exists}, size: {Size} bytes, working directory: '{WorkingDirectory}').",
                dataSource, fullPath, exists, size, Environment.CurrentDirectory);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to determine SQLite data source.");
        }
    }

    private static int CompressLegacyGames(TreacheryContext context, ILogger logger)
    {
        var connection = context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed)
            connection.Open();

        try
        {
            return CompressTable(connection, "PersistedGames", logger) + CompressTable(connection, "ArchivedGames", logger);
        }
        finally
        {
            if (wasClosed)
                connection.Close();
        }
    }

    private static int CompressTable(DbConnection connection, string table, ILogger logger)
    {
        long remaining;
        using (var countCommand = connection.CreateCommand())
        {
            countCommand.CommandText = $"""
                SELECT COUNT(*)
                FROM "{table}"
                WHERE typeof("GameState") = 'text' OR typeof("GameParticipation") = 'text'
                """;
            remaining = Convert.ToInt64(countCommand.ExecuteScalar());
        }

        logger.LogInformation("Compressing table {Table}: {Remaining} rows need compression.", table, remaining);
        var stopwatch = Stopwatch.StartNew();
        var count = 0;
        var lastId = long.MinValue;
        while (true)
        {
            // Each row commits independently so an interrupted startup can resume without
            // loading all histories into memory or repeating already compressed rows.
            using var transaction = connection.BeginTransaction();
            using var select = connection.CreateCommand();
            select.Transaction = transaction;
            select.CommandText = $"""
                SELECT "Id", "GameState", "GameParticipation"
                FROM "{table}"
                WHERE "Id" > @lastId
                  AND (typeof("GameState") = 'text' OR typeof("GameParticipation") = 'text')
                ORDER BY "Id"
                LIMIT 1
                """;
            AddParameter(select, "@lastId", lastId);

            byte[] state;
            byte[] participation;
            using (var reader = select.ExecuteReader())
            {
                if (!reader.Read())
                {
                    logger.LogInformation("Finished compressing table {Table}: {RowCount} rows compressed in {Elapsed}.", table, count, stopwatch.Elapsed);
                    return count;
                }

                lastId = reader.GetInt64(0);
                state = CompressIfText(reader.GetValue(1));
                participation = CompressIfText(reader.GetValue(2));
            }

            using var update = connection.CreateCommand();
            update.Transaction = transaction;
            update.CommandText = $"""
                UPDATE "{table}"
                SET "GameState" = @state, "GameParticipation" = @participation
                WHERE "Id" = @id
                """;
            AddParameter(update, "@state", state);
            AddParameter(update, "@participation", participation);
            AddParameter(update, "@id", lastId);
            if (update.ExecuteNonQuery() != 1)
                throw new InvalidOperationException($"Unable to compress {table} row {lastId}.");

            transaction.Commit();
            count++;

            if (count % ProgressInterval == 0)
                logger.LogInformation("Compressing table {Table}: {RowCount}/{Remaining} rows done, elapsed {Elapsed}.", table, count, remaining, stopwatch.Elapsed);
        }
    }

    private static byte[] CompressIfText(object value) => value switch
    {
        string json => CompressedJson.Compress(json),
        byte[] compressed => compressed,
        _ => throw new InvalidOperationException("Expected game JSON stored as TEXT or BLOB.")
    };

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
