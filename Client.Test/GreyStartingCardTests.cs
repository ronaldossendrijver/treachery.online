using Bunit;
using Treachery.Bots;
using Treachery.Client.GameEventComponents;
using Treachery.Client.GenericComponents;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Client.Test;

[TestClass]
[DoNotParallelize]
public sealed class GreyStartingCardTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void IxMaySelectHarkonnenExtraStartingCardWhenHouseRuleIsEnabled(bool ruleEnabled)
    {
        var game = CreateGameAtGreyStartingCardSelection(ruleEnabled);
        var expectedNumberOfChoices = ruleEnabled ? 3 : 2;
        Assert.AreEqual(expectedNumberOfChoices, game.StartingTreacheryCards!.Items.Count);

        using var test = new ComponentTestContext(game, Faction.Grey);
        var component = test.RenderContext.Render<GreySelectedStartingCardComponent>();
        var choices = component.FindComponent<SelectFromImageComponent<TreacheryCard>>().Instance.Values!.ToList();

        Assert.AreEqual(expectedNumberOfChoices, choices.Count);
        var selectedCard = choices[^1];

        new GreySelectedStartingCard(game, Faction.Grey)
        {
            Card = selectedCard
        }.Execute(false, true);

        Assert.Contains(selectedCard, game.GetPlayer(Faction.Grey)!.TreacheryCards);
        Assert.DoesNotContain(selectedCard, game.GetPlayer(Faction.Black)!.TreacheryCards);
        Assert.AreEqual(2, game.GetPlayer(Faction.Black)!.TreacheryCards.Count);
        Assert.AreEqual(0, game.StartingTreacheryCards.Items.Count);
    }

    private static Game CreateGameAtGreyStartingCardSelection(bool ruleEnabled)
    {
        var game = new Game();
        var rules = new List<Rule>
        {
            Rule.BasicTreacheryCards,
            Rule.FillWithBots
        };
        if (ruleEnabled)
            rules.Add(Rule.GreyMaySelectBlackExtraStartingCard);

        var establishPlayers = new EstablishPlayers(game, Faction.None)
        {
            Seed = 85,
            Settings = new GameSettings
            {
                InitialRules = rules,
                AllowedFactionsInPlay = [Faction.Black, Faction.Grey],
                MaximumTurns = 10,
                NumberOfPlayers = 2
            }
        };

        Assert.IsNull(establishPlayers.Execute(true, true));

        var bots = game.Players.ToDictionary(
            player => player.Faction,
            player => new ClassicBot(game, player, BotParameters.GetDefaultParameters(player.Faction)));

        for (var eventNumber = 0; game.CurrentPhase != Phase.GreySelectingCard && eventNumber < 100; eventNumber++)
        {
            var gameEvent = DetermineBotEvent(game, bots)
                ?? throw new InvalidOperationException($"No bot could act in phase {game.CurrentPhase}.");
            Assert.IsNull(gameEvent.Execute(true, true));
        }

        Assert.AreEqual(Phase.GreySelectingCard, game.CurrentPhase);
        return game;
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
                    return gameEvent;
            }
        }

        return null;
    }
}
