using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Treachery.Shared;

namespace Treachery.Test;

public class TrainingData
{
    public Dictionary<int,Faction[]> Winners { get; set; }
    
    public List<DecisionInfo> Decisions { get; set; }
    
    public void Output()
    {
        var shipmentFile = new StreamWriter(File.Open("shipments.csv", FileMode.OpenOrCreate));
        var moveFile = new StreamWriter(File.Open("move.csv", FileMode.OpenOrCreate));
        var battleFile = new StreamWriter(File.Open("battle.csv", FileMode.OpenOrCreate));
        var bidFile = new StreamWriter(File.Open("bid.csv", FileMode.OpenOrCreate));

        foreach (var decision in Decisions)
        {
            var winners = Winners.GetValueOrDefault(decision.GameId, []);
            if (!winners.Contains(decision.Decision.Initiator)) continue; // only train on decisions by winners

            var state = decision.State.GetCommaSeparatedStateData();
            
            switch (decision.Decision)
            {
                case Shipment shipment:
                    shipmentFile.WriteLine($"{state};{shipment.To.Id};{shipment.ForceAmount};{shipment.SpecialForceAmount}");
                    break;
                
                case Move move:
                    moveFile.WriteLine($"{state};{move.To.Id};");
                    break;
                
                case Battle battle:
                    battleFile.WriteLine($"{state};{battle.Forces};");
                    break;
                
                case Bid bid:
                    bidFile.WriteLine($"{state};{bid.Amount};{bid.AllyContributionAmount};{bid.RedContributionAmount};");
                    break;
                
                default: // left blank

                    break;
            }
        }
    }


    
}

public class PlayerKnowledge
{
    // Player
    public PlayerInfo I { get; set; } = new();

    // Ally
    public PlayerInfo Ally { get; set; } = new();
    
    // Opponents
    
    public PlayerInfo YellowOpponent { get; set; }
    public PlayerInfo GreenOpponent { get; set; }
    public PlayerInfo BlackOpponent { get; set; }
    public PlayerInfo RedOpponent { get; set; }
    public PlayerInfo OrangeOpponent { get; set; }
    public PlayerInfo BlueOpponent { get; set; }
    public PlayerInfo GreyOpponent { get; set; }
    public PlayerInfo PurpleOpponent { get; set; }
    public PlayerInfo BrownOpponent { get; set; }
    public PlayerInfo WhiteOpponent { get; set; }
    public PlayerInfo PinkOpponent { get; set; }
    public PlayerInfo CyanOpponent { get; set; }
    
    // Public board state
    public List<LocationInfo> Locations { get; set; } 
    
    // Known info about game
    public int NextSpiceCardId { get; set; }
    public int TreacheryCardOnBidId { get; set; }
    public Faction PredictedFaction { get; set; }
    public int PredictedTurn { get; set; }
    public bool KwizatsAvailable { get; set; }
    
    // Rules
    public bool AllyBlocksAdvisors { get; set; }
    public bool Homeworlds { get; set; }
    public int LatestAtreidesOrAllyBidAmount { get; set; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            I.GetCommaSeparatedStateData(),
            Ally.GetCommaSeparatedStateData(),
            YellowOpponent.GetCommaSeparatedStateData(),
            GreenOpponent.GetCommaSeparatedStateData(),
            BlackOpponent.GetCommaSeparatedStateData(),
            RedOpponent.GetCommaSeparatedStateData(),
            OrangeOpponent.GetCommaSeparatedStateData(),
            BlueOpponent.GetCommaSeparatedStateData(),
            GreyOpponent.GetCommaSeparatedStateData(),
            PurpleOpponent.GetCommaSeparatedStateData(),
            BrownOpponent.GetCommaSeparatedStateData(),
            WhiteOpponent.GetCommaSeparatedStateData(),
            PinkOpponent.GetCommaSeparatedStateData(),
            CyanOpponent.GetCommaSeparatedStateData(),
    NextSpiceCardId,
    TreacheryCardOnBidId,
    (int)PredictedFaction,
    PredictedTurn,
     B(KwizatsAvailable),
     B(AllyBlocksAdvisors),
     B(Homeworlds),
    LatestAtreidesOrAllyBidAmount,
            
            Locations.Select(x => x.GetCommaSeparatedStateData()));
    
    private static string B(bool value) => value ? "1" : "0";
}

public class LocationInfo
{
    public int Spice { get; set; }
    public int MyForces { get; set; }
    public int MySpecialForces { get; set; }
    public int AllyForces { get; set; }
    public int AllySpecialForces { get; set; }
    public int YellowOpponentForces { get; set; }
    public int GreenOpponentForces { get; set; }
    public int BlackOpponentForces { get; set; }
    public int RedOpponentForces { get; set; }
    public int OrangeOpponentForces { get; set; }
    public int BlueOpponentForces { get; set; }
    public int GreyOpponentForces { get; set; }
    public int PurpleOpponentForces { get; set; }
    public int BrownOpponentForces { get; set; }
    public int WhiteOpponentForces { get; set; }
    public int PinkOpponentForces { get; set; }
    public int CyanOpponentForces { get; set; }
    public int YellowOpponentSpecialForces { get; set; }
    public int GreenOpponentSpecialForces { get; set; }
    public int BlackOpponentSpecialForces { get; set; }
    public int RedOpponentSpecialForces { get; set; }
    public int OrangeOpponentSpecialForces { get; set; }
    public int BlueOpponentSpecialForces { get; set; }
    public int GreyOpponentSpecialForces { get; set; }
    public int PurpleOpponentSpecialForces { get; set; }
    public int BrownOpponentSpecialForces { get; set; }
    public int WhiteOpponentSpecialForces { get; set; }
    public int PinkOpponentSpecialForces { get; set; }
    public int CyanOpponentSpecialForces { get; set; }
    public bool InStorm { get; set; }
    public bool ProtectedFromStorm { get; set; }
    public bool SuffersStormNextTurn { get; set; }
    public bool HasWormNextTurn { get; set; }
    public Ambassador? Ambassador { get; set; }
    public TerrorType? Terror { get; set; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            Spice,
            MyForces,
            MySpecialForces,
            AllyForces,
            AllySpecialForces,
            YellowOpponentForces,
            GreenOpponentForces,
            BlackOpponentForces,
            RedOpponentForces,
            OrangeOpponentForces,
            BlueOpponentForces,
            GreyOpponentForces,
            PurpleOpponentForces,
            BrownOpponentForces,
            WhiteOpponentForces,
            PinkOpponentForces,
            CyanOpponentForces,
            YellowOpponentSpecialForces,
            GreenOpponentSpecialForces,
            BlackOpponentSpecialForces,
            RedOpponentSpecialForces,
            OrangeOpponentSpecialForces,
            BlueOpponentSpecialForces,
            GreyOpponentSpecialForces,
            PurpleOpponentSpecialForces,
            BrownOpponentSpecialForces,
            WhiteOpponentSpecialForces,
            PinkOpponentSpecialForces,
            CyanOpponentSpecialForces,
            B(InStorm),
            B(ProtectedFromStorm),
            B(SuffersStormNextTurn),
            B(HasWormNextTurn),
            Ambassador == null ? "-1" : (int)Ambassador,
            Terror == null ? "-1" : (int)Terror
        );
    
    private static string B(bool value) => value ? "1" : "0";
}

public class PlayerInfo
{
    public Faction Faction { get; set; }
    public Faction Ally { get; set; }
    public int Spice { get; set; }
    public HashSet<int> CardIds { get; set; } 
    public HashSet<int> TraitorIds { get; set; }
    public HashSet<int> FaceDancerIds { get; set; }
    public HashSet<int> LivingLeaderIds { get; set; }
    public HashSet<int> DeadLeaderIds { get; set; }
    public bool MustSupportForcesInBattle { get; set; } 
    public bool MustSupportSpecialForcesInBattle { get; set; }
    public bool CanUseAdvancedKarama { get; set; }
    public int ForcesOnHomeworld1 { get; set; }
    public int SpecialForcesOnHomeworld1 { get; set; }
    public int ForcesOnHomeworld2 { get; set; }
    public int SpecialForcesOnHomeworld2 { get; set; }
    public bool CanShipAndMoveThisTurn { get; set; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            (int)Faction,
            (int)Ally,
            Spice,
            B(CanShipAndMoveThisTurn),
            B(CanUseAdvancedKarama),
            ForcesOnHomeworld1,
            ForcesOnHomeworld2,
            SpecialForcesOnHomeworld1,
            SpecialForcesOnHomeworld2,
            B(MustSupportForcesInBattle),
            B(MustSupportSpecialForcesInBattle),
            Set(1, 59, CardIds),
            Set(1, 62, TraitorIds),
            Set(1, 62, FaceDancerIds),
            Set(1, 62, LivingLeaderIds),
            Set(1, 62, DeadLeaderIds));

    private static string B(bool value) => value ? "1" : "0";

    private static string Set(int min, int max, HashSet<int> values) => string.Join(';',
        Enumerable.Range(min, max).Select(x => values.Contains(x) ? "1" : "0"));
    
    
}

public class DecisionInfo(int gameId, PlayerKnowledge state, GameEvent decision)
{
    public int GameId { get; set; } = gameId;
    public PlayerKnowledge State { get; set; } = state;
    public GameEvent Decision { get; set; } = decision;
}


