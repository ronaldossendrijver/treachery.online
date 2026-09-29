using System.Data;
using System.Data.Common;

namespace Treachery.Server;

public static class GameStorageMigration
{
    public static int Migrate(TreacheryContext context)
    {
        context.Database.Migrate();
        return CompressLegacyGames(context);
    }

    private static int CompressLegacyGames(TreacheryContext context)
    {
        var connection = context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed)
            connection.Open();

        try
        {
            return CompressTable(connection, "PersistedGames") + CompressTable(connection, "ArchivedGames");
        }
        finally
        {
            if (wasClosed)
                connection.Close();
        }
    }

    private static int CompressTable(DbConnection connection, string table)
    {
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
                    return count;

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
