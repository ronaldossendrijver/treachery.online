/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Svg;
using Svg.Pathing;
using Svg.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Treachery.Client;
using Treachery.Shared;

namespace Treachery.Test
{

    [TestClass]
    public class Tests
    {
        private void SaveSpecialCases(Game g, GameEvent e)
        {
            if (e is TerrorRevealed)
            {
                WriteSavegameIfApplicable(g, e.Player, "Terror revealed");
            }

            /*
            if (g.CurrentTurn > 12 && !g.RecentMilestones.Contains(Milestone.BabyMonster) && g.RecentMilestones.Contains(Milestone.Monster) && (g.ResourceCardDiscardPileA.Items.Take(3).Any(c => c.IsSandTrout) || g.ResourceCardDiscardPileB.Items.Take(3).Any(c => c.IsSandTrout)))
            {
                WriteSavegameIfApplicable(g, e.Player, "Sandtrout triggered...");
            }
            */
            /*
            if (g.Version >= 149 && e is Battle && e.Player.Resources > 0 && e.Player.Ally == Faction.Yellow)
            {
                WriteSavegameIfApplicable(g, e.Player, "Battle by Fremen ally (" + e.Player.AlliedPlayer.Name + ")...");
            }
            */
            /*
            if (e is BattleConcluded && e.Initiator == Faction.Black && !g.CurrentBattle.OpponentOf(Faction.Black).Leaders.Any(l => g.IsAlive(l)) && g.CurrentBattle.PlanOfOpponent(e.Player).Hero is Leader battleLeader &&  g.IsAlive(battleLeader))
            {
                WriteSavegameIfApplicable(g, e.Player, "Capturing last leader used in battle by harkonnen opponent...");
            }

            if (e is BattleConcluded && e.Initiator == Faction.Black && !g.CurrentBattle.OpponentOf(Faction.Black).Leaders.Any(l => g.IsAlive(l)) && g.CurrentBattle.PlanOf(e.Player).Hero is Leader stolenLeader && g.IsAlive(stolenLeader))
            {
                WriteSavegameIfApplicable(g, e.Player, "Capturing last leader returned after used in battle by harkonnen...");
            }
            */
        }

        private readonly List<Type> Written = new();
        private void WriteSavegameIfApplicable(Game g, Type t)
        {
            if (!Written.Contains(t))
            {
                var playerWithAction = g.Players.FirstOrDefault(p => g.GetApplicableEvents(p, false).Contains(t));
                if (playerWithAction != null)
                {
                    lock (Written)
                    {
                        File.WriteAllText("" + (Written.Count + 100) + " " + t.Name + "-" + playerWithAction.Name.Replace('*', 'X') + ".special.json", GameState.GetStateAsString(g));
                        Written.Add(t);
                    }
                }
            }
        }

        private readonly List<string> WrittenCases = new();
        private void WriteSavegameIfApplicable(Game g, Player playerWithAction, string c)
        {
            if (!WrittenCases.Contains(c))
            {
                lock (WrittenCases)
                {
                    var id = playerWithAction == null ? "x" : playerWithAction.Name.Replace('*', 'X');
                    File.WriteAllText(c + "-" + id + ".special.json", GameState.GetStateAsString(g));
                    WrittenCases.Add(c);
                }
            }
        }

        private ObjectCounter<int> _cardcount;
        private ObjectCounter<int> _leadercount;
        private string TestIllegalCases(Game g, GameEvent e)
        {
            var p = g.GetPlayer(e.Initiator);

            p = g.Players.FirstOrDefault(p => p.ForcesInReserve < 0 || p.SpecialForcesInReserve < 0);
            if (p != null) return "Negative forces " + p.Faction + " after " + e.GetType().Name + " - " + g.History.Count;

            p = g.Players.FirstOrDefault(p => p.Faction == Faction.White && (p.SpecialForcesInReserve != 0 || p.SpecialForcesKilled != 0));
            if (p != null) return "Invalid forces " + p.Faction + " after " + e.GetType().Name + " - " + g.History.Count;

            if (g.Version >= 142)
            {
                p = g.Players.FirstOrDefault(p => p.Resources < 0);
                if (p != null) return "Negative spice " + p.Faction + " after " + e.GetType().Name + " - " + g.History.Count;
            }

            if (g.CurrentTurn >= 1)
            {
                p = g.Players.FirstOrDefault(p =>
                    p.ForcesInReserve + p.ForcesKilled + p.ForcesOnPlanet.Sum(b => b.Value.AmountOfForces) +
                    (p.Faction != Faction.White ? p.SpecialForcesInReserve + p.SpecialForcesKilled + p.ForcesOnPlanet.Sum(b => b.Value.AmountOfSpecialForces) : 0) != 20);

                if (p != null)
                {
                    return "Illegal number of forces " + p.Faction;
                }
            }

            if (g.Players.Any(p => p.Leaders.Count(l => g.IsInFrontOfShield(l)) > 1))
            {
                return "More than 1 leader in front of shield after " + e.GetType().Name + " - " + g.History.Count;
            }

            if (g.Version >= 147 && g.Players.Any(p => p.Leaders.Any(l => g.IsInFrontOfShield(l) && l.Faction != p.Faction)))
            {
                return "Foreign leader in front of shield after " + e.GetType().Name + " - " + g.History.Count;
            }

            if (g.Players.Where(p => p.Faction != Faction.Black).Any(p => p.Leaders.Count(l => g.IsSkilled(l)) > 1))
            {
                return "More than 1 skilled leader for 1 player (not counting hark) after " + e.GetType().Name + " - " + g.History.Count;
            }

            if (e is SkillAssigned sa && sa.Leader == null)
            {
                return "Assigning skill to null leader after " + e.GetType().Name + " - " + g.History.Count;
            }

            if (g.SkillDeck != null)
            {
                var allCards = g.SkillDeck.Items.Concat(g.LeaderState.Where(ls => ls.Value.Skill != LeaderSkill.None).Select(ls => ls.Value.Skill)).ToArray();

                if (allCards.Any(item => allCards.Count(c => c == item) > 1))
                {
                    return "Duplicate card in Skill Deck" + " after " + e.GetType().Name + " - " + g.History.Count;
                }
            }

            if (g.CurrentTurn >= 1)
            {
                int previousNumberOfCardsInPlay = _cardcount.CountOf(g.Seed);
                int currentNumberOfCards =
                    g.Players.Sum(player => player.TreacheryCards.Count)
                    + g.TreacheryDeck.Items.Count
                    + g.TreacheryDiscardPile.Items.Count
                    + (g.WhiteCache != null ? g.WhiteCache.Count : 0)
                    + (g.CardsOnAuction != null ? g.CardsOnAuction.Items.Count : 0)
                    + (g.Players.Any(player => g.GetCardSetAsideForBid(player) != null) ? 1 : 0)
                    + g.RemovedTreacheryCards.Count;

                if (previousNumberOfCardsInPlay == 0)
                {
                    lock (_cardcount)
                    {

                        _cardcount.SetToN(g.Seed, currentNumberOfCards);
                    }
                }
                else if (currentNumberOfCards != previousNumberOfCardsInPlay)
                {
                    return string.Format("Total number of cards has changed: {0} - {1}.",
                        previousNumberOfCardsInPlay,
                        currentNumberOfCards);
                }
            }

            if (g.CurrentTurn >= 1)
            {
                int previousNumberOfLeadersInPlay = _leadercount.CountOf(g.Seed);
                int currentNumberOfLeaders = g.Players.Sum(player => player.Leaders.Count);
                if (previousNumberOfLeadersInPlay == 0)
                {
                    lock (_leadercount)
                    {
                        _leadercount.SetToN(g.Seed, currentNumberOfLeaders);
                    }
                }
                else if (currentNumberOfLeaders != previousNumberOfLeadersInPlay)
                {
                    return string.Format("Total number of leaders has changed: {0} - {1}.",
                        previousNumberOfLeadersInPlay,
                        currentNumberOfLeaders);
                }
            }


            if (g.TreacheryDeck != null)
            {
                var allCards = g.TreacheryDeck.Items.Concat(g.TreacheryDiscardPile.Items).Concat(g.Players.SelectMany(p => p.TreacheryCards)).ToArray();
                if (allCards.Any(item => allCards.Count(c => c == item) > 1))
                {
                    return "Duplicate card in Treachery Card Deck" + " after " + e.GetType().Name + " - " + g.History.Count;
                }
            }

            if (g.ResourceCardDeck != null)
            {
                var allCards = g.ResourceCardDeck.Items.Concat(g.ResourceCardDiscardPileA.Items).Concat(g.ResourceCardDiscardPileB.Items).ToArray();
                if (allCards.Any(item => allCards.Count(c => c == item) > 1))
                {
                    return "Duplicate card in Spice Deck" + " after " + e.GetType().Name + " - " + g.History.Count;
                }
            }

            p = g.Players.FirstOrDefault(p => p.TreacheryCards.Count > p.MaximumNumberOfCards);
            if (p != null && g.CurrentPhase != Phase.PerformingKarmaHandSwap && g.CurrentPhase != Phase.Discarding)
            {
                return "Too many cards " + p + " after " + e.GetType().Name + " - " + g.History.Count;
            }

            var blue = g.GetPlayer(Faction.Blue);
            if (blue != null &&
                blue.ForcesInLocations.Any(bat => bat.Value.AmountOfSpecialForces > 0 && !g.Players.Any(p => p.Occupies(bat.Key.Territory))))
            {
                return "Lonely advisor";
            }

            if (g.Version >= 148 && blue != null && g.Map.Territories(true).Any(t => blue.ForcesIn(t) > 0 && blue.SpecialForcesIn(t) > 0))
            {
                return "Advisor and fighter together";
            }


            if (g.Players.Any(p => p.Leaders.Any(l => l.Faction != p.Faction && p.Faction != Faction.Purple && !g.CapturedLeaders.ContainsKey(l))))
            {
                return "Lost Leader";
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
                    var game = new Game(state.Version, false);
                    var testcase = new Testcase();

                    foreach (var e in state.Events)
                    {
                        e.Game = game;
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
            int expansionLevel = 3;
            var rules = Game.RulesetDefinition[Ruleset.ExpansionAdvancedGame].ToList();
            rules.Add(Rule.FillWithBots);
            var allFactions = new List<Faction> { Faction.White, Faction.Brown, Faction.Grey, Faction.Green, Faction.Orange, Faction.Red, Faction.Blue, Faction.Yellow, Faction.Purple, Faction.Black };
            int nrOfPlayers = 6;
            int nrOfTurns = 7;
            rules.Add(Rule.BotsCannotAlly);
            var rulesAsArray = rules.ToArray();

            File.AppendAllLines("results.csv", new string[] { string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                                                        "Battle_MimimumChanceToAssumeEnemyHeroSurvives",
                                                        "Battle_MimimumChanceToAssumeMyLeaderSurvives",
                                                        "Battle_MaxStrengthOfDialledForces",
                                                        "Battle_DialShortageThresholdForThrowing",
                                                        "Wins",
                                                        "Spice",
                                                        "Points",
                                                        "ForcesOnPlanet",
                                                        "Faction") });


            foreach (Faction toTest in allFactions) //10
            {
                for (float battle_MimimumChanceToAssumeEnemyHeroSurvives = 0.1f; battle_MimimumChanceToAssumeEnemyHeroSurvives <= 1; battle_MimimumChanceToAssumeEnemyHeroSurvives += 0.2f) // 5
                {
                    for (float battle_MimimumChanceToAssumeMyLeaderSurvives = 0.1f; battle_MimimumChanceToAssumeMyLeaderSurvives <= 1; battle_MimimumChanceToAssumeMyLeaderSurvives += 0.2f) //5
                    {
                        for (int Battle_MaxStrengthOfDialledForces = 8; Battle_MaxStrengthOfDialledForces <= 18; Battle_MaxStrengthOfDialledForces += 5) //3
                        {
                            for (int Battle_DialShortageThresholdForThrowing = 0; Battle_DialShortageThresholdForThrowing <= 6; Battle_DialShortageThresholdForThrowing += 3) //3
                            {
                                //10*5*5*3*3 = 2250 lines
                                var p = BotParameters.GetDefaultParameters(toTest);
                                p.Battle_MimimumChanceToAssumeEnemyHeroSurvives = battle_MimimumChanceToAssumeEnemyHeroSurvives;
                                p.Battle_MimimumChanceToAssumeMyLeaderSurvives = battle_MimimumChanceToAssumeMyLeaderSurvives;
                                p.Battle_DialShortageThresholdForThrowing = Battle_DialShortageThresholdForThrowing;

                                var pDict = new Dictionary<Faction, BotParameters>() { { toTest, p } };
                                DetermineWins(Environment.ProcessorCount * 3, expansionLevel, rulesAsArray, nrOfPlayers, nrOfTurns, pDict, toTest, out int wins, out int spice, out int points, out int forcesOnPlanet);

                                File.AppendAllLines("results.csv", new string[] { string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                                                        battle_MimimumChanceToAssumeEnemyHeroSurvives, battle_MimimumChanceToAssumeMyLeaderSurvives, Battle_MaxStrengthOfDialledForces, Battle_DialShortageThresholdForThrowing,
                                                        wins, spice, points, forcesOnPlanet, toTest) });
                            }
                        }
                    }
                }
            }
        }

        private void DetermineWins(int nrOfGames, int expansionLevel, Rule[] rules, int nrOfPlayers, int nrOfTurns, Dictionary<Faction, BotParameters> p, Faction f,
            out int wins, out int spice, out int points, out int forcesOnPlanet)
        {
            int countWins = 0;
            int countSpice = 0;
            int countPoints = 0;
            int countForcesOnPlanet = 0;

            var po = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Parallel.For(0, nrOfGames, po,
                i =>
                {
                    var game = LetBotsPlay(expansionLevel, rules, nrOfPlayers, nrOfTurns, p, false, false, null, f);
                    var playerToCheck = game.Players.Single(p => p.Faction == f);
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

            Message.DefaultDescriber = Skin.Current;

            _cardcount = new();
            _leadercount = new();

            int nrOfGames = 100;
            int nrOfTurns = 10;
            int nrOfPlayers = 6;

            Console.WriteLine("Winner;Method;Turn;Events;Leaders killed;Forces killed;Owned cards;Owned Spice;Discarded");

            //Expansion, advanced game, all expansions, all factions:
            int expansionLevel = 3;
            var rules = Game.RulesetDefinition[Ruleset.AllExpansionsAdvancedGame].ToList();
            rules.Add(Rule.FillWithBots);
            rules.Add(Rule.AssistedNotekeeping);
            rules.Add(Rule.DisableOrangeSpecialVictory);

            //rules.Add(Rule.BotsCannotAlly);

            var rulesAsArray = rules.ToArray();
            var wincounter = new ObjectCounter<Faction>();

            ParallelOptions po = new();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount;
            Parallel.For(0, nrOfGames, po,
                   index =>
                   {
                       PlayGameAndRecordResults(expansionLevel, nrOfPlayers, nrOfTurns, rulesAsArray, wincounter, statistics);
                   });

            foreach (var f in wincounter.Counted.OrderByDescending(f => wincounter.CountOf(f)))
            {
                Console.WriteLine(Skin.Current.Format("{0}: {1} ({2}%)", f, wincounter.CountOf(f), (100f * wincounter.CountOf(f) / nrOfGames)));
            }

            if (statistics != null)
            {
                statistics.Output(Skin.Current);
            }
        }

        private void PlayGameAndRecordResults(int expansionLevel, int nrOfPlayers, int nrOfTurns, Rule[] rulesAsArray, ObjectCounter<Faction> wincounter, Statistics statistics)
        {
            var game = LetBotsPlay(expansionLevel, rulesAsArray, nrOfPlayers, nrOfTurns, null, false, true, statistics);

            Console.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                string.Join(",", game.Winners.Select(p => DetermineName(p))),
                Skin.Current.Describe(game.WinMethod),
                game.CurrentTurn,
                game.History.Count,
                Skin.Current.Join(game.LeaderState.Where(l => !l.Value.Alive).Select(l => l.Key)),
                game.Players.Sum(p => p.ForcesKilled + p.SpecialForcesKilled),
                Skin.Current.Join(game.Players.SelectMany(p => p.TreacheryCards)),
                game.Players.Sum(p => p.Resources),
                Skin.Current.Join(game.TreacheryDiscardPile.Items));

            foreach (var winner in game.Winners)
            {
                wincounter.Count(winner.Faction);
            }
        }

        private readonly List<TimedTest> timedTests = new();
        private readonly List<Game> failedGames = new();
        private Game LetBotsPlay(int expansionLevel, Rule[] rules, int nrOfPlayers, int nrOfTurns, Dictionary<Faction, BotParameters> p, bool infoLogging, bool performTests, Statistics statistics, Faction mustPlay = Faction.None)
        {
            BattleOutcome previousBattleOutcome = null;

            var game = new Game(false)
            {
                BotInfologging = infoLogging,
            };

            var timer = new TimedTest(game, 60);
            timer.Elapsed += HandleElapsedTestTime;
            timedTests.Add(timer);

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
                var start = new EstablishPlayers(game) { ApplicableRules = rules.ToArray(), FactionsInPlay = factions, MaximumTurns = nrOfTurns, MaximumNumberOfPlayers = nrOfPlayers, Players = Array.Empty<string>(), Seed = new Random().Next() };
                start.Time = DateTime.Now;
                start.Execute(false, true);

                if (p != null)
                {
                    foreach (var kvp in p)
                    {
                        game.Players.Single(p => p.Faction == kvp.Key).Param = kvp.Value;
                    }
                }

                while (game.CurrentPhase != Phase.GameEnded)
                {
                    var evt = PerformBotEvent(game, performTests);

                    if (evt == null)
                    {
                        File.WriteAllText("novalidbotevent" + game.Seed + ".json", GameState.GetStateAsString(game));
                    }
                    Assert.IsNotNull(evt, "bots couldn't come up with a valid event");

                    evt.Time = DateTime.Now;

                    if (game.History.Count == nrOfTurns * 500)
                    {
                        File.WriteAllText("stuck" + game.Seed + ".json", GameState.GetStateAsString(game));
                    }
                    Assert.AreNotEqual(nrOfTurns * 500, game.History.Count, "bots got stuck");

                    if (failedGames.Contains(game))
                    {
                        File.WriteAllText("timeout" + game.Seed + ".json", GameState.GetStateAsString(game));
                        failedGames.Remove(game);
                    }
                    Assert.IsFalse(failedGames.Contains(game), "timeout");

                    if (performTests)
                    {
                        var illegalCase = TestIllegalCases(game, evt);
                        if (illegalCase != "")
                        {
                            File.WriteAllText("illegalcase" + illegalCase + "_" + game.Seed + ".json", GameState.GetStateAsString(game));
                        }
                        Assert.AreEqual("", illegalCase);

                        SaveSpecialCases(game, evt);
                    }

                    GatherStatistics(statistics, game, ref previousBattleOutcome);
                }
            }
            catch
            {
                timer.Stop();
                timedTests.Remove(timer);
                throw;
            }

            timer.Stop();
            timedTests.Remove(timer);

            return game;
        }

        private void HandleElapsedTestTime(object sender, ElapsedEventArgs e)
        {
            var game = sender as Game;
            failedGames.Add(game);
        }

        private static GameEvent PerformBotEvent(Game game, bool performTests)
        {
            var bots = Deck<Player>.Randomize(game.Players.Where(p => p.IsBot));

            var botEvents = new Dictionary<Player, IEnumerable<Type>>();
            foreach (var bot in bots)
            {
                botEvents.Add(bot, game.GetApplicableEvents(bot, true));
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
            if (performTests && executeResult != null)
            {
                File.WriteAllText("invalid" + game.Seed + ".json", GameState.GetStateAsString(game));
            }
            if (performTests) Assert.IsNull(executeResult);
        }

        [TestMethod]
        public void Regression()
        {
            var statistics = new Statistics();

            Message.DefaultDescriber = Skin.Current;

            ProfileGames();

            _cardcount = new();
            _leadercount = new();

            try
            {
                Console.WriteLine("Re-playing all savegame files in {0}...", Directory.GetCurrentDirectory());

                int gamesTested = 0;
                ParallelOptions po = new();
                po.MaxDegreeOfParallelism = Environment.ProcessorCount;
                Parallel.ForEach(Directory.EnumerateFiles(".", "savegame*.json"), po, f =>
                {
                    gamesTested++;
                    ReplayGame(f, statistics);
                });

                Assert.AreNotEqual(0, gamesTested);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            if (statistics != null)
            {
                statistics.Output(Skin.Current);
            }
        }

        private void ReplayGame(string fileData, Statistics statistics)
        {
            var fs = File.OpenText(fileData);
            var state = GameState.Load(fs.ReadToEnd());
            Console.WriteLine("Checking {0} (version {1})...", fileData, state.Version);
            var game = new Game(state.Version, false);

            fs = File.OpenText(fileData + ".testcase");
            var tc = LoadObject<Testcase>(fs.ReadToEnd());

            int valueId = 0;

            BattleOutcome previousBattleOutcome = null;
            bool gatherStatistics = false;

            foreach (var evt in state.Events)
            {
                evt.Game = game;
                var previousPhase = game.CurrentPhase;

                var result = evt.Execute(true, true);
                if (result != null)
                {
                    File.WriteAllText("invalid.json", GameState.GetStateAsString(game));
                }
                Assert.IsNull(result, fileData + ", " + evt.GetType().Name + " (" + valueId + ", " + evt.GetMessage() + ")");

                var actualValues = DetermineTestvalues(game);
                tc.Testvalues[valueId].Equals(actualValues);
                if (!tc.Testvalues[valueId].Equals(actualValues))
                {
                    File.WriteAllText("invalid.json", GameState.GetStateAsString(game));
                }
                Assert.AreEqual(tc.Testvalues[valueId], actualValues, fileData + ", " + previousPhase + " - " + game.CurrentPhase + ", " + evt.GetType().Name + " (" + valueId + ", " + evt.GetMessage() + "): " + Testvalues.Difference);

                var strangeCase = TestIllegalCases(game, evt);
                if (strangeCase != "")
                {
                    File.WriteAllText("illegalcase_" + game.EventCount + "_" + strangeCase + ".json", GameState.GetStateAsString(game));
                }
                Assert.AreEqual("", strangeCase, fileData + ", " + strangeCase);

                SaveSpecialCases(game, evt);

                valueId++;

                if (!gatherStatistics && statistics != null && evt is EstablishPlayers && game.Players.Count > 1 && game.Players.Count > 2 * game.Players.Count(p => p.IsBot))
                {
                    gatherStatistics = true;
                }

                if (gatherStatistics)
                {
                    GatherStatistics(statistics, game, ref previousBattleOutcome);
                }
            }
        }

        private void GatherStatistics(Statistics statistics, Game game, ref BattleOutcome previousBattleOutcome)
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

                    var nrOfBots = game.Players.Where(p => p.IsBot).Count();
                    if (nrOfBots > 0)
                    {
                        statistics.GamePlayerSetup.Count(string.Format("{0} players ({1} bots)", game.Players.Count, nrOfBots));
                    }
                    else
                    {
                        statistics.GamePlayerSetup.Count(string.Format("{0} players", game.Players.Count));
                    }


                    statistics.GameWinningMethods.Count(game.WinMethod);
                    statistics.GameNumberOfTurns.Count(game.CurrentTurn);
                    statistics.GameTimes.Add(latest.Time.Subtract(game.History[0].Time));

                    foreach (var p in game.Winners)
                    {
                        statistics.GameWinningPlayers.Count(DetermineName(p));
                        var fnt = new FactionAndTurn() { Faction = p.Faction, Turn = game.CurrentTurn };
                        statistics.GameWinningFactionsInTurns.Count(fnt);
                    }
                }
                else if (latest is BattleInitiated)
                {
                    statistics.Battles++;
                    statistics.BattlingFactions.Count(game.CurrentBattle.Aggressor);
                    statistics.BattlingFactions.Count(game.CurrentBattle.Defender);
                }
                else if (latest is TreacheryCalled traitorcalled && traitorcalled.TraitorCalled)
                {
                    statistics.TraitoredLeaders.Count(Skin.Current.Describe(game.CurrentBattle.PlanOfOpponent(traitorcalled.Player).Hero));
                }
                else if (latest is FaceDanced fd && !fd.Passed)
                {
                    statistics.FacedancedLeaders.Count(Skin.Current.Describe(game.WinnerHero));
                }
                else if (latest is ClairVoyancePlayed cp)
                {
                    statistics.Truthtrances.Count(cp.GetQuestion().ToString(Skin.Current).Replace(';', ':'));
                }
                else if (latest is DealAccepted da)
                {
                    statistics.AcceptedDeals.Count(da.GetDealContents().ToString(Skin.Current).Replace(';', ':'));
                }
                else if (latest is Karma karma)
                {
                    statistics.Karamas.Count(karma.Prevented);
                }
                else if (game.BattleOutcome != null && previousBattleOutcome != game.BattleOutcome)
                {
                    var outcome = game.BattleOutcome;
                    previousBattleOutcome = outcome;

                    statistics.BattleWinningFactions.Count(outcome.Winner != null ? outcome.Winner.Faction : Faction.None);
                    statistics.BattleLosingFactions.Count(outcome.Loser != null ? outcome.Loser.Faction : Faction.None);
                    statistics.BattleWinningLeaders.Count(Skin.Current.Describe(outcome.WinnerBattlePlan.Hero));
                    statistics.BattleLosingLeaders.Count(Skin.Current.Describe(outcome.LoserBattlePlan.Hero));

                    if (outcome.LoserHeroKilled) statistics.BattleKilledLeaders.Count(Skin.Current.Describe(outcome.LoserBattlePlan.Hero));
                    if (outcome.WinnerHeroKilled) statistics.BattleKilledLeaders.Count(Skin.Current.Describe(outcome.WinnerBattlePlan.Hero));

                    if (outcome.WinnerBattlePlan.Weapon != null) statistics.UsedWeapons.Count(Skin.Current.Describe(outcome.WinnerBattlePlan.Weapon));
                    if (outcome.WinnerBattlePlan.Defense != null) statistics.UsedDefenses.Count(Skin.Current.Describe(outcome.WinnerBattlePlan.Defense));
                    if (outcome.LoserBattlePlan.Weapon != null) statistics.UsedWeapons.Count(Skin.Current.Describe(outcome.LoserBattlePlan.Weapon));
                    if (outcome.LoserBattlePlan.Defense != null) statistics.UsedDefenses.Count(Skin.Current.Describe(outcome.LoserBattlePlan.Defense));
                }
                else if (game.CurrentPhase == Phase.BeginningOfCollection && latest is EndPhase)
                {
                    foreach (var p in game.Players)
                    {
                        if (p.Occupies(game.Map.Arrakeen)) statistics.FactionsOccupyingArrakeen.Count(p.Faction);
                        if (p.Occupies(game.Map.Carthag)) statistics.FactionsOccupyingCarthag.Count(p.Faction);
                        if (p.Occupies(game.Map.SietchTabr)) statistics.FactionsOccupyingSietchTabr.Count(p.Faction);
                        if (p.Occupies(game.Map.HabbanyaSietch)) statistics.FactionsOccupyingHabbanyaSietch.Count(p.Faction);
                        if (p.Occupies(game.Map.TueksSietch)) statistics.FactionsOccupyingTueksSietch.Count(p.Faction);
                        if (p.Occupies(game.Map.HiddenMobileStronghold)) statistics.FactionsOccupyingHMS.Count(p.Faction);
                    }
                }
            }
        }

        private static string DetermineName(Player p)
        {
            return p.IsBot ? Skin.Current.Format("{0}Bot", p.Faction) : p.Name.Replace(';', ':');
        }

        private static Testvalues DetermineTestvalues(Game game)
        {
            var forces = game.Forces(false);

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

            for (int i = 0; i < game.Players.Count; i++)
            {
                var p = game.Players[i];

                result.playervalues[i] = new TestvaluesPerPlayer()
                {
                    faction = p.Faction,
                    ally = p.Ally,
                    position = p.PositionAtTable,
                    resources = p.Resources,
                    bribes = p.Bribes,
                    forcesinreserve = p.ForcesInReserve,
                    specialforcesinreserve = p.SpecialForcesInReserve,
                    totaldeathcount = p.Leaders.Sum(l => game.LeaderState[l].DeathCounter),
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

        private static T LoadObject<T>(string data)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.TypeNameHandling = TypeNameHandling.All;
            var textReader = new StringReader(data);
            var jsonReader = new JsonTextReader(textReader);
            return serializer.Deserialize<T>(jsonReader);
        }


        [TestMethod]
        public void SaveAndLoadSkin()
        {
            var leader = LeaderManager.LeaderLookup.Find(1008);
            var oldName = Skin.Current.Describe(leader);
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var writer = new StringWriter();
            serializer.Serialize(writer, Skin.Dune1979);
            writer.Close();
            var skinData = writer.ToString();
            File.WriteAllText("skin.json", skinData);

            var textReader = new StringReader(File.ReadAllText("skin.json"));
            var jsonReader = new JsonTextReader(textReader);
            var skinToTest = serializer.Deserialize<Skin>(jsonReader);
            Assert.AreEqual(oldName, skinToTest.Describe(leader));
        }


        //[TestMethod]
        public void ApplyTransforms()
        {
            string filename = "e:\\svg2\\faction10force.svg";

            var accumulatedTransforms = new Stack<SvgTransformCollection>();

            var doc = SvgDocument.Open(filename);

            ApplyTransforms(doc, accumulatedTransforms, 3);

            doc.Write(filename + ".svg");
        }

        private static void ApplyTransforms(SvgElement element, Stack<SvgTransformCollection> transforms, int digits)
        {
            bool hadTransforms = element.Transforms != null;
            var newTransforms = new SvgTransformCollection();

            if (hadTransforms)
            {
                transforms.Push(element.Transforms);
            }

            if (element is SvgPath path)
            {
                foreach (var pathSegment in path.PathData)
                {
                    if (pathSegment is SvgCubicCurveSegment svgCubicCurveSegment)
                    {
                        //svgCubicCurveSegment.Start = Translate(svgCubicCurveSegment.Start, transforms, digits);
                        svgCubicCurveSegment.End = Translate(svgCubicCurveSegment.End, transforms, digits);
                        svgCubicCurveSegment.FirstControlPoint = Translate(svgCubicCurveSegment.FirstControlPoint, transforms, digits);
                        svgCubicCurveSegment.SecondControlPoint = Translate(svgCubicCurveSegment.SecondControlPoint, transforms, digits);
                    }
                    else
                    {
                        //pathSegment.Start = Translate(pathSegment.Start, transforms, digits);
                        pathSegment.End = Translate(pathSegment.End, transforms, digits);
                    }
                }
            }
            else if (element is SvgCircle circle)
            {
                var transformedCenter = Translate(circle.Center, transforms, digits);
                circle.CenterX = new SvgUnit(transformedCenter.X);
                circle.CenterY = new SvgUnit(transformedCenter.Y);
                circle.Radius = new SvgUnit(Scale(circle.Radius, transforms, digits));
            }
            else if (element is SvgUse use)
            {
                var point = new PointF() { X = use.X.Value, Y = use.Y.Value };
                var transformedPoint = Translate(point, transforms, digits);
                use.X = new SvgUnit(transformedPoint.X);
                use.Y = new SvgUnit(transformedPoint.Y);
            }
            else if (element is SvgRectangle rect)
            {
                var point = new PointF() { X = rect.X.Value, Y = rect.Y.Value };
                var transformedPoint = Translate(point, transforms, digits);
                rect.X = new SvgUnit(transformedPoint.X);
                rect.Y = new SvgUnit(transformedPoint.Y);
                rect.Width = new SvgUnit(Scale(rect.Width, transforms, digits));
                rect.Height = new SvgUnit(Scale(rect.Height, transforms, digits));
            }
            else if (element is SvgLinearGradientServer linearGradient && linearGradient.GradientUnits == SvgCoordinateUnits.UserSpaceOnUse)
            {
                transforms.Push(linearGradient.GradientTransform);
                var point1 = Translate(new PointF() { X = linearGradient.X1, Y = linearGradient.Y1 }, transforms, digits);
                linearGradient.X1 = point1.X;
                linearGradient.Y1 = point1.Y;
                var point2 = Translate(new PointF() { X = linearGradient.X2, Y = linearGradient.Y2 }, transforms, digits);
                linearGradient.X2 = point2.X;
                linearGradient.Y2 = point2.Y;
                transforms.Pop();
                linearGradient.GradientTransform.Clear();
            }
            else if (element is SvgText text)
            {
                var point = new PointF();
                foreach (var x in text.X)
                {
                    point.X = x;
                }
                foreach (var y in text.Y)
                {
                    point.Y = y;
                }

                var transformedPoint = Translate(point, transforms, digits);
                text.X.Clear();
                text.X.Add(new SvgUnit(transformedPoint.X));
                text.Y.Clear();
                text.Y.Add(new SvgUnit(transformedPoint.Y));

                float rotation = Rotation(transforms, 1);

                if (Math.Abs(rotation) > 0.005)
                {
                    newTransforms.Add(new SvgRotate(rotation * 57.2957795f, transformedPoint.X, transformedPoint.Y));
                }
            }
            else
            {
                Console.WriteLine("Skipped {0} ({1})", element.GetType(), element.ToString());
            }

            foreach (var child in element.Children)
            {
                ApplyTransforms(child, transforms, digits);
            }

            if (hadTransforms)
            {
                transforms.Pop();
                element.Transforms.Clear();
            }

            if (newTransforms.Any())
            {
                element.Transforms = newTransforms;
            }
        }

        private static PointF Translate(SvgPoint p, IEnumerable<SvgTransformCollection> transforms, int digits)
        {
            var point = new PointF() { X = p.X.Value, Y = p.Y.Value };
            return Translate(point, transforms, digits);
        }


        private static PointF Translate(PointF p, IEnumerable<SvgTransformCollection> transforms, int digits)
        {
            var thePoints = new PointF[] { p };

            foreach (var tc in transforms)
            {
                foreach (var t in tc)
                {
                    if (OperatingSystem.IsWindows())
                    {
                        t.Matrix.TransformPoints(thePoints);
                    }
                }
            }

            return new PointF() { X = (float)Math.Round(thePoints[0].X, digits), Y = (float)Math.Round(thePoints[0].Y, digits) };
        }

        private static float Rotation(IEnumerable<SvgTransformCollection> transforms, int digits)
        {
            if (OperatingSystem.IsWindows())
            {
                Matrix combinedMatrix = new();
                foreach (var tc in transforms)
                {
                    foreach (var t in tc)
                    {
                        combinedMatrix.Multiply(t.Matrix);
                    }
                }

                return (float)Math.Asin(combinedMatrix.MatrixElements.M12);
            }
            else
            {
                return 0;
            }
        }

        private static float Scale(float radius, IEnumerable<SvgTransformCollection> transforms, int digits)
        {
            foreach (var tc in transforms)
            {
                foreach (var t in tc)
                {
                    if (OperatingSystem.IsWindows())
                    {
                        radius *= t.Matrix.MatrixElements.M11;
                    }
                }
            }

            return (float)Math.Round(radius, digits);
        }
        /*
        [TestMethod]
        public static void SVG()
        {
            var accumulatedTransforms = new List<SvgTransformCollection>();

            var doc = SvgDocument.Open("c:\\map.svg");
            foreach (var group1 in doc.Children)
            {
                if (group1.ID == "Dune")
                {
                    if (group1.Transforms != null)
                    {
                        accumulatedTransforms.Add(group1.Transforms);
                    }

                    foreach (var child in group1.Children)
                    {
                        if (child.ID == "Areas")
                        {
                            if (child.Transforms != null)
                            {
                                accumulatedTransforms.Add(child.Transforms);
                            }

                            foreach (var areaOrSubgroup in child.Children)
                            {
                                Console.WriteLine(areaOrSubgroup.GetType());

                                if (areaOrSubgroup is SvgGroup)
                                {
                                    if (areaOrSubgroup.Transforms != null)
                                    {
                                        accumulatedTransforms.Add(areaOrSubgroup.Transforms);
                                    }

                                    foreach (var area in areaOrSubgroup.Children)
                                    {
                                        if (area is SvgPath)
                                        {
                                            ProcessPath(accumulatedTransforms, area);
                                        }
                                    }
                                }
                                else if (areaOrSubgroup is SvgPath)
                                {
                                    ProcessPath(accumulatedTransforms, areaOrSubgroup);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ProcessPath(List<SvgTransformCollection> accumulatedTransforms, SvgElement area)
        {
            var path = area as SvgPath;
            //Console.WriteLine(area.ID);

            var shape = new List<Segment>();

            foreach (var elt in path.PathData)
            {
                if (elt is SvgClosePathSegment)
                {
                    shape.Add(new Close());
                }
                else if (elt is SvgCubicCurveSegment)
                {
                    var x = elt as SvgCubicCurveSegment;
                    shape.Add(new BezierTo(TransformPoint(x.Start, accumulatedTransforms), TransformPoint(x.End, accumulatedTransforms), TransformPoint(x.FirstControlPoint, accumulatedTransforms), TransformPoint(x.SecondControlPoint, accumulatedTransforms)));
                }
                else if (elt is SvgLineSegment)
                {
                    var x = elt as SvgLineSegment;
                    shape.Add(new LineTo(TransformPoint(x.Start, accumulatedTransforms), TransformPoint(x.End, accumulatedTransforms)));
                }
                else if (elt is SvgMoveToSegment)
                {
                    var x = elt as SvgMoveToSegment;
                    shape.Add(new MoveTo(TransformPoint(x.Start, accumulatedTransforms), TransformPoint(x.End, accumulatedTransforms)));
                }
            }

            Console.WriteLine("private static readonly Segment[] {1} = {{ {0} }};", string.Join(", ", shape), area.ID);

            var xml = area.GetXML();
            int end = xml.IndexOf("z\"") + 1;
            var newPath = string.Join(' ', shape.Select(s => s.ToSvgString()));
            //Console.WriteLine( "{0}{1}{2}", xml.Substring(0, 9), newPath, xml.Substring(end, xml.Length - end));
        }
        */

        /*
        public static void TranslatePoints()
        {
            Game g = new();
            foreach (var l in g.Map.Locations)
            {
                Console.WriteLine("{0};{1};{2};{3};{4};{5};{6}",
                    l.Territory.Id,
                    l.Sector,
                    l.Name,
                    (int)(l.Center.X / 7.32),
                    (int)(l.Center.Y / 7.32),
                    l.SpiceLocation != null ? (int)(l.SpiceLocation.X / 7.32) : -1,
                    l.SpiceLocation != null ? (int)(l.SpiceLocation.Y / 7.32) : -1);
            }
        }

        private static PointF TransformPoint(PointF p, IEnumerable<SvgTransformCollection> transforms)
        {
            var thePoints = new PointF[] { p };

            foreach (var tc in transforms)
            {
                foreach (var t in tc)
                {
                    //t.Matrix.TransformPoints(thePoints);
                }
            }

            return new PointF((int)(7.362344583f * thePoints[0].X), (int)(7.349840256f * thePoints[0].Y));
        }
        */

        private static void SaveObject(object toSave, string filename)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;
            var writer = new StringWriter();
            serializer.Serialize(writer, toSave);
            writer.Close();
            var skinData = writer.ToString();
            File.WriteAllText(filename, skinData);
        }


        private static int GetRandomId()
        {
            return (new Random()).Next();
        }


        [TestMethod]
        public void TestShuffleMethod()
        {
            int[] counters = new int[100];

            for (int run = 0; run < 100000; run++)
            {
                var deck = new Deck<int>(Enumerable.Range(0, 100), new Random());
                deck.Shuffle();
                counters[deck.Draw()]++;
            }

            Console.WriteLine("Average times any item is on top: {0}", counters.Average());
            Console.WriteLine("Standard deviation in the times any item is on top: {0}", CalculateStandardDeviation(counters.Select(c => (double)c)));

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Times {0} was on top: {1}", i, counters[i]);
            }
        }

        private static double CalculateStandardDeviation(IEnumerable<double> values)
        {
            double standardDeviation = 0;

            if (values.Any())
            {
                // Compute the average.     
                double avg = values.Average();

                // Perform the Sum of (value-avg)_2_2.      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                // Put it all together.      
                standardDeviation = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return standardDeviation;
        }

        [TestMethod]
        public void DetermineBias()
        {
            var joined = new List<string>()
            {
                "Ronald",
                "Rene",
                "Ren�",
                "X1",
                "X2",
                ""
            };

            var factions = new List<Faction> { Faction.Green, Faction.Black, Faction.Yellow, Faction.Red, Faction.Orange, Faction.Blue };

            var assignedFactions = new List<Tuple<string, Faction>>();
            var assignedPositions = new List<Tuple<Faction, int>>();
            for (int i = 0; i < 10000; i++)
            {
                int _playerID = GetRandomId();
                var game = new Game(_playerID, false);

                game.HandleEvent(new EstablishPlayers() { Players = joined, FactionsInPlay = factions, MaximumTurns = 10, ApplicableRules = Game.RulesetDefinition[Ruleset.AdvancedGame], Seed = _playerID });

                foreach (var p in game.Players)
                {
                    assignedFactions.Add(new Tuple<string, Faction>(p.Name, p.Faction));
                    assignedPositions.Add(new Tuple<Faction, int>(p.Faction, p.PositionAtTable));
                }
            }

            Console.WriteLine("{0}: {1} {2} {3} {4} {5} {6}", "Player".PadLeft(20), "Atr".PadLeft(5), "Hrk".PadLeft(5), "Fre".PadLeft(5), "Emp".PadLeft(5), "Gld".PadLeft(5), "Bgt".PadLeft(5));
            foreach (var playerName in joined)
            {
                var a = assignedFactions.Count(t => t.Item1 == playerName && t.Item2 == Faction.Green).ToString();
                var h = assignedFactions.Count(t => t.Item1 == playerName && t.Item2 == Faction.Black).ToString();
                var f = assignedFactions.Count(t => t.Item1 == playerName && t.Item2 == Faction.Yellow).ToString();
                var e = assignedFactions.Count(t => t.Item1 == playerName && t.Item2 == Faction.Red).ToString();
                var g = assignedFactions.Count(t => t.Item1 == playerName && t.Item2 == Faction.Orange).ToString();
                var b = assignedFactions.Count(t => t.Item1 == playerName && t.Item2 == Faction.Blue).ToString();
                Console.WriteLine("{0}: {1} {2} {3} {4} {5} {6}", playerName.PadLeft(20), a.PadLeft(5), h.PadLeft(5), f.PadLeft(5), e.PadLeft(5), g.PadLeft(5), b.PadLeft(5));
            }

            Console.WriteLine("{0}: {1} {2} {3} {4} {5} {6}", "Faction".PadLeft(20), "0".PadLeft(5), "1".PadLeft(5), "2".PadLeft(5), "3".PadLeft(5), "4".PadLeft(5), "5".PadLeft(5));
            foreach (var faction in factions)
            {
                var p0 = assignedPositions.Count(t => t.Item1 == faction && t.Item2 == 0).ToString();
                var p1 = assignedPositions.Count(t => t.Item1 == faction && t.Item2 == 1).ToString();
                var p2 = assignedPositions.Count(t => t.Item1 == faction && t.Item2 == 2).ToString();
                var p3 = assignedPositions.Count(t => t.Item1 == faction && t.Item2 == 3).ToString();
                var p4 = assignedPositions.Count(t => t.Item1 == faction && t.Item2 == 4).ToString();
                var p5 = assignedPositions.Count(t => t.Item1 == faction && t.Item2 == 5).ToString();
                Console.WriteLine("{0}: {1} {2} {3} {4} {5} {6}", faction.ToString().PadLeft(20), p0.PadLeft(5), p1.PadLeft(5), p2.PadLeft(5), p3.PadLeft(5), p4.PadLeft(5), p5.PadLeft(5));
            }
        }


    }
}
