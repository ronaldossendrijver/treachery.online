using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Treachery.Shared;

namespace Treachery.Test
{
    public class Statistics
    {
        public int PlayedGames { get; set; }
        public ObjectCounter<Ruleset> GameTypes {get; } = new();
        public ObjectCounter<string> GamePlayerSetup { get; } = new();
        public ObjectCounter<string> GamePlayingPlayers {get; } = new();
        public ObjectCounter<string> GameWinningPlayers {get; } = new();
        public ObjectCounter<Faction> GamePlayingFactions {get; } = new();
        public ObjectCounter<Faction> GameWinningFactions {get; } = new();
        public ObjectCounter<WinMethod> GameWinningMethods {get; } = new();
        public List<TimeSpan> GameTimes {get; } = new();
        public ObjectCounter<int> GameNumberOfTurns {get; } = new();

        public int Battles { get; set; }
        public ObjectCounter<Faction> BattlingFactions {get; } = new();
        public ObjectCounter<Faction> BattleWinningFactions {get; } = new();
        public ObjectCounter<Faction> BattleLosingFactions {get; } = new();
        public ObjectCounter<string> BattleWinningLeaders {get; } = new();
        public ObjectCounter<string> BattleLosingLeaders {get; } = new();
        public ObjectCounter<string> BattleKilledLeaders {get; } = new();
        public ObjectCounter<string> TraitoredLeaders {get; } = new();
        public ObjectCounter<string> FacedancedLeaders {get; } = new();
        public ObjectCounter<string> UsedWeapons {get; } = new();
        public ObjectCounter<string> UsedDefenses {get; } = new();

        public ObjectCounter<Faction> FactionsOccupyingArrakeen {get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingCarthag {get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingSietchTabr {get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingHabbanyaSietch {get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingTueksSietch {get; } = new();
        public ObjectCounter<Faction> FactionsOccupyingHMS {get; } = new();
        public ObjectCounter<string> Truthtrances { get; } = new();
        public ObjectCounter<FactionAdvantage> Karamas { get; } = new();
        public ObjectCounter<string> AcceptedDeals { get; } = new();

        public void Output(IDescriber describer, int topX)
        {
            Console.WriteLine("PlayedGames:" + PlayedGames);
            Console.WriteLine("Battles:" + Battles);
            Console.WriteLine("Total Game Time (minutes):" + Math.Round(GameTimes.Sum(t => t.TotalMinutes), 0));

            OutputCounter("GameTypes", GameTypes, describer);
            OutputCounter("GamePlayerSetup", GamePlayerSetup, describer);
            OutputCounter("GamePlayingPlayers", GamePlayingPlayers, describer);
            OutputCounter("GameWinningPlayers", GameWinningPlayers, describer);
            OutputCounter("GamePlayingFactions", GamePlayingFactions, describer);
            OutputCounter("GameWinningFactions", GameWinningFactions, describer);
            OutputCounter("GameWinningMethods", GameWinningMethods, describer);
            OutputCounter("GameNumberOfTurns", GameNumberOfTurns, describer);

            OutputCounter("BattlingFactions", BattlingFactions, describer);
            OutputCounter("BattleWinningFactions", BattleWinningFactions, describer);
            OutputCounter("BattleLosingFactions", BattleLosingFactions, describer);
            OutputCounter("BattleWinningLeaders", BattleWinningLeaders, describer);
            OutputCounter("BattleLosingLeaders", BattleLosingLeaders, describer);
            OutputCounter("BattleKilledLeaders", BattleKilledLeaders, describer);
            OutputCounter("TraitoredLeaders", TraitoredLeaders, describer);
            OutputCounter("FacedancedLeaders", FacedancedLeaders, describer);
            OutputCounter("UsedWeapons", UsedWeapons, describer);
            OutputCounter("UsedDefenses", UsedDefenses, describer);

            OutputCounter("FactionsOccupyingArrakeen", FactionsOccupyingArrakeen, describer);
            OutputCounter("FactionsOccupyingCarthag", FactionsOccupyingCarthag, describer);
            OutputCounter("FactionsOccupyingSietchTabr", FactionsOccupyingSietchTabr, describer);
            OutputCounter("FactionsOccupyingHabbanyaSietch", FactionsOccupyingHabbanyaSietch, describer);
            OutputCounter("FactionsOccupyingTueksSietch", FactionsOccupyingTueksSietch, describer);
            OutputCounter("FactionsOccupyingHMS", FactionsOccupyingHMS, describer);

            OutputCounter("Truthtrances", Truthtrances, describer, 50);
            OutputCounter("Karamas", Karamas, describer);
            OutputCounter("AcceptedDeals", AcceptedDeals, describer, 50);
        }

        private void OutputCounter<T>(string title, ObjectCounter<T> c, IDescriber describer, int topX = -1)
        {
            Console.WriteLine("***" + title + "***");
            var coll = topX > 0 ? c.GetHighest(topX) : c.Counted;
            foreach (var i in coll)
            {
                Console.WriteLine(describer.Format("{0};{1}", i, c.CountOf(i)));
            }
        }
    }
}
