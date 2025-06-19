/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Test;

[TestClass]
public class Tests
{
    private const bool GatherStatisticsDuringRegressionTest = true;
    private const bool GatherTrainingDataDuringRegressionTest = true;
    private const bool GatherCentralStyleStatistics = true;

    private void SaveSpecialCases(Game g, GameEvent e)
    {
        
    }

    private readonly List<string> _writtenCases = [];
    private void WriteSaveGameIfApplicable(Game g, Player playerWithAction, string c)
    {
        lock (_writtenCases)
        {
            if (_writtenCases.Contains(c)) return;
            var id = playerWithAction == null ? "x" : playerWithAction.Name.Replace('*', 'X');
            File.WriteAllText(c + "-" + id + ".special.json", GameState.GetStateAsString(g));
            _writtenCases.Add(c);
        }
    }

    private ObjectCounter<int> _cardCount;
    private ObjectCounter<int> _leaderCount;
    private string TestIllegalCases(Game g, GameEvent e)
    {
        if (!g.Applicable(Rule.AdvisorsDontConflictWithAlly) && e is BattleConcluded)
        {
            var battleLoser = g.GetPlayer(g.BattleLoser);
            if (battleLoser != null)
            {
                var allyOfBattleLoser = battleLoser.AlliedPlayer;
                if (battleLoser.AnyForcesIn(g.CurrentBattle.Territory) > 0 || 
                    (allyOfBattleLoser != null && (!battleLoser.Is(Faction.Pink) || !allyOfBattleLoser.Is(Faction.Blue)) && allyOfBattleLoser.OccupyingForcesIn(g.CurrentBattle.Territory) > 0)) return "Loser of battle still has forces in the territory - " + g.History.Count;
            }
        }

        var player = g.Players.FirstOrDefault(x => x.ForcesInReserve < 0 || x.SpecialForcesInReserve < 0);
        if (player != null) return "Negative forces " + player.Faction + " after " + e.GetType().Name + " - " + g.History.Count;

        player = g.Players.FirstOrDefault(x => x.Faction == Faction.White && (x.SpecialForcesInReserve != 0 || x.SpecialForcesKilled != 0));
        if (player != null) return "Invalid forces " + player.Faction + " after " + e.GetType().Name + " - " + g.History.Count;

        if (g.CurrentTurn > 1 || g.CurrentMainPhase > MainPhase.Storm)
        {
            player = g.Players.FirstOrDefault(x => x.ForcesInLocations.Keys.Any(l => l is AttachedLocation
            {
                AttachedToLocation: null
            }));
            if (player != null) return "Forces in unattached location - " + player.Faction + " after " + e.GetType().Name + " - " + g.History.Count;
        }

        if (g.Version >= 142)
        {
            player = g.Players.FirstOrDefault(x => x.Resources < 0);
            if (player != null) return "Negative spice " + player.Faction + " after " + e.GetType().Name + " - " + g.History.Count;
        }

        if (g.CurrentTurn >= 1)
        {
            player = g.Players.FirstOrDefault(x =>
                x.ForcesKilled + x.ForcesInLocations.Sum(b => b.Value.AmountOfForces) +
                (x.Faction != Faction.White ? x.SpecialForcesKilled + x.ForcesInLocations.Sum(b => b.Value.AmountOfSpecialForces) : 0) != 20);

            if (player != null && (g.Version >= 157 || !player.Is(Faction.Purple))) return "Illegal number of forces " + player.Faction + " - " + g.History.Count;
        }

        if (g.Players.Any(x => x.Leaders.Count(g.IsInFrontOfShield) > 1)) return "More than 1 leader in front of shield after " + e.GetType().Name + " - " + g.History.Count;

        if (g.Version >= 147 && g.Players.Any(x => x.Leaders.Any(l => g.IsInFrontOfShield(l) && l.Faction != x.Faction))) return "Foreign leader in front of shield after " + e.GetType().Name + " - " + g.History.Count;

        if (e is SkillAssigned { Leader: null }) return "Assigning skill to null leader after " + e.GetType().Name + " - " + g.History.Count;

        if (g.SkillDeck != null)
        {
            var allCards = g.SkillDeck.Items.Concat(g.LeaderState.Where(ls => ls.Value.Skill != LeaderSkill.None).Select(ls => ls.Value.Skill)).ToArray();

            if (allCards.Any(item => allCards.Count(c => c == item) > 1)) return "Duplicate card in Skill Deck" + " after " + e.GetType().Name + " - " + g.History.Count;
        }

        if (g.CurrentTurn >= 1)
        {
            var previousNumberOfCardsInPlay = _cardCount.CountOf(g.Seed);
            var currentNumberOfCards =
                g.Players.Sum(p => p.TreacheryCards.Count)
                + g.TreacheryDeck.Items.Count
                + g.TreacheryDiscardPile.Items.Count
                + (g.WhiteCache?.Count ?? 0)
                + (g.CardsOnAuction != null ? g.CardsOnAuction.Items.Count : 0)
                + (g.Players.Any(p => g.GetCardSetAsideForBid(p) != null) ? 1 : 0)
                + g.RemovedTreacheryCards.Count;

            if (previousNumberOfCardsInPlay == 0)
                lock (_cardCount)
                {

                    _cardCount.SetToN(g.Seed, currentNumberOfCards);
                }
            else if (currentNumberOfCards != previousNumberOfCardsInPlay)
                return string.Format("Total number of cards has changed from {0} to {1} - " + g.History.Count,
                    previousNumberOfCardsInPlay,
                    currentNumberOfCards);
        }

        if (g.CurrentTurn >= 1)
        {
            var previousNumberOfLeadersInPlay = _leaderCount.CountOf(g.Seed);
            var currentNumberOfLeaders = g.Players.Sum(p => p.Leaders.Count(l => l.HeroType != HeroType.Vidal));
            if (previousNumberOfLeadersInPlay == 0)
                lock (_leaderCount)
                {
                    _leaderCount.SetToN(g.Seed, currentNumberOfLeaders);
                }
            else if (currentNumberOfLeaders != previousNumberOfLeadersInPlay)
                return string.Format("Total number of leaders has changed from {0} to {1} - " + g.History.Count,
                    previousNumberOfLeadersInPlay,
                    currentNumberOfLeaders);
        }

        if (g.TreacheryDeck != null)
        {
            var allCards = g.TreacheryDeck.Items.Concat(g.TreacheryDiscardPile.Items).Concat(g.Players.SelectMany(x => x.TreacheryCards)).ToArray();
            if (allCards.Any(item => allCards.Count(c => c.Equals(item)) > 1)) return "Duplicate card in Treachery Card Deck" + " after " + e.GetType().Name + " - " + g.History.Count;
        }

        if (g.ResourceCardDeck != null)
        {
            var allCards = g.ResourceCardDeck.Items.Concat(g.ResourceCardDiscardPileA.Items).Concat(g.ResourceCardDiscardPileB.Items).ToArray();
            if (allCards.Any(item => allCards.Count(c => c == item) > 1)) return "Duplicate card in Spice Deck" + " after " + e.GetType().Name + " - " + g.History.Count;
        }

        player = g.Players.FirstOrDefault(p => p.TreacheryCards.Count > p.MaximumNumberOfCards);
        if (player != null && g.CurrentPhase != Phase.PerformingKarmaHandSwap && g.CurrentPhase != Phase.Discarding) return "Too many cards after " + e.GetType().Name + " - " + g.History.Count;

        var blue = g.GetPlayer(Faction.Blue);
        
        if (blue != null &&
            (!g.Applicable(Rule.DisableNovaFlipping) || g.CurrentMainPhase <= MainPhase.ShipmentAndMove) &&
            blue.ForcesInLocations.Any(bat => bat.Value.AmountOfSpecialForces > 0 && !g.Players.Any(x => x.Occupies(bat.Key.Territory))))
            return "Lonely advisor - " + g.History.Count;

        if (g.Version >= 148 && blue != null && g.Map.Territories(true).Any(t => blue.ForcesIn(t) > 0 && blue.SpecialForcesIn(t) > 0)) return "Advisor and fighter together - " + g.History.Count;

        if (g.Players.Any(x => x.Leaders.Any(l => l.HeroType != HeroType.Vidal && l.Faction != x.Faction && x.Faction != Faction.Purple && !g.CapturedLeaders.ContainsKey(l)))) return "Lost Leader - " + g.History.Count;

        if (g.CurrentPhase == Phase.BeginningOfCollection)
        {
            var terr = g.OccupyingForcesOnPlanet.Where(kvp => !kvp.Key.Equals(g.Map.PolarSink) && !g.IsInStorm(kvp.Key.Territory) && kvp.Value.Count(b => b.Faction != Faction.Pink) > 1).Select(kvp => kvp.Key).FirstOrDefault();
            if (terr != null) return "Territory occupied by more than one faction - " + g.History.Count;
        }

        var pinkPlayer = g.GetPlayer(Faction.Pink);
        if (g.Version > 170 && g.CurrentMainPhase is not MainPhase.ShipmentAndMove)
        {
            foreach (var s in g.Map.Strongholds)
            {
                if (pinkPlayer != null && pinkPlayer.AnyForcesIn(s) > 0)
                    continue; //ignore this check
                
                if (g.OccupyingForcesOnPlanet.GetValueOrDefault(s, []).Count > 2)
                {
                    return "Stronghold occupied by more than two factions - " + g.History.Count;
                }
            }
        }

        if (g.CurrentBattle != null)
        {
            var aggressor = g.CurrentBattle.AggressivePlayer;
            var defender = g.CurrentBattle.DefendingPlayer;
            if (aggressor == null || defender == null) return "Battle without aggressor or defender - " + g.History.Count;
            if (aggressor.AlliedPlayer == defender) return "Battle between allies - " + g.History.Count;
        }

        var pink = g.GetPlayer(Faction.Pink);
        if (pink != null)
        {
            var actualAmount = g.AmbassadorsOnPlanet.Count(kvp => kvp.Value != Ambassador.Blue) + g.UnassignedAmbassadors.Items.Count(a => a != Ambassador.Blue) + g.AmbassadorsSetAside.Count(a => a != Ambassador.Blue) + pink.Ambassadors.Count(a => a != Ambassador.Blue);
            var expectedAmount = EstablishPlayers.AvailableFactions().Count(f => f != Faction.Blue) - 1;

            if (actualAmount != expectedAmount)
                return $"Invalid number of ambassadors: {actualAmount} != {expectedAmount} - " + g.History.Count;
        }

        return "";
    }

    private static void ProfileGames()
    {
        Console.WriteLine("Profiling all savegame files in {0}...", Directory.GetCurrentDirectory());

        foreach (var f in Directory.EnumerateFiles(".", "savegame*.json"))
        {
            var testcaseFilename = f + ".testcase";
            if (!File.Exists(testcaseFilename))
            {
                Console.WriteLine("Profiling {0}...", f);

                var fs = File.OpenText(f);
                var state = GameState.Load(fs.ReadToEnd());
                var game = new Game(state.Version, new Participation());
                var testcase = new Testcase();

                foreach (var e in state.Events)
                {
                    e.Initialize(game);
                    e.Execute(true, true);
                    testcase.Testvalues.Add(DetermineTestvalues(game));
                }

                SaveObject(testcase, testcaseFilename);
            }
        }
    }

    //[TestMethod]
    public void ImproveBots()
    {
        //Console.WriteLine("");

        //Expansion, advanced game:
        var rules = Game.RulesetDefinition[Ruleset.ExpansionAdvancedGame].ToList();
        rules.Add(Rule.FillWithBots);
        var allFactions = new List<Faction> { Faction.White, Faction.Brown, Faction.Grey, Faction.Green, Faction.Orange, Faction.Red, Faction.Blue, Faction.Yellow, Faction.Purple, Faction.Black };
        var nrOfPlayers = 6;
        var nrOfTurns = 7;
        rules.Add(Rule.BotsCannotAlly);
        var rulesAsArray = rules.ToArray();

        File.AppendAllLines("results.csv", [
            string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
            "Battle_MinimumChanceToAssumeEnemyHeroSurvives",
            "Battle_MinimumChanceToAssumeMyLeaderSurvives",
            "Battle_MaxStrengthOfDialledForces",
            "Battle_DialShortageThresholdForThrowing",
            "Wins",
            "Spice",
            "Points",
            "ForcesOnPlanet",
            "Faction")
        ]);


        foreach (var toTest in allFactions) //10
            for (var battleMimimumChanceToAssumeEnemyHeroSurvives = 0.1f; battleMimimumChanceToAssumeEnemyHeroSurvives <= 1; battleMimimumChanceToAssumeEnemyHeroSurvives += 0.2f) // 5
            for (var battleMimimumChanceToAssumeMyLeaderSurvives = 0.1f; battleMimimumChanceToAssumeMyLeaderSurvives <= 1; battleMimimumChanceToAssumeMyLeaderSurvives += 0.2f) //5
            for (var battleMaxStrengthOfDialledForces = 8; battleMaxStrengthOfDialledForces <= 18; battleMaxStrengthOfDialledForces += 5) //3
            for (var battleDialShortageThresholdForThrowing = 0; battleDialShortageThresholdForThrowing <= 6; battleDialShortageThresholdForThrowing += 3) //3
            {
                //10*5*5*3*3 = 2250 lines
                var p = BotParameters.GetDefaultParameters(toTest);
                p.Battle_MimimumChanceToAssumeEnemyHeroSurvives = battleMimimumChanceToAssumeEnemyHeroSurvives;
                p.Battle_MimimumChanceToAssumeMyLeaderSurvives = battleMimimumChanceToAssumeMyLeaderSurvives;
                p.Battle_DialShortageThresholdForThrowing = battleDialShortageThresholdForThrowing;

                var pDict = new Dictionary<Faction, BotParameters> { { toTest, p } };
                DetermineWins(Environment.ProcessorCount * 3, rulesAsArray, nrOfPlayers, nrOfTurns, pDict, toTest, out var wins, out var spice, out var points, out var forcesOnPlanet);

                File.AppendAllLines("results.csv", [
                    string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                    battleMimimumChanceToAssumeEnemyHeroSurvives, battleMimimumChanceToAssumeMyLeaderSurvives, battleMaxStrengthOfDialledForces, battleDialShortageThresholdForThrowing,
                    wins, spice, points, forcesOnPlanet, toTest)
                ]);
            }
    }

    private void DetermineWins(int nrOfGames, Rule[] rules, int nrOfPlayers, int nrOfTurns, Dictionary<Faction, BotParameters> p, Faction f,
        out int wins, out int spice, out int points, out int forcesOnPlanet)
    {
        var countWins = 0;
        var countSpice = 0;
        var countPoints = 0;
        var countForcesOnPlanet = 0;

        var po = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        Parallel.For(0, nrOfGames, po,
            _ =>
            {
                var game = LetBotsPlay(rules, nrOfPlayers, nrOfTurns, p, false, null, 30, f);
                var playerToCheck = game.Players.Single(x => x.Faction == f);
                if (game.Winners.Contains(playerToCheck)) countWins++;
                countSpice += playerToCheck.Resources;
                countPoints += game.NumberOfVictoryPoints(playerToCheck, true);
                countForcesOnPlanet += playerToCheck.ForcesInLocations.Sum(kvp => kvp.Value.TotalAmountOfForces);
            });

        wins = countWins;
        spice = countSpice;
        points = countPoints;
        forcesOnPlanet = countForcesOnPlanet;
    }

    [TestMethod]
    public void TestBots()
    {
        var statistics = new Statistics();

        Message.DefaultDescriber = DefaultSkin.Default;

        _cardCount = new ObjectCounter<int>();
        _leaderCount = new ObjectCounter<int>();

        var nrOfGames = 64;
        var nrOfTurns = 10;
        var nrOfPlayers = 6;

        var timeout = 10;

        Console.WriteLine("Winner;Method;Turn;Events;Leaders killed;Forces killed;Owned cards;Owned Spice;Discarded");

        //Expansion, advanced game, all expansions, all factions:
        var rules = Game.RulesetDefinition[Ruleset.AllExpansionsAdvancedGame].ToList();
        rules.Add(Rule.FillWithBots);
        rules.Add(Rule.AssistedNotekeeping);

        var rulesAsArray = rules.ToArray();
        var wincounter = new ObjectCounter<Faction>();

        ParallelOptions po = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        Parallel.For(0, nrOfGames, po, _ =>
            {
                PlayGameAndRecordResults(nrOfPlayers, nrOfTurns, rulesAsArray, wincounter, statistics, timeout);
            });

        foreach (var f in wincounter.Counted.OrderByDescending(f => wincounter.CountOf(f))) Console.WriteLine(DefaultSkin.Default.Format("{0}: {1} ({2}%)", f, wincounter.CountOf(f), 100f * wincounter.CountOf(f) / nrOfGames));

        statistics.Output(DefaultSkin.Default);
    }

    private void PlayGameAndRecordResults(int nrOfPlayers, int nrOfTurns, Rule[] rulesAsArray, ObjectCounter<Faction> winCounter, Statistics statistics, int timeout)
    {
        var game = LetBotsPlay(rulesAsArray, nrOfPlayers, nrOfTurns, null, true, statistics, timeout);

        Console.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
            string.Join(",", game.Winners.Select(DetermineName)),
            DefaultSkin.Default.Describe(game.WinMethod),
            game.CurrentTurn,
            game.History.Count,
            DefaultSkin.Default.Join(LeaderManager.Leaders.Where(l => !game.IsAlive(l))),
            game.Players.Sum(p => p.ForcesKilled + p.SpecialForcesKilled),
            DefaultSkin.Default.Join(game.Players.SelectMany(p => p.TreacheryCards)),
            game.Players.Sum(p => p.Resources),
            DefaultSkin.Default.Join(game.TreacheryDiscardPile.Items));

        foreach (var winner in game.Winners) winCounter.Count(winner.Faction);
    }

    private readonly List<Game> _failedGames = [];
    private Game LetBotsPlay(Rule[] rules, int nrOfPlayers, int nrOfTurns, Dictionary<Faction, BotParameters> parameters, bool performTests, Statistics statistics, int timeout, Faction mustPlay = Faction.None)
    {
        BattleOutcome previousBattleOutcome = null;
        IBid previousWinningBid = null;

        var game = new Game();
        var timer = new TimedTest(game, timeout);
        
        timer.Elapsed += HandleElapsedTestTime;

        List<Faction> factions;
        if (mustPlay != Faction.None)
        {
            factions = EstablishPlayers.AvailableFactions().ToList().Where(f => f != mustPlay).TakeRandomN(nrOfPlayers - 1).ToList();
            factions.Add(mustPlay);
        }
        else
        {
            factions = EstablishPlayers.AvailableFactions().ToList();
        }

        try
        {
            var start = new EstablishPlayers(game, Faction.None)
            {
                Players = [],
                Seed = new Random().Next(),
                Time = DateTime.Now,
                Settings = new GameSettings
                {
                    InitialRules = rules.ToList(),
                    AllowedFactionsInPlay = factions,
                    MaximumTurns = nrOfTurns,
                    NumberOfPlayers = nrOfPlayers,
                }
            };
            start.Execute(false, true);

            if (parameters != null)
                foreach (var kvp in parameters) game.Players.Single(p => p.Faction == kvp.Key).Param = kvp.Value;

            var maxNumberOfEvents = game.CurrentTurn * game.Players.Count * 60;

            while (game.CurrentPhase != Phase.GameEnded)
            {
                var evt = PerformBotEvent(game, performTests);

                if (evt == null) File.WriteAllText("novalidbotevent" + game.Seed + ".json", GameState.GetStateAsString(game));
                Assert.IsNotNull(evt, "bots couldn't come up with a valid event");

                evt.Time = DateTime.Now;

                if (game.History.Count == maxNumberOfEvents) File.WriteAllText("stuck" + game.Seed + ".json", GameState.GetStateAsString(game));
                Assert.AreNotEqual(maxNumberOfEvents, game.History.Count, "bots got stuck");

                Assert.IsFalse(_failedGames.Contains(game), "timeout");

                if (performTests)
                {
                    var illegalCase = TestIllegalCases(game, evt);
                    if (illegalCase != "") File.WriteAllText("illegalcase" + illegalCase + "_" + game.Seed + ".json", GameState.GetStateAsString(game));
                    Assert.AreEqual("", illegalCase);

                    SaveSpecialCases(game, evt);
                }

                GatherStatistics(statistics, game, ref previousBattleOutcome, ref previousWinningBid);
            }
        }
        catch
        {
            timer.Stop();
            throw;
        }

        timer.Stop();
        return game;
    }

    private void HandleElapsedTestTime(object sender, ElapsedEventArgs e)
    {
        var game = sender as Game;
        File.WriteAllText("timeout" + game?.Seed + ".json", GameState.GetStateAsString(game));
        _failedGames.Add(game);
    }

    private static GameEvent PerformBotEvent(Game game, bool performTests)
    {
        var bots = Deck<Player>.Randomize(game.Players.Where(p => p.IsBot));

        var botEvents = new Dictionary<Player, List<Type>>();
        foreach (var bot in bots) 
            botEvents.Add(bot, game.GetApplicableEvents(bot, true));

        foreach (var bot in bots)
        {
            var evt = bot.DetermineHighestPrioInPhaseAction(botEvents[bot]);

            if (evt != null)
            {
                ExecuteBotEvent(game, performTests, evt);
                return evt;
            }
        }

        foreach (var bot in bots)
        {
            var evt = bot.DetermineHighPrioInPhaseAction(botEvents[bot]);

            if (evt != null)
            {
                ExecuteBotEvent(game, performTests, evt);
                return evt;
            }
        }

        foreach (var bot in bots)
        {
            var evt = bot.DetermineMiddlePrioInPhaseAction(botEvents[bot]);

            if (evt != null)
            {
                ExecuteBotEvent(game, performTests, evt);
                return evt;
            }
        }

        foreach (var bot in bots)
        {
            var evt = bot.DetermineLowPrioInPhaseAction(botEvents[bot]);

            if (evt != null)
            {
                ExecuteBotEvent(game, performTests, evt);
                return evt;
            }
        }

        foreach (var bot in bots)
        {
            var evt = bot.DetermineEndPhaseAction(botEvents[bot]);

            if (evt != null)
            {
                ExecuteBotEvent(game, performTests, evt);
                return evt;
            }
        }

        return null;
    }

    private static void ExecuteBotEvent(Game game, bool performTests, GameEvent evt)
    {
        var executeResult = evt.Execute(performTests, true);

        var msg = "";
        if (performTests && executeResult != null)
        {
            File.WriteAllText("invalid" + game.Seed + ".json", GameState.GetStateAsString(game));
            msg = executeResult.ToString();
        }

        if (performTests) Assert.IsNull(executeResult, msg);
    }

    [TestMethod]
    public void Regression()
    {
        var statistics = new Statistics();
        var trainingData = new TrainingData();
        var centralStyleStatistics = new ConcurrentBag<string>();

        if (GatherCentralStyleStatistics)
        {
            File.WriteAllText("CentralStyleStatistics.json", "{\r\n  \"entries\": [");            
        }

        Message.DefaultDescriber = DefaultSkin.Default;

        ProfileGames();

        _cardCount = new ObjectCounter<int>();
        _leaderCount = new ObjectCounter<int>();

        try
        {
            Console.WriteLine("Re-playing all savegame files in {0}...", Directory.GetCurrentDirectory());

            var gamesTested = 0;
            ParallelOptions po = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Parallel.ForEach(Directory.EnumerateFiles(".", "savegame*.json"), po, fileName =>
            {
                gamesTested++;
                var testcaseFileName = fileName + ".testcase";

                ReplayGame(fileName, testcaseFileName, statistics, trainingData, centralStyleStatistics);
            });

            Assert.AreNotEqual(0, gamesTested);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }

        if (GatherStatisticsDuringRegressionTest)
        {
            statistics.Output(DefaultSkin.Default);    
        }
        
        if (GatherTrainingDataDuringRegressionTest)
        {
            trainingData.Output();    
        }

        if (GatherCentralStyleStatistics)
        {
            foreach (var item in centralStyleStatistics)
            {
                File.AppendAllText("CentralStyleStatistics.json", item);
            }

            File.AppendAllText("CentralStyleStatistics.json", "]\r\n}");
        }
    }

    private void ReplayGame(string fileName, string testcaseFileName, Statistics statistics, TrainingData trainingData, ConcurrentBag<string> centralStyleStatistics)
    {
        var fs = File.OpenText(fileName);
        var stateData = fs.ReadToEnd();
        var state = GameState.Load(stateData);
        fs.Close();
        Console.WriteLine("Checking {0} (version {1})...", fileName, state.Version);
        var game = new Game(state.Version, new Participation());

        fs = File.OpenText(testcaseFileName);
        var tc = Utilities.Deserialize<Testcase>(fs.ReadToEnd());
        fs.Close();

        var valueId = 0;

        BattleOutcome previousBattleOutcome = null;
        IBid previousWinningBid = null;
        
        foreach (var evt in state.Events)
        {
            evt.Initialize(game);
            var previousPhase = game.CurrentPhase;

            var result = evt.Execute(true, true);
            if (result != null) File.WriteAllText("invalid" + game.Seed + ".json", GameState.GetStateAsString(game));
            Assert.IsNull(result, fileName + ", " + evt.GetType().Name + " (" + valueId + ", " + evt.GetMessage() + "): " + result);

            var actualValues = DetermineTestvalues(game);
            if (!tc.Testvalues[valueId].Equals(actualValues)) 
                File.WriteAllText("invalid" + game.Seed + ".json", GameState.GetStateAsString(game));
            
            Assert.AreEqual(tc.Testvalues[valueId], actualValues, fileName + ", " + previousPhase + " - " + game.CurrentPhase + ", " + evt.GetType().Name + " (" + valueId + ", " + evt.GetMessage() + "): " + Testvalues.Difference);

            var strangeCase = TestIllegalCases(game, evt);
            if (strangeCase != "") File.WriteAllText("illegalCase_" + game.EventCount + "_" + strangeCase + ".json", GameState.GetStateAsString(game));
            Assert.AreEqual("", strangeCase, fileName + ", " + strangeCase);

            SaveSpecialCases(game, evt);

            valueId++;

            if (GatherStatisticsDuringRegressionTest) 
                GatherStatistics(statistics, game, ref previousBattleOutcome, ref previousWinningBid);
            
            if (GatherTrainingDataDuringRegressionTest)
                GatherTrainingData(trainingData, game);
        }

        if (GatherCentralStyleStatistics)
        {
            var centralStyleStats = GameStatistics.GetStatistics(game);
            var data = Utilities.Serialize(centralStyleStats);
            centralStyleStatistics.Add(data);
        }
    }

    private static void GatherStatistics(Statistics statistics, Game game, ref BattleOutcome previousBattleOutcome, ref IBid previousWinningBid)
    {
        lock (statistics)
        {
            var latest = game.LatestEvent();

            if (game.CurrentPhase == Phase.GameEnded && latest is EndPhase)
            {
                statistics.PlayedGames++;
                statistics.GameTypes.Count(Game.DetermineApproximateRuleset(game.FactionsInPlay, game.Rules, Game.ExpansionLevel));

                foreach (var p in game.Players)
                {
                    statistics.GamePlayingPlayers.Count(DetermineName(p));
                    statistics.GamePlayingFactions.Count(p.Faction);
                }

                var nrOfBots = game.Players.Count(p => p.IsBot);
                statistics.GamePlayerSetup.Count(nrOfBots > 0
                    ? $"{game.Players.Count} players ({nrOfBots} bots)"
                    : $"{game.Players.Count} players");


                statistics.GameWinningMethods.Count(game.WinMethod);
                statistics.GameNumberOfTurns.Count(game.CurrentTurn);
                statistics.GameTimes.Add(latest.Time.Subtract(game.History[0].Time));
                statistics.GamesPerMonth.Count(game.Started.ToString("yyyyMM"));

                foreach (var p in game.Winners)
                {
                    statistics.GameWinningPlayers.Count(DetermineName(p));
                    var fnt = new FactionAndTurn { Faction = p.Faction, Turn = game.CurrentTurn };
                    statistics.GameWinningFactionsInTurns.Count(fnt);
                }
            }
            else if ((latest is BattleInitiated && game.CurrentBattle != null) || latest is BattleClaimed)
            {
                statistics.Battles++;
                statistics.BattlingFactions.Count(game.CurrentBattle.Aggressor);
                statistics.BattlingFactions.Count(game.CurrentBattle.Defender);
            }
            else if (latest is TreacheryCalled { Succeeded: true } treacheryCalled)
            {
                statistics.TraitoredLeaders.Count(DefaultSkin.Default.Describe(game.CurrentBattle.PlanOfOpponent(treacheryCalled.Player).Hero));
            }
            else if (latest is FaceDanced { Passed: false })
            {
                statistics.FacedancedLeaders.Count(DefaultSkin.Default.Describe(game.WinnerHero));
            }
            else if (latest is ClairVoyancePlayed cp)
            {
                statistics.Truthtrances.Count(cp.GetQuestion().ToString(DefaultSkin.Default).Replace(';', ':'));
            }
            else if (latest is DealAccepted da)
            {
                statistics.AcceptedDeals.Count(da.GetDealContents().ToString(DefaultSkin.Default).Replace(';', ':'));
            }
            else if (latest is Karma karma)
            {
                statistics.Karamas.Count(karma.Prevented);
            }
            else if (game.Applicable(Rule.AdvancedCombat) && latest is Battle && game.AggressorPlan != null && game.DefenderPlan != null)
            {
                if (!game.AggressorPlan.Player.IsBot)
                {
                    statistics.BattleSpiceUsed.Add(new Tuple<Faction, int, float>(game.AggressorPlan.Initiator, game.CurrentTurn, game.AggressorPlan.Cost(game)));
                    statistics.BattleTotalDialUsed.Add(new Tuple<Faction, int, float>(game.AggressorPlan.Initiator, game.CurrentTurn, game.AggressorPlan.Dial(game, game.DefenderPlan.Initiator)));
                    statistics.BattleForceTokensUsed.Add(new Tuple<Faction, int, float>(game.AggressorPlan.Initiator, game.CurrentTurn, game.AggressorPlan.TotalForces));
                }

                if (!game.DefenderPlan.Player.IsBot)
                {
                    statistics.BattleSpiceUsed.Add(new Tuple<Faction, int, float>(game.DefenderPlan.Initiator, game.CurrentTurn, game.DefenderPlan.Cost(game)));
                    statistics.BattleTotalDialUsed.Add(new Tuple<Faction, int, float>(game.DefenderPlan.Initiator, game.CurrentTurn, game.DefenderPlan.Dial(game, game.AggressorPlan.Initiator)));
                    statistics.BattleForceTokensUsed.Add(new Tuple<Faction, int, float>(game.DefenderPlan.Initiator, game.CurrentTurn, game.DefenderPlan.TotalForces));

                }
            }
            else if (latest is Shipment ship)
            {
                statistics.ShipSpiceUsed.Add(new Tuple<Faction, int, float>(ship.Initiator, game.CurrentTurn, Shipment.DetermineCost(game, ship.Player, ship)));
            }
            else if (latest is Revival revive)
            {
                statistics.ReviveSpiceUsed.Add(new Tuple<Faction, int, float>(revive.Initiator, game.CurrentTurn, Revival.DetermineCost(game, revive.Player, revive.Hero, revive.AmountOfForces, revive.AmountOfSpecialForces, revive.ExtraForcesPaidByRed, revive.ExtraSpecialForcesPaidByRed, revive.UsesRedSecretAlly).TotalCost));
            }
            
            if (game.WinningBid != null && game.WinningBid != previousWinningBid)
            {
                previousWinningBid = game.WinningBid;
                statistics.BidSpiceUsed.Add(new Tuple<Faction, int, float>(game.WinningBid.Initiator, game.CurrentTurn, game.WinningBid.KarmaCard == null ? game.WinningBid.TotalAmount : 0));
            }
                
            if (game.BattleOutcome != null && previousBattleOutcome != game.BattleOutcome)
            {
                var outcome = game.BattleOutcome;
                previousBattleOutcome = outcome;

                statistics.BattleWinningFactions.Count(outcome.Winner?.Faction ?? Faction.None);
                statistics.BattleLosingFactions.Count(outcome.Loser?.Faction ?? Faction.None);
                statistics.BattleWinningLeaders.Count(DefaultSkin.Default.Describe(outcome.WinnerBattlePlan.Hero));
                statistics.BattleLosingLeaders.Count(DefaultSkin.Default.Describe(outcome.LoserBattlePlan.Hero));

                if (outcome.LoserHeroKilled) statistics.BattleKilledLeaders.Count(DefaultSkin.Default.Describe(outcome.LoserBattlePlan.Hero));
                if (outcome.WinnerHeroKilled) statistics.BattleKilledLeaders.Count(DefaultSkin.Default.Describe(outcome.WinnerBattlePlan.Hero));

                if (outcome.WinnerBattlePlan.Weapon != null) statistics.UsedWeapons.Count(DefaultSkin.Default.Describe(outcome.WinnerBattlePlan.Weapon));
                if (outcome.WinnerBattlePlan.Defense != null) statistics.UsedDefenses.Count(DefaultSkin.Default.Describe(outcome.WinnerBattlePlan.Defense));
                if (outcome.LoserBattlePlan.Weapon != null) statistics.UsedWeapons.Count(DefaultSkin.Default.Describe(outcome.LoserBattlePlan.Weapon));
                if (outcome.LoserBattlePlan.Defense != null) statistics.UsedDefenses.Count(DefaultSkin.Default.Describe(outcome.LoserBattlePlan.Defense));
            }
                
            if (game.CurrentPhase == Phase.BeginningOfCollection && latest is EndPhase)
                foreach (var p in game.Players)
                {
                    if (p.Occupies(game.Map.Arrakeen)) statistics.FactionsOccupyingArrakeen.Count(p.Faction);
                    if (p.Occupies(game.Map.Carthag)) statistics.FactionsOccupyingCarthag.Count(p.Faction);
                    if (p.Occupies(game.Map.SietchTabr)) statistics.FactionsOccupyingSietchTabr.Count(p.Faction);
                    if (p.Occupies(game.Map.HabbanyaSietch)) statistics.FactionsOccupyingHabbanyaSietch.Count(p.Faction);
                    if (p.Occupies(game.Map.TueksSietch)) statistics.FactionsOccupyingTueksSietch.Count(p.Faction);
                    if (p.Occupies(game.Map.HiddenMobileStronghold)) statistics.FactionsOccupyingHms.Count(p.Faction);
                }
        }
    }
    
    private static void GatherTrainingData(TrainingData trainingData, Game game)
    {
        lock (trainingData)
        {
            var latest = game.LatestEvent();

            if (game.CurrentPhase == Phase.GameEnded && latest is EndPhase)
            {
                trainingData.DeleteDecisionsByLosers(game.Seed, game.Winners.Select(x => x.Faction).ToArray());
            }
            else if (latest is Shipment or Move or Voice or Prescience or Battle or Bid)
            {
                var state = GetPlayerKnowledge(game, latest.Initiator);
                trainingData.Decisions.Add(new DecisionInfo(game.Seed, state, latest));
            }
        }
    }

    private static PlayerKnowledge GetPlayerKnowledge(Game game, Faction faction)
    {
        var player = game.GetPlayer(faction);
        var green = game.GetPlayer(Faction.Green);
        var blue = game.GetPlayer(Faction.Blue);
        
        var result = new PlayerKnowledge
        {
            I =
            {
                Faction = faction,
                Ally = player.Ally,
                Spice = player.Resources,
                CardIds = player.TreacheryCards.Select(x => x.Id).ToHashSet(),
                TraitorIds = player.Traitors.Select(x => x.Id).ToHashSet(),
                FaceDancerIds = player.FaceDancers.Select(x => x.Id).ToHashSet(),
                LivingLeaderIds = player.Leaders.Where(game.IsAlive).Select(x => x.Id).ToHashSet(),
                MustSupportForcesInBattle = Battle.MustPayForAnyForcesInBattle(game, player),
                MustSupportSpecialForcesInBattle = Battle.MustPayForSpecialForcesInBattle(game, player),
                CanUseAdvancedKarama = !game.KarmaPrevented(faction) && !player.SpecialKarmaPowerUsed && game.Applicable(Rule.AdvancedKarama),
                ForcesInReserve = player.ForcesInReserve,
                SpecialForcesInReserve = player.SpecialForcesInReserve,
            },
            
            Ally = DetermineKnownPlayerInfo(player.Ally, game, player, true),
            YellowOpponent = DetermineKnownPlayerInfo(Faction.Yellow, game, player),
            GreenOpponent = DetermineKnownPlayerInfo(Faction.Green, game, player),
            BlackOpponent = DetermineKnownPlayerInfo(Faction.Black, game, player),
            RedOpponent = DetermineKnownPlayerInfo(Faction.Red, game, player),
            OrangeOpponent = DetermineKnownPlayerInfo(Faction.Orange, game, player),
            BlueOpponent = DetermineKnownPlayerInfo(Faction.Blue, game, player),
            GreyOpponent = DetermineKnownPlayerInfo(Faction.Grey, game, player),
            PurpleOpponent = DetermineKnownPlayerInfo(Faction.Purple, game, player),
            BrownOpponent = DetermineKnownPlayerInfo(Faction.Brown, game, player),
            WhiteOpponent = DetermineKnownPlayerInfo(Faction.White, game, player),
            PinkOpponent = DetermineKnownPlayerInfo(Faction.Pink, game, player),
            CyanOpponent = DetermineKnownPlayerInfo(Faction.Cyan, game, player)
        };

        foreach (var l in game.Map.Locations(true))
        {
            result.Locations.Add(DetermineLocationInfo(game, l, player));
        }

        result.Homeworlds = game.Applicable(Rule.Homeworlds);
        result.AllyBlocksAdvisors = !game.Applicable(Rule.AdvisorsDontConflictWithAlly);
        result.LatestAtreidesOrAllyBidAmount =
            game.Bids.Values.LastOrDefault(x=> !x.Passed && (x.Initiator == Faction.Green || x.Initiator == green?.Ally))?.TotalAmount ?? -1;
        result.TreacheryCardOnBidId = game.HasBiddingPrescience(player) && !game.CardsOnAuction.IsEmpty? game.CardsOnAuction.Top.Id  : -1;
        result.PredictedFaction = blue?.PredictedFaction ?? Faction.None;
        result.PredictedTurn = blue?.PredictedTurn ?? -1;
        result.KwizatsAvailable = green?.MessiahAvailable == true;
        
        return result;
    }

    private static LocationInfo DetermineLocationInfo(Game game, Location l, Player player)
    {
        return new LocationInfo
        {
            Id = l.Id,
            Spice = game.ResourcesOnPlanet.GetValueOrDefault(l, 0),
            Terror = game.TerrorIn(l.Territory).FirstOrDefault(),
            Ambassador = game.AmbassadorIn(l.Territory),
            InStorm = game.IsInStorm(l),
            ProtectedFromStorm = l.IsProtectedFromStorm,
            SuffersStormNextTurn = game.HasStormPrescience(player) && game.NextStormWillPassOver(l),
            HasWormNextTurn = game.HasResourceDeckPrescience(player) && !game.ResourceCardDeck.IsEmpty && game.ResourceCardDeck.Top.Territory == l.Territory,
            
            MyForces = player.ForcesIn(l),
            MySpecialForces = player.SpecialForcesIn(l),
            
            AllyForces = player.AlliedPlayer?.ForcesIn(l) ?? 0,
            AllySpecialForces = player.AlliedPlayer?.SpecialForcesIn(l) ?? 0,
            
            YellowOpponentForces = game.GetPlayer(Faction.Yellow)?.ForcesIn(l) ?? 0,
            GreenOpponentForces = game.GetPlayer(Faction.Green)?.ForcesIn(l) ?? 0,  
            BlackOpponentForces = game.GetPlayer(Faction.Black)?.ForcesIn(l) ?? 0,  
            RedOpponentForces = game.GetPlayer(Faction.Red)?.ForcesIn(l) ?? 0,      
            OrangeOpponentForces = game.GetPlayer(Faction.Orange)?.ForcesIn(l) ?? 0,
            BlueOpponentForces = game.GetPlayer(Faction.Blue)?.ForcesIn(l) ?? 0,    
            GreyOpponentForces = game.GetPlayer(Faction.Grey)?.ForcesIn(l) ?? 0,    
            PurpleOpponentForces = game.GetPlayer(Faction.Purple)?.ForcesIn(l) ?? 0,
            BrownOpponentForces = game.GetPlayer(Faction.Brown)?.ForcesIn(l) ?? 0,  
            WhiteOpponentForces = game.GetPlayer(Faction.White)?.ForcesIn(l) ?? 0,  
            PinkOpponentForces = game.GetPlayer(Faction.Pink)?.ForcesIn(l) ?? 0,    
            CyanOpponentForces = game.GetPlayer(Faction.Cyan)?.ForcesIn(l) ?? 0,    
            
            YellowOpponentSpecialForces = game.GetPlayer(Faction.Yellow)?.SpecialForcesIn(l) ?? 0,
            GreenOpponentSpecialForces = game.GetPlayer(Faction.Green)?.SpecialForcesIn(l) ?? 0,  
            BlackOpponentSpecialForces = game.GetPlayer(Faction.Black)?.SpecialForcesIn(l) ?? 0,  
            RedOpponentSpecialForces = game.GetPlayer(Faction.Red)?.SpecialForcesIn(l) ?? 0,      
            OrangeOpponentSpecialForces = game.GetPlayer(Faction.Orange)?.SpecialForcesIn(l) ?? 0,
            BlueOpponentSpecialForces = game.GetPlayer(Faction.Blue)?.SpecialForcesIn(l) ?? 0,    
            GreyOpponentSpecialForces = game.GetPlayer(Faction.Grey)?.SpecialForcesIn(l) ?? 0,    
            PurpleOpponentSpecialForces = game.GetPlayer(Faction.Purple)?.SpecialForcesIn(l) ?? 0,
            BrownOpponentSpecialForces = game.GetPlayer(Faction.Brown)?.SpecialForcesIn(l) ?? 0,  
            WhiteOpponentSpecialForces = game.GetPlayer(Faction.White)?.SpecialForcesIn(l) ?? 0,  
            PinkOpponentSpecialForces = game.GetPlayer(Faction.Pink)?.SpecialForcesIn(l) ?? 0,    
            CyanOpponentSpecialForces = game.GetPlayer(Faction.Cyan)?.SpecialForcesIn(l) ?? 0,    
        };
    }

    private static PlayerInfo DetermineKnownPlayerInfo(Faction faction, Game game, Player me, bool processAlly = false)
    {
        if (me.Ally != faction || !processAlly) 
            return new PlayerInfo();
            
        var otherPlayer = game.GetPlayer(faction);
        
        if (otherPlayer == null) 
            return new PlayerInfo();
        
        return new PlayerInfo
        {
            Faction = otherPlayer.Faction,
            Ally = otherPlayer.Ally,
            Spice = otherPlayer.Resources,
            CardIds = otherPlayer.TreacheryCards.Where(c => me.KnownCards.Contains(c)).Select(x => x.Id).ToHashSet(),
            TraitorIds = otherPlayer.Traitors.Where(c => me.RevealedTraitors.Contains(c)).Select(x => x.Id).ToHashSet(),
            FaceDancerIds = otherPlayer.FaceDancers.Where(c => me.RevealedDancers.Contains(c)).Select(x => x.Id).ToHashSet(),
            LivingLeaderIds = otherPlayer.Leaders.Where(game.IsAlive).Select(x => x.Id).ToHashSet(),
            MustSupportForcesInBattle = Battle.MustPayForAnyForcesInBattle(game, otherPlayer),
            MustSupportSpecialForcesInBattle = Battle.MustPayForSpecialForcesInBattle(game, otherPlayer),
            CanUseAdvancedKarama = !game.KarmaPrevented(otherPlayer.Faction) && !otherPlayer.SpecialKarmaPowerUsed && game.Applicable(Rule.AdvancedKarama),
            ForcesInReserve = otherPlayer.ForcesInReserve,
            SpecialForcesInReserve = otherPlayer.SpecialForcesInReserve,
            CanShipAndMoveThisTurn = game.CurrentMainPhase is MainPhase.ShipmentAndMove && !game.HasActedOrPassed.Contains(faction),
        };
    }

    private static string DetermineName(Player p)
    {
        return p.IsBot ? DefaultSkin.Default.Format("{0}Bot", p.Faction) : p.Name.Replace(';', ':');
    }

    private static Testvalues DetermineTestvalues(Game game)
    {
        var forces = game.Forces();

        var result = new Testvalues
        {
            currentPhase = game.CurrentPhase,
            forcesinArrakeen = forces[game.Map.Arrakeen].Sum(b => b.TotalAmountOfForces),
            forcesinCarthag = forces[game.Map.Carthag].Sum(b => b.TotalAmountOfForces),
            forcesinTabr = forces[game.Map.SietchTabr].Sum(b => b.TotalAmountOfForces),
            forcesinHabbanya = forces[game.Map.HabbanyaSietch].Sum(b => b.TotalAmountOfForces),
            forcesinTuek = forces[game.Map.TueksSietch].Sum(b => b.TotalAmountOfForces),
            nrofplayers = game.Players.Count,
            playervalues = new TestvaluesPerPlayer[game.Players.Count]
        };

        for (var i = 0; i < game.Players.Count; i++)
        {
            var p = game.Players[i];

            result.playervalues[i] = new TestvaluesPerPlayer
            {
                faction = p.Faction,
                ally = p.Ally,
                position = p.Seat,
                resources = p.Resources,
                bribes = p.Bribes,
                forcesinreserve = p.ForcesInReserve,
                specialforcesinreserve = p.SpecialForcesInReserve,
                totaldeathcount = p.Leaders.Sum(game.DeathCount),
                cardcount = p.TreacheryCards.Count,
                cardtypes = p.TreacheryCards.Sum(c => (int)c.Type),
                traitors = p.Traitors.Sum(t => t.Id),
                facedancers = p.FaceDancers.Sum(t => t.Id),
                totalforcesonplanet = p.ForcesInLocations.Where(kvp => kvp.Key is not Homeworld).Sum(kvp => kvp.Value.AmountOfForces),
                totalspecialforcesonplanet = p.ForcesInLocations.Where(kvp => kvp.Key is not Homeworld).Sum(kvp => kvp.Value.AmountOfSpecialForces),
                forceskilled = p.ForcesKilled,
                specialforceskilled = p.SpecialForcesKilled,
                nroftechtokens = p.TechTokens.Count
            };
        }

        return result;
    }

    [TestMethod]
    public void SaveAndLoadSkin()
    {
        var leader = LeaderManager.LeaderLookup.Find(1008);
        var oldName = DefaultSkin.Default.Describe(leader);
        var skinData = Utilities.Serialize(DefaultSkin.Default);
        File.WriteAllText("skin.json", skinData);

        var skinToTest = Utilities.Deserialize<Skin>(File.ReadAllText("skin.json"));
        Assert.AreEqual(oldName, skinToTest.Describe(leader));
    }

    private static void SaveObject(object toSave, string filename)
    {
        var skinData = Utilities.Serialize(toSave);
        File.WriteAllText(filename, skinData);
    }

    [TestMethod]
    public void ScanForUndecoratedGetOnlyProperties()
    {
        var assembly = Assembly.GetAssembly(typeof(GameEvent));
        if (assembly == null)
            return;

        var gameEventType = typeof(GameEvent);
        foreach (var type in assembly.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(gameEventType)))
        {
            var serializerAtt = gameEventType.GetCustomAttributes<JsonDerivedTypeAttribute>().FirstOrDefault(att => att.DerivedType == type);
            Assert.IsNotNull(serializerAtt,
                $"JsonDerivedType attribute missing for {type}");

            foreach (var prop in type.GetProperties().Where(x => !x.CanWrite))
            {
                var att = prop.GetCustomAttribute(typeof(JsonIgnoreAttribute));
                Assert.IsNotNull(att,
                    $"Get-only property {prop} of class {type} does not have the JsonIgnore attribute");
            }
        }
    }
    
    [TestMethod]
    public void ScanForMapErrors()
    {
        var map = new Map();
        map.Initialize();
        var issueFound = false;

        foreach (var l in map.Locations(false))
        {
            var asymNeighbour = l.Neighbours.FirstOrDefault(neighbour => !neighbour.Neighbours.Contains(l));
            if (asymNeighbour != null)
            {
                issueFound = true;
                Console.WriteLine($"Asymmetrical: {DefaultSkin.Default.Describe(l)}[{l.Id}] <-> {DefaultSkin.Default.Describe(asymNeighbour)}[{asymNeighbour.Id}]");
            }
        }

        Assert.IsFalse(issueFound, "Asymmetrical neighbour relationship detected");
    }
}