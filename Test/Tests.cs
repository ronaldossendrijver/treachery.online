/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Treachery.Client;
using Treachery.Shared;
using System.Collections.Concurrent;


namespace Treachery.Test
{

    [TestClass]
    public class Tests
    {
        int gameId = 0;
        int terrorEventId = 0;
        int nrOfBgDeadLeaders = 0;

        private void SaveSpecialCases(Game g, GameEvent e)
        {
            var blue = g.GetPlayer(Faction.Blue);
            if (blue != null)
            {
                if (g.CurrentPhase == Phase.TerrorTriggeredByBlueAccompaniesOrangeShip && g.TerrorIn(g.LastShipmentOrMovement?.To.Territory).Contains(TerrorType.Assassination))
                {
                    gameId = g.Seed;
                    terrorEventId = g.History.Count;
                    nrOfBgDeadLeaders = blue.Leaders.Count(l => !g.IsAlive(l));
                }

                if (g.Seed == gameId && e is TerrorRevealed tr && tr.Type == TerrorType.Assassination && g.History.Count == terrorEventId + 1)
                {
                    if (blue.Leaders.Count(l => !g.IsAlive(l)) != nrOfBgDeadLeaders + 1)
                    {
                        WriteSavegameIfApplicable(g, e.Player, "Failed Assassination");
                    }
                    else
                    {
                        WriteSavegameIfApplicable(g, e.Player, "Assassination");
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

            if (!g.Applicable(Rule.AdvisorsDontConflictWithAlly) && e is BattleConcluded)
            {
                var battleLoser = g.GetPlayer(g.BattleLoser);
                if (battleLoser != null)
                {
                    var allyOfBattleLoser = battleLoser.AlliedPlayer;
                    if (battleLoser.AnyForcesIn(g.CurrentBattle.Territory) > 0 || allyOfBattleLoser != null && allyOfBattleLoser.AnyForcesIn(g.CurrentBattle.Territory) > 0)
                    {
                        return "Loser of battle still has forces in the territory";
                    }
                }
            }

            p = g.Players.FirstOrDefault(p => p.ForcesInReserve < 0 || p.SpecialForcesInReserve < 0);
            if (p != null) return "Negative forces " + p.Faction + " after " + e.GetType().Name + " - " + g.History.Count;

            p = g.Players.FirstOrDefault(p => p.Faction == Faction.White && (p.SpecialForcesInReserve != 0 || p.SpecialForcesKilled != 0));
            if (p != null) return "Invalid forces " + p.Faction + " after " + e.GetType().Name + " - " + g.History.Count;

            if (g.CurrentTurn > 1 || g.CurrentMainPhase > MainPhase.Storm)
            {
                p = g.Players.FirstOrDefault(p => p.ForcesInLocations.Keys.Any(l => l is AttachedLocation al && al.AttachedToLocation == null));
                if (p != null) return "Forces in unattached location - " + p.Faction + " after " + e.GetType().Name + " - " + g.History.Count;
            }

            if (g.Version >= 142)
            {
                p = g.Players.FirstOrDefault(p => p.Resources < 0);
                if (p != null) return "Negative spice " + p.Faction + " after " + e.GetType().Name + " - " + g.History.Count;
            }

            if (g.CurrentTurn >= 1)
            {
                p = g.Players.FirstOrDefault(p =>
                    p.ForcesKilled + p.ForcesInLocations.Sum(b => b.Value.AmountOfForces) +
                    (p.Faction != Faction.White ? p.SpecialForcesKilled + p.ForcesInLocations.Sum(b => b.Value.AmountOfSpecialForces) : 0) != 20);

                if (p != null && (g.Version >= 157 || !p.Is(Faction.Purple)))
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
                    return string.Format("Total number of cards has changed from {0} to {1}.",
                        previousNumberOfCardsInPlay,
                        currentNumberOfCards);
                }
            }

            if (g.CurrentTurn >= 1)
            {
                int previousNumberOfLeadersInPlay = _leadercount.CountOf(g.Seed);
                int currentNumberOfLeaders = g.Players.Sum(player => player.Leaders.Where(l => l.HeroType != HeroType.Vidal).Count());
                if (previousNumberOfLeadersInPlay == 0)
                {
                    lock (_leadercount)
                    {
                        _leadercount.SetToN(g.Seed, currentNumberOfLeaders);
                    }
                }
                else if (currentNumberOfLeaders != previousNumberOfLeadersInPlay)
                {
                    return string.Format("Total number of leaders has changed from {0} to {1}.",
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


            if (g.Players.Any(p => p.Leaders.Any(l => l.HeroType != HeroType.Vidal && l.Faction != p.Faction && p.Faction != Faction.Purple && !g.CapturedLeaders.ContainsKey(l))))
            {
                return "Lost Leader";
            }

            if (g.CurrentPhase == Phase.BeginningOfCollection)
            {
                var terr = g.OccupyingForcesOnPlanet.Where(kvp => kvp.Key != g.Map.PolarSink && !g.IsInStorm(kvp.Key.Territory) && kvp.Value.Count(b => b.Faction != Faction.Pink) > 1).Select(kvp => kvp.Key).FirstOrDefault();
                if (terr != null)
                {
                    return $"{terr} occupied by more than one faction";
                }
            }

            if (g.CurrentBattle != null)
            {
                var aggressor = g.CurrentBattle.AggressivePlayer;
                var defender = g.CurrentBattle.DefendingPlayer;
                if (aggressor == null || defender == null) return "Battle without aggressor or defender";
                if (aggressor.AlliedPlayer == defender) return "Battle between allies";
            }

            var pink = g.GetPlayer(Faction.Pink);
            if (pink != null)
            {
                int actualAmount = g.AmbassadorsOnPlanet.Count(kvp => kvp.Value != Ambassador.Blue) + g.UnassignedAmbassadors.Items.Count(a => a != Ambassador.Blue) + g.AmbassadorsSetAside.Count(a => a != Ambassador.Blue) + pink.Ambassadors.Count(a => a != Ambassador.Blue);
                int expectedAmount = EstablishPlayers.AvailableFactions().Count(f => f != Faction.Blue) - 1;

                if (actualAmount != expectedAmount)
                    return $"Invalid number of ambassadors: {actualAmount} != {EstablishPlayers.AvailableFactions().Count() - 1}";
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
                    var game = new Game(state.Version);
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
                                DetermineWins(Environment.ProcessorCount * 3, rulesAsArray, nrOfPlayers, nrOfTurns, pDict, toTest, out int wins, out int spice, out int points, out int forcesOnPlanet);

                                File.AppendAllLines("results.csv", new string[] { string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                                                        battle_MimimumChanceToAssumeEnemyHeroSurvives, battle_MimimumChanceToAssumeMyLeaderSurvives, Battle_MaxStrengthOfDialledForces, Battle_DialShortageThresholdForThrowing,
                                                        wins, spice, points, forcesOnPlanet, toTest) });
                            }
                        }
                    }
                }
            }
        }

        private void DetermineWins(int nrOfGames, Rule[] rules, int nrOfPlayers, int nrOfTurns, Dictionary<Faction, BotParameters> p, Faction f,
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
                    var game = LetBotsPlay(rules, nrOfPlayers, nrOfTurns, p, false, false, null, 30, f);
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
            int nrOfTurns = 12;
            int nrOfPlayers = 7;

            int timeout = 10;

            Console.WriteLine("Winner;Method;Turn;Events;Leaders killed;Forces killed;Owned cards;Owned Spice;Discarded");

            //Expansion, advanced game, all expansions, all factions:
            var rules = Game.RulesetDefinition[Ruleset.AllExpansionsAdvancedGame].ToList();
            rules.Add(Rule.FillWithBots);
            rules.Add(Rule.AssistedNotekeeping);
            rules.Add(Rule.DisableOrangeSpecialVictory);

            var rulesAsArray = rules.ToArray();
            var wincounter = new ObjectCounter<Faction>();

            ParallelOptions po = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.For(0, nrOfGames, po,
                   index =>
                   {
                       PlayGameAndRecordResults(nrOfPlayers, nrOfTurns, rulesAsArray, wincounter, statistics, timeout);
                   });

            foreach (var f in wincounter.Counted.OrderByDescending(f => wincounter.CountOf(f)))
            {
                Console.WriteLine(Skin.Current.Format("{0}: {1} ({2}%)", f, wincounter.CountOf(f), (100f * wincounter.CountOf(f) / nrOfGames)));
            }

            statistics?.Output(Skin.Current);
        }

        private void PlayGameAndRecordResults(int nrOfPlayers, int nrOfTurns, Rule[] rulesAsArray, ObjectCounter<Faction> wincounter, Statistics statistics, int timeout)
        {
            var game = LetBotsPlay(rulesAsArray, nrOfPlayers, nrOfTurns, null, false, true, statistics, timeout);

            Console.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                string.Join(",", game.Winners.Select(p => DetermineName(p))),
                Skin.Current.Describe(game.WinMethod),
                game.CurrentTurn,
                game.History.Count,
                Skin.Current.Join(LeaderManager.Leaders.Where(l => !game.IsAlive(l))),
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
        private Game LetBotsPlay(Rule[] rules, int nrOfPlayers, int nrOfTurns, Dictionary<Faction, BotParameters> p, bool infoLogging, bool performTests, Statistics statistics, int timeout, Faction mustPlay = Faction.None)
        {
            BattleOutcome previousBattleOutcome = null;

            var game = new Game()
            {
                BotInfologging = infoLogging,
            };

            var timer = new TimedTest(game, timeout);
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
                var start = new EstablishPlayers(game, Faction.None)
                {
                    ApplicableRules = rules.ToArray(),
                    FactionsInPlay = factions,
                    MaximumTurns = nrOfTurns,
                    MaximumNumberOfPlayers = nrOfPlayers,
                    Players = Array.Empty<string>(),
                    Seed = new Random().Next(),
                    Time = DateTime.Now
                };
                start.Execute(false, true);

                if (p != null)
                {
                    foreach (var kvp in p)
                    {
                        game.Players.Single(p => p.Faction == kvp.Key).Param = kvp.Value;
                    }
                }

                int maxNumberOfEvents = game.CurrentTurn * game.Players.Count * 60;

                while (game.CurrentPhase != Phase.GameEnded)
                {
                    var evt = PerformBotEvent(game, performTests);

                    if (evt == null)
                    {
                        File.WriteAllText("novalidbotevent" + game.Seed + ".json", GameState.GetStateAsString(game));
                    }
                    Assert.IsNotNull(evt, "bots couldn't come up with a valid event");

                    evt.Time = DateTime.Now;


                    if (game.History.Count == maxNumberOfEvents)
                    {
                        File.WriteAllText("stuck" + game.Seed + ".json", GameState.GetStateAsString(game));
                    }
                    Assert.AreNotEqual(maxNumberOfEvents, game.History.Count, "bots got stuck");

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
            File.WriteAllText("timeout" + game.Seed + ".json", GameState.GetStateAsString(game));
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

            if (performTests)
            {
                Assert.IsNull(executeResult, msg);
            }
        }

        [TestMethod]
        public void Regression()
        {
            var statistics = new Statistics();
            var centralStyleStatistics = new ConcurrentBag<string>();

            File.WriteAllText("CentralStyleStatistics.json", "{\r\n  \"entries\": [");

            Message.DefaultDescriber = Skin.Current;

            ProfileGames();

            _cardcount = new();
            _leadercount = new();

            try
            {
                Console.WriteLine("Re-playing all savegame files in {0}...", Directory.GetCurrentDirectory());

                int gamesTested = 0;
                ParallelOptions po = new()
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount//1
                };
                Parallel.ForEach(Directory.EnumerateFiles(".", "savegame*.json"), po, fileName =>
                {
                    gamesTested++;
                    var testcaseFileName = fileName + ".testcase";

                    try
                    {
                        ReplayGame(fileName, testcaseFileName, statistics, centralStyleStatistics);
                    }
                    catch (Exception)
                    {
                        //Console.WriteLine("Errors encountered in " + fileName);
                        //File.Delete(fileName);
                        //File.Delete(testcaseFileName);
                        throw;
                    }
                });

                Assert.AreNotEqual(0, gamesTested);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            
            statistics?.Output(Skin.Current);
            
            bool firstLine = true;
            foreach (var item in centralStyleStatistics)
            {
                if (!firstLine)
                {
                    File.AppendAllText("CentralStyleStatistics.json", ",\r\n");
                    firstLine = false;
                }

                File.AppendAllText("CentralStyleStatistics.json", item);
            }

            File.AppendAllText("CentralStyleStatistics.json", "]\r\n}");
        }

        private void ReplayGame(string fileName, string testcaseFileName, Statistics statistics, ConcurrentBag<string> centralStyleStatistics)
        {
            var fs = File.OpenText(fileName);
            var state = GameState.Load(fs.ReadToEnd());
            fs.Close();
            Console.WriteLine("Checking {0} (version {1})...", fileName, state.Version);
            var game = new Game(state.Version);

            fs = File.OpenText(testcaseFileName);
            var tc = LoadObject<Testcase>(fs.ReadToEnd());
            fs.Close();

            int valueId = 0;

            BattleOutcome previousBattleOutcome = null;
            bool gatherStatistics = false;

            foreach (var evt in state.Events)
            {
                evt.Initialize(game);
                var previousPhase = game.CurrentPhase;

                var result = evt.Execute(true, true);
                if (result != null)
                {
                    File.WriteAllText("invalid" + game.Seed + ".json", GameState.GetStateAsString(game));
                }
                Assert.IsNull(result, fileName + ", " + evt.GetType().Name + " (" + valueId + ", " + evt.GetMessage() + "): " + result?.ToString());

                var actualValues = DetermineTestvalues(game);
                tc.Testvalues[valueId].Equals(actualValues);
                if (!tc.Testvalues[valueId].Equals(actualValues))
                {
                    File.WriteAllText("invalid" + game.Seed + ".json", GameState.GetStateAsString(game));
                }
                Assert.AreEqual(tc.Testvalues[valueId], actualValues, fileName + ", " + previousPhase + " - " + game.CurrentPhase + ", " + evt.GetType().Name + " (" + valueId + ", " + evt.GetMessage() + "): " + Testvalues.Difference);

                var strangeCase = TestIllegalCases(game, evt);
                if (strangeCase != "")
                {
                    File.WriteAllText("illegalcase_" + game.EventCount + "_" + strangeCase + ".json", GameState.GetStateAsString(game));
                }
                Assert.AreEqual("", strangeCase, fileName + ", " + strangeCase);

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

            var centralStyleStats = GameStatistics.GetStatistics(game);
            var data = GetStatisticsAsString(centralStyleStats);
            centralStyleStatistics.Add(data);
            //var json = new StringContent(data, Encoding.UTF8, "application/json");
        }

        private static string GetStatisticsAsString(GameStatistics g)
        {
            var serializer = JsonSerializer.CreateDefault();
            serializer.TypeNameHandling = TypeNameHandling.None;
            var writer = new StringWriter();
            serializer.Serialize(writer, g);
            writer.Close();
            return writer.ToString();
        }

        private static void GatherStatistics(Statistics statistics, Game game, ref BattleOutcome previousBattleOutcome)
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
                    statistics.GamesPerMonth.Count(game.Started.ToString("yyyyMM"));

                    foreach (var p in game.Winners)
                    {
                        statistics.GameWinningPlayers.Count(DetermineName(p));
                        var fnt = new FactionAndTurn() { Faction = p.Faction, Turn = game.CurrentTurn };
                        statistics.GameWinningFactionsInTurns.Count(fnt);
                    }
                }
                else if (latest is BattleInitiated && game.CurrentBattle != null || latest is BattleClaimed)
                {
                    statistics.Battles++;
                    statistics.BattlingFactions.Count(game.CurrentBattle.Aggressor);
                    statistics.BattlingFactions.Count(game.CurrentBattle.Defender);
                }
                else if (latest is TreacheryCalled traitorcalled && traitorcalled.Succeeded)
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
                    totaldeathcount = p.Leaders.Sum(l => game.DeathCount(l)),
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
            serializer.Serialize(writer, Skin.Default);
            writer.Close();
            var skinData = writer.ToString();
            File.WriteAllText("skin.json", skinData);

            var textReader = new StringReader(File.ReadAllText("skin.json"));
            var jsonReader = new JsonTextReader(textReader);
            var skinToTest = serializer.Deserialize<Skin>(jsonReader);
            Assert.AreEqual(oldName, skinToTest.Describe(leader));
        }

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
        public void ScanForUndecoratedGetOnlyProperties()
        {
            foreach (Type type in Assembly.GetAssembly(typeof(GameEvent)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(GameEvent))))
            {
                foreach (var prop in type.GetProperties())
                {
                    if (!prop.CanWrite)
                    {
                        var att = prop.GetCustomAttribute(typeof(JsonIgnoreAttribute));
                        //if (att == null) Console.WriteLine($"Get-only property {prop} of class {type} does not have the JsonIgnore attribute");
                        Assert.IsTrue(att != null, $"Get-only property {prop} of class {type} does not have the JsonIgnore attribute");
                    }
                }
            }
        }

        [TestMethod]
        public void ScanForMapErrors()
        {
            var map = new Map();
            map.Initialize();
            bool issueFound = false;

            foreach (var l in map.Locations(false))
            {
                var asymNeighbour = l.Neighbours.FirstOrDefault(neighbour => !neighbour.Neighbours.Contains(l));
                if (asymNeighbour != null)
                {
                    issueFound = true;
                    Console.WriteLine($"Asymmetrical: {Skin.Current.Describe(l)}[{l.Id}] <-> {Skin.Current.Describe(asymNeighbour)}[{asymNeighbour.Id}]");
                }
            }

            Assert.IsFalse(issueFound, "Asymmetrical neighbour relationship detected");
        }
    }
}
