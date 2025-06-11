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
