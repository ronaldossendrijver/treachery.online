using System;

namespace Treachery.Shared;

public class UserConnections
{
    private Dictionary<string, ConnectionInfo> ConnectionInfoByGameId { get; } = [];

    public void SetConnectionId(string gameId, string connectionId)
    {
        ConnectionInfoByGameId[gameId] = new ConnectionInfo { ConnectionId = connectionId };
    }

    public IEnumerable<string> GetGameIdsWithOldConnections(int ageInDays)
    {
        return ConnectionInfoByGameId.Where(idAndInfo =>
            DateTimeOffset.Now
                .Subtract(idAndInfo.Value.Created).TotalDays > ageInDays)
            .Select(idAndInfo => idAndInfo.Key);
    }

    public bool TryGetConnectionId(string gameId, out string connectionId)
    {
        connectionId = string.Empty;

        if (ConnectionInfoByGameId.TryGetValue(gameId, out var connectionInfo))
        {
            connectionId = connectionInfo.ConnectionId;
            return true;
        }

        return false;
    }

    public void RemoveConnectionId(string gameId)
    {
        ConnectionInfoByGameId.Remove(gameId);
    }
}

public class ConnectionInfo
{
    public DateTimeOffset Created { get; } = DateTimeOffset.Now;

    public string ConnectionId { get; init; }
}

