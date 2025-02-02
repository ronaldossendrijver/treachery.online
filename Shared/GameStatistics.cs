using System;

namespace Treachery.Shared;
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

    public static GameStatistics GetStatistics(Game game)
    {
        var result = new GameStatistics
        {
            date = game.LastAction,
            method = ToString(game.WinMethod),
            players = game.Players.Select(p => new GameStatisticsPlayerInfo
            {
                name = p.Name,
                faction = ToString(p.Faction),
                id = ""
            }).ToArray(),
            ruleset = ToString(game.Ruleset),
            time = RoundToHalves((game.LastAction - game.FirstAction).TotalHours),
            turn = game.CurrentTurn,
            winners = game.Winners.Select(p => ToString(p.Faction)).ToArray()
        };

        return result;
    }

    private static float RoundToHalves(double value)
    {
        return (float)Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2;
    }

    private static string ToString(WinMethod m)
    {
        return m switch
        {
            WinMethod.Strongholds => "STRONGHOLDS",
            WinMethod.Prediction => "PREDICTION",
            WinMethod.Timeout => "TIMEOUT",
            WinMethod.Forfeit => "FORFEIT",
            WinMethod.YellowSpecial => "FREMEN_DEFAULT",
            WinMethod.OrangeSpecial => "GUILD_DEFAULT",
            _ => "None"
        };
    }

    private static string ToString(Faction f)
    {
        return f switch
        {
            Faction.Yellow => "FREMEN",
            Faction.Green => "ATREIDES",
            Faction.Black => "HARKONNEN",
            Faction.Red => "EMPEROR",
            Faction.Orange => "GUILD",
            Faction.Blue => "BG",
            Faction.Grey => "IX",
            Faction.Purple => "TLEILAXU",
            Faction.White => "RICHESE",
            Faction.Brown => "CHOAM",
            _ => "None"
        };
    }

    private static string ToString(Ruleset rules)
    {
        return rules switch
        {
            Ruleset.AdvancedGame or Ruleset.ExpansionAdvancedGame => "GF9 advanced",
            Ruleset.BasicGame or Ruleset.ExpansionBasicGame => "GF9 basic",
            _ => "None"
        };
    }
}

public class GameStatisticsPlayerInfo
{
    public string faction { get; set; }
    public string name { get; set; }
    public string id { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles