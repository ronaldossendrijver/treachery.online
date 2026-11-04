using Treachery.Bots;
using Treachery.Shared;

namespace Treachery.Test;

[TestClass]
public sealed class VoiceTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void BotPassesWhenKnownCheapHeroCannotBeVoicedByCategory(bool secondEdition)
    {
        var game = CreateBattle(Faction.Blue);
        if (secondEdition)
            game.Rules.AddRange([Rule.SE_Core, Rule.SE_VoiceByCategory]);
        var blue = game.GetPlayer(Faction.Blue)!;
        var green = game.GetPlayer(Faction.Green)!;
        var cheapHero = TreacheryCardManager.Items.First(card => card.Type == TreacheryCardType.Mercenary);
        green.TreacheryCards.Add(cheapHero);
        blue.KnownCards.Add(cheapHero);
        var bot = new BasicBot(game, blue, BotParameters.GetDefaultParameters(Faction.Blue));

        var action = bot.DetermineHighPriorityInPhaseAction(game.GetApplicableEvents(blue, false));

        var voice = Assert.IsInstanceOfType<Voice>(action);
        Assert.AreEqual(secondEdition, voice.Passed);
        if (!secondEdition)
        {
            Assert.AreEqual(TreacheryCardType.Mercenary, voice.Type);
            Assert.IsTrue(voice.Must);
        }
        Assert.AreSame(game, voice.Game);
        Assert.IsNotNull(voice.GetMessage());
        Assert.IsNull(voice.Validate());
        Assert.AreEqual(0, bot.ExceptionCount);
        Assert.IsEmpty(bot.DrainInvalidDecisions());
        Assert.IsNull(voice.Execute(true, true));
        Assert.AreEqual(secondEdition, game.VoiceWasPassed);
    }

    [TestMethod]
    [DataRow(Faction.Blue)]
    [DataRow(Faction.White)]
    public void PassingVoiceResolvesTheDecisionWithoutUsingThePower(Faction faction)
    {
        var game = CreateBattle(faction);
        var player = game.GetPlayer(faction)!;
        var green = game.GetPlayer(Faction.Green)!;
        var nexus = player.Nexus;
        var bot = new BasicBot(game, green, BotParameters.GetDefaultParameters(Faction.Green));

        Assert.IsTrue(Voice.MayUseVoice(game, player));
        Assert.IsNull(bot.DetermineMiddlePriorityInPhaseAction([typeof(Prescience)]));
        Assert.IsNotNull(new Voice(game, faction).Validate());

        var pass = new Voice(game, faction) { Passed = true };
        Assert.IsNull(pass.Execute(true, true));

        Assert.IsTrue(game.VoiceWasPassed);
        Assert.IsNull(game.CurrentVoice);
        Assert.AreEqual(nexus, player.Nexus);
        Assert.IsFalse(Voice.MayUseVoice(game, player));
        Assert.DoesNotContain(typeof(Voice), game.GetApplicableEvents(player, false));
        Assert.DoesNotContain(Milestone.Voice, game.RecentMilestones);
        Assert.Contains("won't use Voice", pass.GetMessage().ToString(DefaultSkin.Default));
        Assert.IsEmpty(GameStatus.DetermineStatus(game, player, true).FlashInfo);
        Assert.IsNotNull(new Voice(game, faction) { Passed = true }.Validate());
        Assert.IsNotNull(new Voice(game, faction) { Type = TreacheryCardType.Projectile }.Validate());

        var prescience = bot.DetermineMiddlePriorityInPhaseAction([typeof(Prescience)]);
        Assert.IsInstanceOfType<Prescience>(prescience);
        Assert.IsNull(prescience.Execute(true, true));
        Assert.IsNotNull(game.CurrentPrescience);
    }

    [TestMethod]
    [DataRow(Faction.Blue)]
    [DataRow(Faction.White)]
    public void PassingVoiceDoesNotRevokeASubmittedBattlePlan(Faction faction)
    {
        var game = CreateBattle(faction);
        var plan = new Battle(game, Faction.Green);
        Assert.IsNull(plan.Execute(false, true));

        Assert.IsNull(new Voice(game, faction)
        {
            Passed = true,
            Must = true,
            Type = TreacheryCardType.Projectile
        }.Execute(true, true));

        Assert.AreSame(plan, game.AggressorPlan);
        Assert.IsNull(game.CurrentVoice);
    }

    [TestMethod]
    [DataRow(Faction.Blue)]
    [DataRow(Faction.White)]
    public void UsingVoiceStillAppliesThePowerAndRevokesTheOpponentsPlan(Faction faction)
    {
        var game = CreateBattle(faction);
        Assert.IsNull(new Battle(game, Faction.Green).Execute(false, true));
        var voice = new Voice(game, faction) { Must = true, Type = TreacheryCardType.Projectile };

        Assert.IsNull(voice.Execute(true, true));

        Assert.AreSame(voice, game.CurrentVoice);
        Assert.IsFalse(game.VoiceWasPassed);
        Assert.IsNull(game.AggressorPlan);
        Assert.AreEqual(Faction.None, game.GetPlayer(faction)!.Nexus);
        Assert.Contains(Milestone.Voice, game.RecentMilestones);
        Assert.IsNotEmpty(GameStatus.DetermineStatus(game, game.GetPlayer(faction), true).FlashInfo);
    }

    [TestMethod]
    public void PassingVoiceRequiresPermissionToUseVoice()
    {
        var game = CreateBattle(Faction.White);
        var pass = new Voice(game, Faction.Green) { Passed = true };

        Assert.IsNotNull(pass.Validate());
        Assert.IsNotNull(pass.Execute(true, true));
        Assert.IsFalse(game.VoiceWasPassed);
        Assert.IsTrue(Voice.MayUseVoice(game, game.GetPlayer(Faction.White)!));
    }

    [TestMethod]
    [DataRow(Faction.Blue)]
    [DataRow(Faction.Red)]
    public void PassingVoiceResolvesTheSharedDecisionForBeneGesseritAndTheirAlly(Faction initiator)
    {
        var game = CreateBattle(Faction.Red);
        var ally = game.GetPlayer(Faction.Red)!;
        var blue = new Player(game, Faction.Blue) { Ally = Faction.Red, Seat = 2 };
        ally.Ally = Faction.Blue;
        game.Players.Add(blue);
        typeof(Game).GetProperty(nameof(Game.BlueAllowsUseOfVoice))!.SetValue(game, true);

        Assert.IsTrue(Voice.MayUseVoice(game, blue));
        Assert.IsTrue(Voice.MayUseVoice(game, ally));
        Assert.IsNull(new Voice(game, initiator) { Passed = true }.Execute(true, true));

        Assert.IsFalse(Voice.MayUseVoice(game, blue));
        Assert.IsFalse(Voice.MayUseVoice(game, ally));
    }

    [TestMethod]
    [DataRow(Faction.Blue)]
    [DataRow(Faction.White)]
    public void EndingTheBattleRestoresTheVoiceOpportunity(Faction faction)
    {
        var game = CreateBattle(faction);
        Assert.IsNull(new Voice(game, faction) { Passed = true }.Execute(true, true));
        typeof(Game).GetProperty(nameof(Game.BattleSequence))!.SetValue(game, new PlayerSequence(game));
        typeof(Game).GetProperty(nameof(Game.CurrentPhase))!.SetValue(game, Phase.BattleReport);

        Assert.IsNull(new EndPhase(game, Faction.None).Execute(false, true));
        Assert.IsFalse(game.VoiceWasPassed);
        Assert.IsNull(game.CurrentVoice);
        Assert.IsNull(game.CurrentBattle);
        Assert.IsNull(new BattleInitiated(game, Faction.Green)
        {
            Target = faction,
            Territory = game.Map.Arrakeen.Territory
        }.Execute(false, true));

        Assert.IsTrue(Voice.MayUseVoice(game, game.GetPlayer(faction)!));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void SerializedVoiceEventsPreserveWhetherTheDecisionWasPassed(bool passed)
    {
        var game = CreateBattle(Faction.White);
        GameEvent original = new Voice(game, Faction.White)
        {
            Passed = passed,
            Type = TreacheryCardType.Projectile
        };
        var restored = Utilities.Deserialize<GameEvent>(Utilities.Serialize(original));

        Assert.IsInstanceOfType<Voice>(restored);
        restored.Initialize(game);
        Assert.AreEqual(passed, ((Voice)restored).Passed);
        Assert.IsNull(restored.Execute(true, true));
        Assert.AreEqual(passed, game.VoiceWasPassed);
        Assert.AreEqual(passed, game.CurrentVoice == null);
        Assert.AreEqual(passed ? Faction.Blue : Faction.None, game.GetPlayer(Faction.White)!.Nexus);
    }

    [TestMethod]
    public void HistoricalVoiceEventsWithoutPassedStillUseThePower()
    {
        var game = CreateBattle(Faction.White);
        var json = System.Text.Json.Nodes.JsonNode.Parse(Utilities.Serialize<GameEvent>(
            new Voice(game, Faction.White) { Type = TreacheryCardType.Projectile }))!.AsObject();
        Assert.IsTrue(json.Remove(nameof(Voice.Passed)));
        var restored = Utilities.Deserialize<GameEvent>(json.ToJsonString());

        Assert.IsInstanceOfType<Voice>(restored);
        restored.Initialize(game);
        Assert.IsFalse(((Voice)restored).Passed);
        Assert.IsNull(restored.Execute(true, true));
        Assert.AreSame(restored, game.CurrentVoice);
        Assert.IsFalse(game.VoiceWasPassed);
    }

    internal static Game CreateBattle(Faction voiceFaction)
    {
        var game = new Game { CurrentMainPhase = MainPhase.Battle };
        var green = new Player(game, Faction.Green) { Seat = 0 };
        var opponent = new Player(game, voiceFaction)
        {
            Seat = 1,
            Nexus = voiceFaction == Faction.White ? Faction.Blue : Faction.None
        };
        game.Players.AddRange([green, opponent]);
        green.AddForces(game.Map.Arrakeen, 1, false);
        opponent.AddForces(game.Map.Arrakeen, 1, false);
        Assert.IsNull(new BattleInitiated(game, Faction.Green)
        {
            Target = voiceFaction,
            Territory = game.Map.Arrakeen.Territory
        }.Execute(false, true));
        typeof(Game).GetProperty(nameof(Game.CurrentPhase))!.SetValue(game, Phase.BattlePhase);
        return game;
    }
}
