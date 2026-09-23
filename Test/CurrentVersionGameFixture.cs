using Treachery.Shared;

namespace Treachery.Test;

internal static class CurrentVersionGameFixture
{
    public static Game ReplayUntil(string stateData, int eventIndex)
    {
        var state = GameState.Load(stateData);
        if (state.Version != Game.LatestVersion)
        {
            throw new ArgumentException(
                $"Component fixtures must use game version {Game.LatestVersion}; version {state.Version} is not supported.",
                nameof(stateData));
        }

        var events = state.Events.ToArray();
        if (eventIndex < 0 || eventIndex > events.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(eventIndex));
        }

        var game = new Game();
        foreach (var gameEvent in events.Take(eventIndex))
        {
            gameEvent.Initialize(game);
            var validation = gameEvent.Execute(false, true);
            if (validation != null)
            {
                throw new InvalidOperationException(
                    $"Could not replay {gameEvent.GetType().Name}: {validation.ToString(DefaultSkin.Default)}");
            }
        }

        return game;
    }

    public static Game Create(int version)
    {
        if (version is < Game.LatestVersion - 3 or > Game.LatestVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(version));
        }

        return new Game(version, new Participation());
    }
}
