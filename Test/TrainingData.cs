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
    
    public List<Tuple<int,PlayerKnowledge,GameEvent>> Decisions { get; set; }
    
    public void Output()
    {
        var shipmentFile = File.Open("shipments.csv", FileMode.OpenOrCreate);
        
        foreach (var decision in Decisions)
        {
            var winners = Winners.GetValueOrDefault(decision.Item1, []);
            if (!winners.Contains(decision.Item3.Initiator)) continue; // only train on decisions by winners

            var state = WriteState();
            
            switch (decision.Item3)
            {
                case Shipment shipment:
                
                    break;
                
                default: // left blank

                    break;
            }
        }
    }

    private string WriteState()
    {
        return string.Empty;
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
    public Dictionary<int,LocationInfo> Locations { get; set; } 
    
    // Known info about game
    public int NextStormMoves { get; set; }
    public int NextSpiceCardId { get; set; }
    public int TreacheryCardOnBidId { get; set; }
    public Faction PredictedFaction { get; set; }
    public int PredictedTurn { get; set; }
    public bool KwizatsAvailable { get; set; }
    
    // Rules
    public bool AllyBlocksAdvisors { get; set; }
    public bool Homeworlds { get; set; }
    public int LatestAtreidesOrAllyBidAmount { get; set; }
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
    public int SuffersStormNextTurn { get; set; }
    public bool HasWormNextTurn { get; set; }
    public Ambassador? Ambassador { get; set; }
    public TerrorType? Terror { get; set; }

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
}
