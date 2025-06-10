using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Treachery.Shared;

namespace Treachery.Test;

public class TrainingData
{
    public Dictionary<int,Faction[]> Winners { get; set; }
    
    public List<Tuple<int,PlayerKnowledge,GameEvent>> Decisions { get; set; }
}

public class PlayerKnowledge
{
    // Player
    public PlayerInfo I { get; } = new PlayerInfo();
        
    // Known info about other players    
    public List<PlayerInfo> Opponents { get; } = [];
    
    // Public board state
    public Dictionary<int,LocationInfo> Locations { get; set; } 
    
    // Known info about game
    public int NextStormMoves { get; set; }
    public int NextSpiceCardId { get; set; }
    public int NextTreacheryCardId { get; set; }
    public Faction PredictedFaction { get; set; }
    public int PredictedTurn { get; set; }
    public int KwizatsCounter { get; set; }
    
    // Rules
    public bool AllyBlocksAdvisors { get; set; }
    public bool Homeworlds { get; set; }
}

public class LocationInfo
{
    public int Spice { get; set; }
    public int MyForces { get; set; }
    public int MySpecialForces { get; set; }
    public int ForcesPlayer2 { get; set; }
    public int SpecialForcesPlayer2 { get; set; }
    public int ForcesPlayer3 { get; set; }
    public int SpecialForcesPlayer3 { get; set; }
    public int ForcesPlayer4 { get; set; }
    public int SpecialForcesPlayer4 { get; set; }
    public int ForcesPlayer5 { get; set; }
    public int SpecialForcesPlayer5 { get; set; }
    public int ForcesPlayer6 { get; set; }
    public int SpecialForcesPlayer6 { get; set; }
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
}
