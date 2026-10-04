namespace Treachery.Server;

public sealed class GameInfoSummaryCache
{
    private readonly ConcurrentDictionary<string, GameInfo> _summaries = new();

    public bool TryGet(string gameId, [NotNullWhen(true)] out GameInfo? summary) =>
        _summaries.TryGetValue(gameId, out summary);

    public void Set(string gameId, GameInfo summary) => _summaries[gameId] = summary;

    public void Remove(string gameId) => _summaries.TryRemove(gameId, out _);
}
