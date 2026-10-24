using System.IO;

namespace Treachery.Test;

[TestClass]
public sealed class BotFactoryTests
{
    private sealed class FakeBot(Game game, Player player) : BasicBot(game, player, BotParameters.GetDefaultParameters(player.Faction));

    private sealed class FakeAdvancedBotProvider : IBotProvider
    {
        public BotType Type => BotType.Advanced;

        public IBot Create(Game game, Player player) => new FakeBot(game, player);
    }

    [TestMethod]
    public void AdvancedBotsRuleIsABotRuleAndSelectsAdvancedBots()
    {
        var game = new Game();
        Assert.Contains(Rule.AdvancedBots, Game.GetRulesInGroup(RuleGroup.Bots, 0).ToList());
        Assert.AreEqual("Use advanced Bots (beta)", DefaultSkin.Default.Describe(Rule.AdvancedBots));

        Assert.AreEqual(BotType.Basic, BotFactory.BotTypeFor(game));
        game.RulesForBots.Add(Rule.AdvancedBots);
        Assert.AreEqual(BotType.Advanced, BotFactory.BotTypeFor(game));
    }

    [TestMethod]
    [DoNotParallelize]
    public void AdvancedBotsFallBackToBasicBotsWhenNoProviderIsRegistered()
    {
        var game = new Game();
        var player = new Player(game, Faction.Green);
        game.Players.Add(player);

        BotFactory.Unregister(BotType.Advanced);
        Assert.IsFalse(BotFactory.IsAvailable(BotType.Advanced));
        Assert.IsTrue(BotFactory.IsAvailable(BotType.Basic));
        Assert.IsInstanceOfType<BasicBot>(BotFactory.Create(BotType.Advanced, game, player));

        try
        {
            BotFactory.Register(new FakeAdvancedBotProvider());
            Assert.IsTrue(BotFactory.IsAvailable(BotType.Advanced));
            Assert.IsInstanceOfType<FakeBot>(BotFactory.Create(BotType.Advanced, game, player));
            Assert.IsNotInstanceOfType<FakeBot>(BotFactory.Create(BotType.Basic, game, player));
        }
        finally
        {
            BotFactory.Unregister(BotType.Advanced);
        }
    }

    [TestMethod]
    public void LoadingPluginsFromADirectoryWithoutPluginsLoadsNothing()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            Assert.IsEmpty(BotFactory.LoadPlugins(directory.FullName));
            Assert.IsEmpty(BotFactory.LoadPlugins(Path.Combine(directory.FullName, "missing")));
        }
        finally
        {
            directory.Delete(true);
        }
    }
}