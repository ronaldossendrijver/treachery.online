using System;

namespace Treachery.Shared
{
    #pragma warning disable IDE1006 // Naming Styles

    public class GameStatistics
    {
        public DateTime date { get; set; }

        public float time { get; set; }

        public int turn { get; set; }

        public string ruleset { get; set; }

        public GameStatisticsPlayerInfo[] players { get; set; }

        public string[] winners { get; set; }

        public string method { get; set; }
    }

    public class GameStatisticsPlayerInfo
    {
        public string faction { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    #pragma warning restore IDE1006 // Naming Styles
    
}
