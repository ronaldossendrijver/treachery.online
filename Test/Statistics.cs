using System;
using System.Collections.Generic;
using System.Linq;

using Treachery.Shared;

namespace Treachery.Test
{
    public class Statistics
    {
        public int PlayedGames { get; set; }
        public ObjectCounter<Ruleset> GameTypes { get; } = new();
        public ObjectCounter<string> GamePlayerSetup { get; } = new();
        public ObjectCounter<string> GamePlayingPlayers { get; } = new();
        public ObjectCounter<string> GameWinningPlayers { get; } = new();
        public ObjectCounter<Faction> GamePlayingFactions { get; } = new();
        public ObjectCounter<FactionAndTurn> GameWinningFactionsInTurns { get; } = new();
        public ObjectCounter<WinMethod> GameWinningMethods { get; } = new();
        public List<TimeSpan> GameTimes { get; } = new();
        public ObjectCounter<int> GameNumberOfTurns { get; } = new();

        public ObjectCounter<string> GamesPerMonth = new();

        public int Battles { get; set; }
        public ObjectCounter<Faction> BattlingFactions { get; } = new();
        public ObjectCounter<Faction> BattleWinningFactions { get; } = new();
        public ObjectCounter<Faction> BattleLosingFactions { get; } = new();
        public ObjectCounter<string> BattleWinningLeaders { get; } = new();
        public ObjectCounter<string> BattleLosingLeaders { get; } = new();
        public ObjectCounter<string> BattleKilledLeaders { get; } = new();
        public ObjectCounter<string> TraitoredLeaders { get; } = new();
        public ObjectCounter<string> FacedancedLeaders { get; } = new();
        public ObjectCounter<string> UsedWeapons { get; } = new();
        public ObjectCounter<string> UsedDefenses { get; } = new();

        public ObjectCounter<Faction> FactionsOccupyingArrakeen { get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingCarthag { get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingSietchTabr { get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingHabbanyaSietch { get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingTueksSietch { get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingHMS { get; } = new();
        public ObjectCounter<string> Truthtrances { get; } = new();
        public ObjectCounter<FactionAdvantage> Karamas { get; } = new();
        public ObjectCounter<string> AcceptedDeals { get; } = new();

        public ObjectCounter<Faction> GameWinningFactions
        {
            get
            {
                var result = new ObjectCounter<Faction>();

                foreach (var fit in GameWinningFactionsInTurns.Counted)
                {
                    result.CountN(fit.Faction, GameWinningFactionsInTurns.CountOf(fit));
                }

                return result;
            }
        }

        public ObjectCounter<int> GameWinningTurns
        {
            get
            {
                var result = new ObjectCounter<int>();

                foreach (var fit in GameWinningFactionsInTurns.Counted)
                {
                    result.CountN(fit.Turn, GameWinningFactionsInTurns.CountOf(fit));
                }

                return result;
            }
        }

        public void Output(IDescriber describer)
        {
            Console.WriteLine("PlayedGames:" + PlayedGames);
            Console.WriteLine("Battles:" + Battles);
            Console.WriteLine("Total Game Time (minutes):" + Math.Round(GameTimes.Sum(t => t.TotalMinutes), 0));

            OutputCounter("GamesPerMonth", GamesPerMonth, describer);
            OutputCounter("GameTypes", GameTypes, describer);
            OutputCounter("GamePlayerSetup", GamePlayerSetup, describer);
            OutputCounter("GameNumberOfTurns", GameNumberOfTurns, describer);
            OutputCounter("GameWinningMethods", GameWinningMethods, describer);

            OutputCounterGrouped("Players",
                GamePlayingPlayers.Counted,
                new ObjectCounter<string>[] { GamePlayingPlayers, GameWinningPlayers },
                new string[] { "Played", "Won" },
                describer);

            OutputCounterGrouped("Factions",
                GamePlayingFactions.Counted,
                new ObjectCounter<Faction>[] { GamePlayingFactions, GameWinningFactions, BattlingFactions, BattleWinningFactions, BattleLosingFactions, FactionsOccupyingArrakeen, FactionsOccupyingCarthag, FactionsOccupyingSietchTabr, FactionsOccupyingHabbanyaSietch, FactionsOccupyingTueksSietch, FactionsOccupyingHMS },
                new string[] { "Played", "Won", "Battles", "Won", "Lost", "Arrakeen", "Carthag", "Tabr", "Habbanya", "Tuek's", "HMS" },
                describer);

            OutputCounterGrouped("Leaders",
                BattleWinningLeaders.Counted.Union(BattleLosingLeaders.Counted),
                new ObjectCounter<string>[] { BattleWinningLeaders, BattleLosingLeaders, BattleKilledLeaders, TraitoredLeaders, FacedancedLeaders },
                new string[] { "Won", "Lost", "Killed", "Traitor", "Facedancer" },
                describer);

            OutputCounter("UsedWeapons", UsedWeapons, describer);
            OutputCounter("UsedDefenses", UsedDefenses, describer);

            OutputCounter("Truthtrances", Truthtrances, describer, 50);
            OutputCounter("Karamas", Karamas, describer);
            OutputCounter("AcceptedDeals", AcceptedDeals, describer, 50);

        }

        private static void OutputCounter<T>(string title, ObjectCounter<T> c, IDescriber describer, int topX = -1)
        {
            Console.WriteLine("***" + title + "***");
            var coll = topX > 0 ? c.GetHighest(topX) : c.Counted;
            foreach (var i in coll)
            {
                Console.WriteLine(describer.Format("{0};{1}", i, c.CountOf(i)));
            }
        }

        private static void OutputCounterGrouped<T>(string title, IEnumerable<T> groupedBy, IEnumerable<ObjectCounter<T>> counters, IEnumerable<string> counterLabels, IDescriber describer)
        {
            if (counters.Count() != counterLabels.Count()) throw new ArgumentException("Number of counters does not equal number of counter labels");

            //Header
            Console.WriteLine("***" + title + "***");

            Console.Write(typeof(T).Name);
            foreach (var label in counterLabels)
            {
                Console.Write(";{0}", label);
            }
            Console.WriteLine();

            //Body

            foreach (var group in groupedBy)
            {
                Console.Write(describer.Describe(group));
                foreach (var counter in counters)
                {
                    Console.Write(describer.Format(";{0}", counter.CountOf(group)));
                }

                Console.WriteLine();
            }
        }

    }

    public class FactionAndTurn
    {
        public Faction Faction;
        public int Turn;

        public override string ToString()
        {
            return Message.DefaultDescriber.Format("{0};{1}", Faction, Turn);
        }
    }

}
