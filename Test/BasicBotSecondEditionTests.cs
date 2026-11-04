using System.Reflection;

namespace Treachery.Test;

[TestClass]
public sealed class BasicBotSecondEditionTests
{
    [TestMethod]
    public void BasicBotUsesKaramaAgainstPrescienceThatRevealsItsPlan()
    {
        var (game, player) = CreateResponseGame(
            FactionAdvantage.GreenBattlePlanPrescience,
            Faction.Green,
            Faction.Red,
            Faction.Red);
        var bot = new BasicBot(game, player, BotParameters.GetDefaultParameters(player.Faction));

        var action = bot.DetermineLowPriorityInPhaseAction(game.GetApplicableEvents(player, true));

        var karama = Assert.IsInstanceOfType<Karma>(action);
        Assert.AreSame(player.TreacheryCards.Single(), karama.Card);
        Assert.AreEqual(FactionAdvantage.GreenBattlePlanPrescience, karama.Prevented);
        Assert.IsNull(karama.Validate());
    }

    [TestMethod]
    public void BasicBotUsesKaramaAgainstAnOpposingBattleVoice()
    {
        var (game, player) = CreateResponseGame(
            FactionAdvantage.BlueUsingVoice,
            Faction.Blue,
            Faction.Green,
            Faction.Green);
        var bot = new BasicBot(game, player, BotParameters.GetDefaultParameters(player.Faction));

        var action = bot.DetermineLowPriorityInPhaseAction(game.GetApplicableEvents(player, true));

        var karama = Assert.IsInstanceOfType<Karma>(action);
        Assert.AreSame(player.TreacheryCards.Single(), karama.Card);
        Assert.AreEqual(FactionAdvantage.BlueUsingVoice, karama.Prevented);
        Assert.IsNull(karama.Validate());
    }

    [TestMethod]
    public void BasicBotPassesWhenBattleAdvantageDoesNotAffectItsSide()
    {
        var (game, player) = CreateResponseGame(
            FactionAdvantage.GreenBattlePlanPrescience,
            Faction.Green,
            Faction.Red,
            Faction.Yellow);
        var bot = new BasicBot(game, player, BotParameters.GetDefaultParameters(player.Faction));

        var action = bot.DetermineLowPriorityInPhaseAction(game.GetApplicableEvents(player, true));

        Assert.IsInstanceOfType<KaramaResponsePassed>(action);
    }

    private static (Game Game, Player Player) CreateResponseGame(
        FactionAdvantage advantage,
        Faction aggressor,
        Faction defender,
        Faction responder)
    {
        var game = new Game();
        game.Rules.AddRange([Rule.SE_Core, Rule.SE_Karama]);
        AddPlayer(game, Faction.Green);
        AddPlayer(game, Faction.Blue);
        AddPlayer(game, Faction.Red);
        AddPlayer(game, Faction.Yellow);

        Set(game, nameof(Game.CurrentMainPhase), MainPhase.Battle);
        Set(game, nameof(Game.CurrentPhase), Phase.KaramaResponse);
        Set(game, nameof(Game.CurrentBattle), new BattleInitiated(game, aggressor)
        {
            Target = defender,
            Territory = game.Map.Arrakeen.Territory
        });
        Set(game, nameof(Game.PendingKaramaAdvantage), advantage);
        game.KaramaResponders.Add(responder);
        var player = game.GetPlayer(responder)!;
        player.TreacheryCards.Add(TreacheryCardManager.Items.First(card => card.Type == TreacheryCardType.Karma));

        if (advantage == FactionAdvantage.BlueUsingVoice)
            Set(game, nameof(Game.CurrentVoice), new Voice(game, Faction.Blue) { Must = true, Type = TreacheryCardType.Laser });
        else
            Set(game, nameof(Game.CurrentPrescience), new Prescience(game, Faction.Green) { Aspect = PrescienceAspect.Dial });

        return (game, player);
    }

    private static Player AddPlayer(Game game, Faction faction)
    {
        var player = new Player(game, faction) { Seat = game.Players.Count };
        player.AssignLeaders(game);
        game.Players.Add(player);
        return player;
    }

    private static void Set(Game game, string propertyName, object value) =>
        typeof(Game).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(game, value);
}
