using Bunit;
using NSubstitute;
using Treachery.Client.GameEventComponents;
using Treachery.Bots;
using Treachery.Client.OtherComponents;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Client.Test;

[TestClass]
[DoNotParallelize]
public sealed class BotGeneratedComponentSmokeTests
{
    [TestMethod]
    public void StormComponentSubmitsValidEventFromGeneratedCurrentState()
    {
        var (game, player) = FindScenario<StormDialled>();
        using var test = new ComponentTestContext(game, player.Faction);
        StormDialled? submitted = null;
        test.Client.RequestGameEvent(Arg.Do<StormDialled>(value => submitted = value))
            .Returns(Task.FromResult<string?>(null));

        var component = test.RenderContext.Render<StormDialledComponent>();
        var confirm = component.FindAll("button").Single();
        Assert.IsFalse(confirm.HasAttribute("disabled"), confirm.ParentElement?.GetAttribute("title"));

        confirm.Click();

        Assert.IsNotNull(submitted);
        Assert.AreEqual(player.Faction, submitted.Initiator);
        Assert.IsNull(submitted.Validate());
    }

    [TestMethod]
    [Timeout(30_000, CooperativeCancellation = true)]
    public void CurrentVersionBotGameRendersApplicableActionPanels()
    {
        var game = CreateGame();
        var bots = game.Players.ToDictionary(
            player => player.Faction,
            player => new ClassicBot(game, player, BotParameters.GetDefaultParameters(player.Faction)));
        var renderedActionTypes = new HashSet<Type>();

        for (var eventNumber = 0; game.CurrentPhase != Phase.GameEnded && eventNumber < 1_000; eventNumber++)
        {
            foreach (var player in game.Players)
            {
                var actions = game.GetApplicableEvents(player, true);
                if (actions.Count == 0)
                {
                    continue;
                }

                using var test = new ComponentTestContext(game, player.Faction);
                test.Client.Actions.Returns(actions);
                test.Client.IsHost.Returns(true);
                test.RenderContext.Render<ActionPanel>();
                renderedActionTypes.UnionWith(actions);
            }

            var gameEvent = DetermineBotEvent(game, bots);
            Assert.IsNotNull(gameEvent, $"No bot could act in phase {game.CurrentPhase}.");
            gameEvent.Time = DateTimeOffset.UnixEpoch.AddSeconds(eventNumber);
            Assert.IsNull(
                gameEvent.Execute(true, true),
                $"Bot produced an invalid {gameEvent.GetType().Name} in phase {game.CurrentPhase}.");
        }

        Assert.AreEqual(Phase.GameEnded, game.CurrentPhase, "The generated game did not finish within 1,000 events.");
        Assert.IsGreaterThanOrEqualTo(
            15,
            renderedActionTypes.Count,
            $"Expected broad component coverage, but rendered only: {string.Join(", ", renderedActionTypes.Select(type => type.Name).Order())}");
    }

    private static Game CreateGame()
    {
        var game = new Game();
        var rules = Game.RulesetDefinition[Ruleset.AllExpansionsAdvancedGame].ToList();
        rules.Add(Rule.FillWithBots);

        var establishPlayers = new EstablishPlayers(game, Faction.None)
        {
            Seed = 1729,
            Settings = new GameSettings
            {
                InitialRules = rules,
                AllowedFactionsInPlay = EstablishPlayers.AvailableFactions().ToList(),
                MaximumTurns = 2,
                NumberOfPlayers = 6
            }
        };

        Assert.IsNull(establishPlayers.Execute(true, true));
        return game;
    }

    private static (Game Game, Player Player) FindScenario<TEvent>() where TEvent : GameEvent
    {
        var game = CreateGame();
        var bots = game.Players.ToDictionary(
            player => player.Faction,
            player => new ClassicBot(game, player, BotParameters.GetDefaultParameters(player.Faction)));

        for (var eventNumber = 0; game.CurrentPhase != Phase.GameEnded && eventNumber < 1_000; eventNumber++)
        {
            var player = game.Players.FirstOrDefault(candidate =>
                game.GetApplicableEvents(candidate, true).Contains(typeof(TEvent)));
            if (player != null)
            {
                return (game, player);
            }

            var gameEvent = DetermineBotEvent(game, bots)
                ?? throw new InvalidOperationException($"No bot could act in phase {game.CurrentPhase}.");
            gameEvent.Time = DateTimeOffset.UnixEpoch.AddSeconds(eventNumber);
            var validation = gameEvent.Execute(true, true);
            if (validation != null)
            {
                throw new InvalidOperationException(
                    $"Bot produced an invalid {gameEvent.GetType().Name}: {validation.ToString(DefaultSkin.Default)}");
            }
        }

        throw new InvalidOperationException($"No current-version scenario produced {typeof(TEvent).Name}.");
    }

    private static GameEvent? DetermineBotEvent(Game game, IReadOnlyDictionary<Faction, ClassicBot> bots)
    {
        foreach (var priority in new Func<ClassicBot, List<Type>, GameEvent?>[]
                 {
                     (bot, actions) => bot.DetermineHighestPriorityInPhaseAction(actions),
                     (bot, actions) => bot.DetermineHighPriorityInPhaseAction(actions),
                     (bot, actions) => bot.DetermineMiddlePriorityInPhaseAction(actions),
                     (bot, actions) => bot.DetermineLowPriorityInPhaseAction(actions),
                     (bot, actions) => bot.DetermineEndPhaseAction(actions)
                 })
        {
            foreach (var player in game.Players)
            {
                var gameEvent = priority(bots[player.Faction], game.GetApplicableEvents(player, true));
                if (gameEvent != null)
                {
                    return gameEvent;
                }
            }
        }

        return null;
    }
}
