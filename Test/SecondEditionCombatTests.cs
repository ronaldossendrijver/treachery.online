using System.Reflection;

namespace Treachery.Test;

[TestClass]
public class SecondEditionCombatTests
{
    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(1, 0)]
    [DataRow(2, 1)]
    [DataRow(3, 1)]
    [DataRow(4, 2)]
    [DataRow(7, 2)]
    [DataRow(8, 3)]
    [DataRow(20, 3)]
    public void BonusUsesForceThresholds(int forceStrength, int bonus) =>
        Assert.AreEqual(bonus, Battle.BonusForForceStrength(forceStrength));

    [TestMethod]
    public void SardaukarThresholdUsesWeightedForceStrengthAndFremenException()
    {
        var game = CreateGame(Faction.Red, Faction.Green);
        var plan = new Battle(game, Faction.Red) { SpecialForces = 2, SpiceBonus = 2 };
        Assert.AreEqual(2, plan.MaximumSpiceBonus(game));
        Assert.AreEqual(6f, plan.Dial(game, Faction.Green));
        Assert.AreEqual(4, plan.Cost(game));
        Set(game, nameof(Game.CurrentBattle), new BattleInitiated(game, Faction.Red) { Target = Faction.Yellow });
        Assert.AreEqual(1, plan.MaximumSpiceBonus(game));
    }

    [TestMethod]
    public void EmperorAllyDiscountAndSolarisAreDefaultOnly()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        var player = game.GetPlayer(Faction.Green)!;
        player.Ally = Faction.Red;
        var plan = new Battle(game, Faction.Green) { Forces = 8, SpiceBonus = 3 };
        Assert.AreEqual(4, plan.Cost(game));
        game.Rules.Remove(Rule.SE_SpiceBonus);
        game.Rules.Add(Rule.AdvancedCombat);
        Assert.AreEqual(8, plan.Cost(game));
        Assert.AreEqual(0, plan.EffectiveSpiceBonus(game));
        Assert.IsNotNull(plan.Validate());
    }

    [TestMethod]
    public void FremenAutomaticallyReceiveMaximumBonusWithoutSpice()
    {
        var game = CreateGame(Faction.Yellow, Faction.Red);
        var plan = new Battle(game, Faction.Yellow) { Forces = 8 };
        Assert.AreEqual(3, plan.EffectiveSpiceBonus(game));
        Assert.AreEqual(11f, plan.Dial(game, Faction.Red));
        Assert.AreEqual(0, plan.Cost(game));
    }

    [TestMethod]
    public void RevealingPaymentsIsIdempotentAndDiscardsWinnerWorthlessCards()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        var player = game.GetPlayer(Faction.Green)!;
        player.Resources = 10;
        var worthless = TreacheryCardManager.Items.First(c => c.IsUseless);
        player.TreacheryCards.Add(worthless);
        var plan = new Battle(game, Faction.Green) { Forces = 4, SpiceBonus = 2, Weapon = worthless };
        Set(game, nameof(Game.AggressorPlan), plan);
        Set(game, nameof(Game.DefenderPlan), new Battle(game, Faction.Red));
        Invoke(game, "RevealSecondEditionBattlePayments");
        Invoke(game, "RevealSecondEditionBattlePayments");
        Assert.AreEqual(6, player.Resources);
        Assert.IsFalse(player.Has(worthless));
        Assert.AreEqual(1, game.TreacheryDiscardPile!.Items.Count(c => c == worthless));
    }

    [TestMethod]
    public void TraitorWinnerReceivesOnlyTheirOwnBonusPaymentBack()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        var green = game.GetPlayer(Faction.Green)!;
        var red = game.GetPlayer(Faction.Red)!;
        green.Resources = red.Resources = 10;
        var winnerPlan = new Battle(game, Faction.Green) { Forces = 4, SpiceBonus = 2 };
        var loserPlan = new Battle(game, Faction.Red) { Forces = 4, SpiceBonus = 2, Hero = red.Leaders.First() };
        Set(game, nameof(Game.AggressorPlan), winnerPlan);
        Set(game, nameof(Game.DefenderPlan), loserPlan);
        Invoke(game, "RevealSecondEditionBattlePayments");
        Invoke(game, "OneTraitorCalled", game.Map.Arrakeen.Territory, green, red, loserPlan, winnerPlan);
        Assert.AreEqual(10 + loserPlan.Hero!.Value, green.Resources);
        Assert.AreEqual(6, red.Resources);
    }

    [TestMethod]
    public void LeaderEligibilityIsLatchedAndRepeatedlyKilledLeaderRemainsRevivable()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        var player = game.GetPlayer(Faction.Green)!;
        foreach (var leader in player.Leaders) game.LeaderState[leader].DeathCounter = 1;
        Assert.IsTrue(Revival.NormallyRevivableHeroes(game, player).Contains(player.Leaders[0]));
        game.LeaderState[player.Leaders[1]].DeathCounter = 2;
        game.LeaderState[player.Leaders[0]].DeathCounter = 3;
        Assert.IsTrue(Revival.NormallyRevivableHeroes(game, player).Contains(player.Leaders[0]));
    }

    [TestMethod]
    public void MessiahCanReviveBeforeOrdinaryLeaderEligibilityForTwoSpice()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        var player = game.GetPlayer(Faction.Green)!;
        game.LeaderState[LeaderManager.Messiah].DeathCounter = 1;
        Assert.IsTrue(Revival.NormallyRevivableHeroes(game, player).Contains(LeaderManager.Messiah));
        Assert.AreEqual(2, Revival.GetPriceOfHeroRevival(game, player, LeaderManager.Messiah));
    }

    [TestMethod]
    public void GholaCanReviveMultipleStarsWithoutConsumingNormalStarRevival()
    {
        var game = CreateGame(Faction.Red, Faction.Green);
        var player = game.GetPlayer(Faction.Red)!;
        player.SpecialForcesKilled = 4;
        player.TreacheryCards.Add(TreacheryCardManager.Items.First(c => c.Type == TreacheryCardType.RaiseDead));
        game.FactionsThatRevivedSpecialForcesThisTurn.Add(Faction.Red);
        var ghola = new RaiseDeadPlayed(game, Faction.Red) { AmountOfSpecialForces = 4, Hero = null };
        Assert.IsNull(ghola.Validate());
        Assert.AreEqual(4, RaiseDeadPlayed.ValidMaxAmount(game, player, true));
    }

    [TestMethod]
    public void DefaultPlansRejectOldHalfStrengthFields()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        Assert.IsNotNull(new Battle(game, Faction.Green) { ForcesAtHalfStrength = 1 }.Validate());
        Assert.IsNotNull(new Battle(game, Faction.Green) { Forces = -1 }.Validate());
        Assert.AreEqual(0, Battle.MaxAllyResources(game, game.GetPlayer(Faction.Green)!, 8, 0));
    }

    [TestMethod]
    public void TruthtranceDoesNotOfferFutureBattleCommitments()
    {
        var game = CreateGame(Faction.Green, Faction.Red);
        var questions = ClairVoyancePlayed.ValidQuestions(game, Faction.Red).ToArray();
        Assert.IsFalse(questions.Contains(ClairvoyanceQuestion.WillAttackX));
        Assert.IsFalse(questions.Contains(ClairvoyanceQuestion.LeaderInBattle));
        Assert.IsTrue(questions.Contains(ClairvoyanceQuestion.LeaderAsTraitor));
    }

    [TestMethod]
    public void NormalVictoryUsesSecondEditionPriorityBeforeStrongholdCount()
    {
        var game = CreateGame(Faction.Green, Faction.Yellow, Faction.Red, Faction.Black);
        game.Rules.Add(Rule.SE_Victory);
        var green = game.GetPlayer(Faction.Green)!;
        var red = game.GetPlayer(Faction.Red)!;
        var black = game.GetPlayer(Faction.Black)!;
        red.Ally = black.Faction;
        black.Ally = red.Faction;

        green.AddForces(game.Map.Arrakeen, 1, false);
        green.AddForces(game.Map.SietchTabr, 1, false);
        red.AddForces(game.Map.Carthag, 1, false);
        red.AddForces(game.Map.HabbanyaSietch, 1, false);
        red.AddForces(game.Map.TueksSietch, 1, false);
        green.TechTokens.AddRange([TechToken.Graveyard, TechToken.Resources, TechToken.Ships]);
        red.TechTokens.AddRange([TechToken.Graveyard, TechToken.Resources, TechToken.Ships]);

        Invoke(game, "CheckNormalWin");

        Assert.AreEqual(1, game.Winners.Count);
        Assert.AreSame(green, game.Winners[0]);
    }

    [TestMethod]
    public void FinalRoundTiebreakFavorsUnalliedFactionOverAlliance()
    {
        var game = CreateGame(Faction.Green, Faction.Orange, Faction.Red);
        game.Rules.Add(Rule.SE_Victory);
        var green = game.GetPlayer(Faction.Green)!;
        var orange = game.GetPlayer(Faction.Orange)!;
        var red = game.GetPlayer(Faction.Red)!;
        orange.Ally = red.Faction;
        red.Ally = orange.Faction;

        green.AddForces(game.Map.Arrakeen, 1, false);
        green.AddForces(game.Map.SietchTabr, 1, false);
        orange.AddForces(game.Map.Carthag, 1, false);
        orange.AddForces(game.Map.HabbanyaSietch, 1, false);

        Invoke(game, "CheckOtherWinConditions");

        Assert.AreEqual(1, game.Winners.Count);
        Assert.AreSame(green, game.Winners[0]);
    }

    [TestMethod]
    public void UnalliedGuildSpecialVictoryComparesAgainstIndividualFactionCounts()
    {
        var game = CreateGame(Faction.Orange, Faction.Green, Faction.Black);
        game.Rules.Add(Rule.SE_Victory);
        var guild = game.GetPlayer(Faction.Orange)!;
        var green = game.GetPlayer(Faction.Green)!;
        var black = game.GetPlayer(Faction.Black)!;
        green.Ally = black.Faction;
        black.Ally = green.Faction;

        guild.AddForces(game.Map.SietchTabr, 1, false);
        green.AddForces(game.Map.Arrakeen, 1, false);
        black.AddForces(game.Map.Carthag, 1, false);

        Invoke(game, "CheckSpecialWinConditions");

        Assert.AreEqual(1, game.Winners.Count);
        Assert.AreSame(guild, game.Winners[0]);
        Assert.AreEqual(WinMethod.GuildSpecial, game.WinMethod);
    }

    private static Game CreateGame(params Faction[] factions)
    {
        var game = new Game();
        game.Rules.AddRange([Rule.SE_Core, Rule.SE_SpiceBonus, Rule.SE_BattleResolution, Rule.SE_LeaderRevival, Rule.SE_CardTiming]);
        foreach (var faction in factions)
        {
            var player = new Player(game, faction) { Resources = 100, Seat = game.Players.Count };
            player.AssignLeaders(game);
            game.Players.Add(player);
        }
        Set(game, nameof(Game.CurrentBattle), new BattleInitiated(game, factions[0]) { Target = factions[1], Territory = game.Map.Arrakeen.Territory });
        Set(game, nameof(Game.TreacheryDiscardPile), new Deck<TreacheryCard>(new LoggedRandom()));
        return game;
    }

    private static void Set(Game game, string name, object value) =>
        typeof(Game).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(game, value);

    private static void Invoke(Game game, string name, params object?[] values) =>
        typeof(Game).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, values);
}
