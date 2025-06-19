using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Treachery.Shared;

namespace Treachery.Test;

public class TrainingData
{
    public List<DecisionInfo> Decisions { get; set; } = [];
    
    public void Output()
    {
        var shipmentFile = new StreamWriter(File.Open("shipments.csv", FileMode.Create), Encoding.ASCII, 10000);
        var shipmentFileHeaderWritten = false;
        
        var moveFile = new StreamWriter(File.Open("move.csv", FileMode.Create));
        var moveFileHeaderWritten = false;

        var battleFile = new StreamWriter(File.Open("battle.csv", FileMode.Create));
        var battleFileHeaderWritten = false;

        var bidFile = new StreamWriter(File.Open("bid.csv", FileMode.Create));
        var bidFileHeaderWritten = false;

        foreach (var decision in Decisions)
        {
            var state = decision.State.GetCommaSeparatedStateData();
            
            switch (decision.Decision)
            {
                case Shipment shipment:
                    if (!shipmentFileHeaderWritten) shipmentFile.WriteLine($"{decision.State.GetCommaSeparatedHeaders()};ShipmentToId;ShipmentForces;ShipmentSpecialForces");
                    shipmentFile.WriteLine($"{state};{shipment.To.Id};{shipment.ForceAmount};{shipment.SpecialForceAmount}");
                    shipmentFileHeaderWritten = true;
                    break;
                
                case Move move:
                    //moveFile.WriteLine($"{state};{move.To.Id};");
                    break;
                
                case Battle battle:
                    //battleFile.WriteLine($"{state};{battle.Forces};");
                    break;
                
                case Bid bid:
                    //bidFile.WriteLine($"{state};{bid.Amount};{bid.AllyContributionAmount};{bid.RedContributionAmount};");
                    break;
            }
        }
    }

    public void DeleteDecisionsByLosers(int gameId, Faction[] winners)
    {
        Decisions.RemoveAll(d => d.GameId == gameId && !winners.Contains(d.Decision.Initiator));
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
    public List<LocationInfo> Locations { get; set; } = []; 
    
    // Known info about game
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
            TreacheryCardOnBidId,
            (int)PredictedFaction,
            PredictedTurn,
            B(KwizatsAvailable),
            B(AllyBlocksAdvisors),
            B(Homeworlds),
            LatestAtreidesOrAllyBidAmount,
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
            string.Join(";",Locations.Select(x => x.GetCommaSeparatedStateData())));
    
    public string GetCommaSeparatedHeaders() =>
        string.Join(";",
            nameof(TreacheryCardOnBidId),
            nameof(PredictedFaction),
            nameof(PredictedTurn),
            nameof(KwizatsAvailable),
            nameof(AllyBlocksAdvisors),
            nameof(Homeworlds),
            nameof(LatestAtreidesOrAllyBidAmount),
            PlayerInfo.GetCommaSeparatedHeaders("My"),
            PlayerInfo.GetCommaSeparatedHeaders("Ally"),
            PlayerInfo.GetCommaSeparatedHeaders("Frm"),
            PlayerInfo.GetCommaSeparatedHeaders("Atr"),
            PlayerInfo.GetCommaSeparatedHeaders("Hark"),
            PlayerInfo.GetCommaSeparatedHeaders("Emp"),
            PlayerInfo.GetCommaSeparatedHeaders("Gld"),
            PlayerInfo.GetCommaSeparatedHeaders("Bg"),
            PlayerInfo.GetCommaSeparatedHeaders("Ix"),
            PlayerInfo.GetCommaSeparatedHeaders("Bt"),
            PlayerInfo.GetCommaSeparatedHeaders("Chm"),
            PlayerInfo.GetCommaSeparatedHeaders("Rich"),
            PlayerInfo.GetCommaSeparatedHeaders("Eca"),
            PlayerInfo.GetCommaSeparatedHeaders("Mor"),
            string.Join(";", Locations.Select(x => LocationInfo.GetCommaSeparatedHeaders(x.Id.ToString()))));
    
    private static string B(bool value) => value ? "1" : "0";
}

public class LocationInfo
{
    public int Id { get; set; }
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
    
    public static string GetCommaSeparatedHeaders(string locId) =>
        string.Join(";",
            nameof(Spice) + locId,
            nameof(MyForces) + locId,
            nameof(MySpecialForces) + locId,
            nameof(AllyForces) + locId,
            nameof(AllySpecialForces) + locId,
            nameof(YellowOpponentForces) + locId,
            nameof(GreenOpponentForces) + locId,
            nameof(BlackOpponentForces) + locId,
            nameof(RedOpponentForces) + locId,
            nameof(OrangeOpponentForces) + locId,
            nameof(BlueOpponentForces) + locId,
            nameof(GreyOpponentForces) + locId,
            nameof(PurpleOpponentForces) + locId,
            nameof(BrownOpponentForces) + locId,
            nameof(WhiteOpponentForces) + locId,
            nameof(PinkOpponentForces) + locId,
            nameof(CyanOpponentForces) + locId,
            nameof(YellowOpponentSpecialForces) + locId,
            nameof(GreenOpponentSpecialForces) + locId,
            nameof(BlackOpponentSpecialForces) + locId,
            nameof(RedOpponentSpecialForces) + locId,
            nameof(OrangeOpponentSpecialForces) + locId,
            nameof(BlueOpponentSpecialForces) + locId,
            nameof(GreyOpponentSpecialForces) + locId,
            nameof(PurpleOpponentSpecialForces) + locId,
            nameof(BrownOpponentSpecialForces) + locId,
            nameof(WhiteOpponentSpecialForces) + locId,
            nameof(PinkOpponentSpecialForces) + locId,
            nameof(CyanOpponentSpecialForces) + locId,
            nameof(InStorm) + locId,
            nameof(ProtectedFromStorm) + locId,
            nameof(SuffersStormNextTurn) + locId,
            nameof(HasWormNextTurn) + locId,
            nameof(Ambassador) + locId,
            nameof(Terror) + locId
        );
    
    private static string B(bool value) => value ? "1" : "0";
}

public class PlayerInfo
{
    public Faction Faction { get; set; }
    public Faction Ally { get; set; }
    public int Spice { get; set; }
    public HashSet<int> CardIds { get; set; } = [];
    public HashSet<int> TraitorIds { get; set; } = [];
    public HashSet<int> FaceDancerIds { get; set; } = [];
    public HashSet<int> LivingLeaderIds { get; set; } = [];
    public bool HasTechTokenCharity { get; set; }
    public bool HasTechTokenShip { get; set; }
    public bool HasTechTokenRevival { get; set; }

    public bool MustSupportForcesInBattle { get; set; } 
    public bool MustSupportSpecialForcesInBattle { get; set; }
    public bool CanUseAdvancedKarama { get; set; }
    public int ForcesInReserve { get; set; }
    public int SpecialForcesInReserve { get; set; }
    public bool CanShipAndMoveThisTurn { get; set; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            (int)Faction,
            (int)Ally,
            Spice,
            B(CanShipAndMoveThisTurn),
            B(CanUseAdvancedKarama),
            ForcesInReserve,
            SpecialForcesInReserve,
            B(MustSupportForcesInBattle),
            B(MustSupportSpecialForcesInBattle),
            B(HasTechTokenCharity),
            B(HasTechTokenRevival),
            B(HasTechTokenShip),
            Set(1, 59, CardIds),
            Set(1, 62, TraitorIds),
            Set(1, 62, FaceDancerIds),
            Set(1, 62, LivingLeaderIds));
    
    public static string GetCommaSeparatedHeaders(string who) =>
        string.Join(";",
            who + nameof(Faction),
            who + nameof(Ally),
            who + nameof(Spice),
            who + nameof(CanShipAndMoveThisTurn),
            who + nameof(CanUseAdvancedKarama),
            who + nameof(ForcesInReserve),
            who + nameof(SpecialForcesInReserve),
            who + nameof(MustSupportForcesInBattle),
            who + nameof(MustSupportSpecialForcesInBattle),
            who + nameof(HasTechTokenCharity),
            who + nameof(HasTechTokenRevival),
            who + nameof(HasTechTokenShip),
            who + HeaderSet("Card", 1, 59),
            who + HeaderSet("Traitor", 1, 62),
            who + HeaderSet("Fd", 1, 62),
            who + HeaderSet("Leader", 1, 62));

    private static string B(bool value) => value ? "1" : "0";

    private static string Set(int min, int max, HashSet<int> values) => string.Join(';',
        Enumerable.Range(min, max).Select(x => values.Contains(x) ? "1" : "0"));
    
    private static string HeaderSet(string header, int min, int max) => string.Join(';',
        Enumerable.Range(min, max).Select(x => $"{header}{x}"));
}

public class DecisionInfo(int gameId, PlayerKnowledge state, GameEvent decision)
{
    public int GameId { get; set; } = gameId;
    public PlayerKnowledge State { get; set; } = state;
    public GameEvent Decision { get; set; } = decision;
}


