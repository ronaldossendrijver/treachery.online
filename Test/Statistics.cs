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

            OutputCounter("GameTypes", GameTypes, describer, topX);
            OutputCounter("GamePlayerSetup", GamePlayerSetup, describer, topX);
            OutputCounter("GamePlayingPlayers", GamePlayingPlayers, describer, topX);
            OutputCounter("GameWinningPlayers", GameWinningPlayers, describer, topX);
            OutputCounter("GamePlayingFactions", GamePlayingFactions, describer, topX);
            OutputCounter("GameWinningFactions", GameWinningFactions, describer, topX);
            OutputCounter("GameWinningMethods", GameWinningMethods, describer, topX);
            OutputCounter("GameNumberOfTurns", GameNumberOfTurns, describer, topX);

            OutputCounter("BattlingFactions", BattlingFactions, describer, topX);
            OutputCounter("BattleWinningFactions", BattleWinningFactions, describer, topX);
            OutputCounter("BattleLosingFactions", BattleLosingFactions, describer, topX);
            OutputCounter("BattleWinningLeaders", BattleWinningLeaders, describer, topX);
            OutputCounter("BattleLosingLeaders", BattleLosingLeaders, describer, topX);
            OutputCounter("BattleKilledLeaders", BattleKilledLeaders, describer, topX);
            OutputCounter("TraitoredLeaders", TraitoredLeaders, describer, topX);
            OutputCounter("FacedancedLeaders", FacedancedLeaders, describer, topX);
            OutputCounter("UsedWeapons", UsedWeapons, describer, topX);
            OutputCounter("UsedDefenses", UsedDefenses, describer, topX);

            OutputCounter("FactionsOccupyingArrakeen", FactionsOccupyingArrakeen, describer, topX);
            OutputCounter("FactionsOccupyingCarthag", FactionsOccupyingCarthag, describer, topX);
            OutputCounter("FactionsOccupyingSietchTabr", FactionsOccupyingSietchTabr, describer, topX);
            OutputCounter("FactionsOccupyingHabbanyaSietch", FactionsOccupyingHabbanyaSietch, describer, topX);
            OutputCounter("FactionsOccupyingTueksSietch", FactionsOccupyingTueksSietch, describer, topX);
            OutputCounter("FactionsOccupyingHMS", FactionsOccupyingHMS, describer, topX);

            OutputCounter("Truthtrances", Truthtrances, describer, topX);
            OutputCounter("Karamas", Karamas, describer, topX);
            OutputCounter("AcceptedDeals", AcceptedDeals, describer, topX);
        }

        private void OutputCounter<T>(string title, ObjectCounter<T> c, IDescriber describer, int topX)
        {
            Console.WriteLine("***" + title + "***");
            foreach (var i in c.GetHighest(topX))
            {
                Console.WriteLine(describer.Format("{0};{1}", i, c.CountOf(i)));
            }
        }
    }
}
