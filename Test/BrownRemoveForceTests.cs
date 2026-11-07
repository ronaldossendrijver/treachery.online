using System.Reflection;

namespace Treachery.Test;

[TestClass]
public sealed class BrownRemoveForceTests
{
    [TestMethod]
    [DataRow(0, false)]
    [DataRow(1, false)]
    [DataRow(2, false)]
    [DataRow(3, false)]
    [DataRow(4, false)]
    [DataRow(5, false)]
    [DataRow(0, true)]
    [DataRow(1, true)]
    [DataRow(2, true)]
    [DataRow(3, true)]
    [DataRow(4, true)]
    [DataRow(5, true)]
    public void BotSkipsLoneForcesInStrongholdsInStorm(int strongholdIndex, bool specialForce)
    {
        var (game, brown, opponent, strongholds) = CreateGame(strongholdIndex, specialForce);
        var stronghold = strongholds[strongholdIndex];
        SetStorm(game, stronghold.Sector);
        Assert.IsTrue(game.MeetsNormalVictoryCondition(opponent, true));
        Assert.IsFalse(BrownRemoveForce.ValidLocations(game).Contains(stronghold));
        var bot = new BasicBot(game, brown, BotParameters.GetDefaultParameters(Faction.Brown));

        var action = bot.DetermineLowPriorityInPhaseAction([typeof(BrownRemoveForce)]);

        Assert.IsNull(action);
        Assert.IsEmpty(bot.DrainInvalidDecisions());
        Assert.AreEqual(1, opponent.AnyForcesIn(stronghold));
        Assert.AreEqual(1, brown.TreacheryCards.Count);
    }

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(1, false)]
    [DataRow(2, false)]
    [DataRow(3, false)]
    [DataRow(4, false)]
    [DataRow(5, false)]
    [DataRow(0, true)]
    [DataRow(1, true)]
    [DataRow(2, true)]
    [DataRow(3, true)]
    [DataRow(4, true)]
    [DataRow(5, true)]
    public void BotStillTargetsLoneNormalAndSpecialForcesOutsideStorm(int strongholdIndex, bool specialForce)
    {
        var (game, brown, _, strongholds) = CreateGame(strongholdIndex, specialForce);
        var bot = new BasicBot(game, brown, BotParameters.GetDefaultParameters(Faction.Brown));

        var action = bot.DetermineLowPriorityInPhaseAction([typeof(BrownRemoveForce)]);

        var removal = Assert.IsInstanceOfType<BrownRemoveForce>(action);
        Assert.AreSame(strongholds[strongholdIndex], removal.Location);
        Assert.AreEqual(Faction.Red, removal.Target);
        Assert.AreEqual(specialForce, removal.SpecialForce);
        Assert.IsNull(removal.Validate());
        Assert.IsEmpty(bot.DrainInvalidDecisions());
    }

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(1, 2)]
    [DataRow(2, 3)]
    [DataRow(3, 4)]
    [DataRow(4, 5)]
    public void BotContinuesToNextEligibleStrongholdWhenPreferredTargetIsInStorm(int blockedIndex, int nextIndex)
    {
        var (game, brown, opponent, strongholds) = CreateGame(blockedIndex, true);
        opponent.RemoveForces(strongholds[nextIndex], 1);
        SetStorm(game, strongholds[blockedIndex].Sector);
        var bot = new BasicBot(game, brown, BotParameters.GetDefaultParameters(Faction.Brown));

        var action = bot.DetermineLowPriorityInPhaseAction([typeof(BrownRemoveForce)]);

        var removal = Assert.IsInstanceOfType<BrownRemoveForce>(action);
        Assert.AreSame(strongholds[nextIndex], removal.Location);
        Assert.IsFalse(removal.SpecialForce);
        Assert.IsNull(removal.Validate());
        Assert.IsEmpty(bot.DrainInvalidDecisions());
    }

    [TestMethod]
    public void BotPreservesStrongholdPriorityForLegalTargets()
    {
        var (game, brown, opponent, strongholds) = CreateGame(0, false);
        foreach (var stronghold in strongholds.Skip(1))
            opponent.RemoveForces(stronghold, 1);
        var bot = new BasicBot(game, brown, BotParameters.GetDefaultParameters(Faction.Brown));

        var action = bot.DetermineLowPriorityInPhaseAction([typeof(BrownRemoveForce)]);

        var removal = Assert.IsInstanceOfType<BrownRemoveForce>(action);
        Assert.AreSame(game.Map.Carthag, removal.Location);
        Assert.IsNull(removal.Validate());
    }

    private static (Game Game, Player Brown, Player Opponent, Location[] Strongholds) CreateGame(
        int loneForceIndex, bool specialForce)
    {
        var game = new Game { CurrentMainPhase = MainPhase.Contemplate };
        var brown = new Player(game, Faction.Brown) { Seat = 3 };
        var opponent = new Player(game, Faction.Red) { Seat = 1 };
        game.Players.AddRange([brown, opponent]);
        brown.TreacheryCards.Add(TreacheryCardManager.Items.Single(card => card.Id == TreacheryCardManager.CardTripToGamont));
        game.Map.HiddenMobileStronghold.PointAt(game, game.Map.FuneralPlain);
        Location[] strongholds =
        [
            game.Map.Carthag, game.Map.Arrakeen, game.Map.TueksSietch,
            game.Map.SietchTabr, game.Map.HabbanyaSietch, game.Map.HiddenMobileStronghold
        ];
        for (var i = 0; i < strongholds.Length; i++)
        {
            if (i == loneForceIndex && specialForce)
                opponent.AddSpecialForces(strongholds[i], 1, false);
            else
                opponent.AddForces(strongholds[i], i == loneForceIndex ? 1 : 2, false);
        }

        return (game, brown, opponent, strongholds);
    }

    private static void SetStorm(Game game, int sector) =>
        typeof(Game).GetProperty(nameof(Game.SectorInStorm), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(game, sector);
}
