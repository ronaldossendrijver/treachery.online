using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Test;

public record TrainingData
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
    private Game Game { get; init; }
    
    private Player P1 { get; set; }
    private Player P2 { get; set; }
    private Player P3 { get; set; }
    private Player P4 { get; set; }
    private Player P5 { get; set; }
    private Player P6 { get; set; }
    
    public PlayerKnowledge(Game theGame, Faction faction)
    {
        Game = theGame;
        Init(faction);
    }
    
    // This player
    private PlayerInfo Player1 { get; set; }
    
    // Other players
    private PlayerInfo Player2 { get; set; }
    private PlayerInfo Player3 { get; set; }
    private PlayerInfo Player4 { get; set; }
    private PlayerInfo Player5 { get; set; }
    private PlayerInfo Player6 { get; set; }
    
    // Public board state
    public List<LocationInfo> Locations { get; } = []; 
    
    // Known info about game
    public int TreacheryCardOnBidId { get; set; }
    public Faction PredictedFaction { get; set; }
    public int PredictedTurn { get; set; }
    public bool KwizatsAvailable { get; set; }
    public int LatestAtreidesOrAllyBidAmount { get; set; }
    
    // Rules
    public bool AllyBlocksAdvisors { get; set; }
    public bool Homeworlds { get; set; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            TreacheryCardOnBidId,
            (int)PredictedFaction,
            PredictedTurn,
            B(KwizatsAvailable),
            B(AllyBlocksAdvisors),
            B(Homeworlds),
            LatestAtreidesOrAllyBidAmount,
            Player1.GetCommaSeparatedStateData(),
            Player2.GetCommaSeparatedStateData(),
            Player3.GetCommaSeparatedStateData(),
            Player4.GetCommaSeparatedStateData(),
            Player5.GetCommaSeparatedStateData(),
            Player6.GetCommaSeparatedStateData(),
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
            PlayerInfo.GetCommaSeparatedHeaders("Player1"),
            PlayerInfo.GetCommaSeparatedHeaders("Player2"),
            PlayerInfo.GetCommaSeparatedHeaders("Player3"),
            PlayerInfo.GetCommaSeparatedHeaders("Player4"),
            PlayerInfo.GetCommaSeparatedHeaders("Player5"),
            PlayerInfo.GetCommaSeparatedHeaders("Player6"),
            string.Join(";", Locations.Select(x => LocationInfo.GetCommaSeparatedHeaders(x.Id.ToString()))));
    
    private static string B(bool value) => value ? "1" : "0";
    
    private void Init(Faction faction)
    {
        P1 = Game.GetPlayer(faction);
        P2 = Game.GetPlayerBySeat((P1.Seat + 1) % Game.Players.Count);
        P3 = Game.GetPlayerBySeat((P1.Seat + 2) % Game.Players.Count);
        P4 = Game.GetPlayerBySeat((P1.Seat + 3) % Game.Players.Count);
        P5 = Game.GetPlayerBySeat((P1.Seat + 4) % Game.Players.Count);
        P6 = Game.GetPlayerBySeat((P1.Seat + 5) % Game.Players.Count);
        
        var atr = Game.GetPlayer(Faction.Green);

        Player1 = new PlayerInfo
        {
            Faction = faction,
            Ally = P1.Ally,
            Spice = P1.Resources,
            CardIds = P1.TreacheryCards.Select(x => x.Id).ToHashSet(),
            TraitorIds = P1.Traitors.Select(x => x.Id).ToHashSet(),
            FaceDancerIds = P1.FaceDancers.Select(x => x.Id).ToHashSet(),
            LivingLeaderIds = P1.Leaders.Where(Game.IsAlive).Select(x => x.Id).ToHashSet(),
            MustSupportForcesInBattle = Battle.MustPayForAnyForcesInBattle(Game, P1),
            CanUseAdvancedKarama = !Game.KarmaPrevented(faction) && !P1.SpecialKarmaPowerUsed &&
                                   Game.Applicable(Rule.AdvancedKarama),
            ForcesInReserve = P1.ForcesInReserve,
            SpecialForcesInReserve = P1.SpecialForcesInReserve,
            CanShipAndMoveThisTurn = Game.CurrentMainPhase is MainPhase.ShipmentAndMove &&
                                     !Game.HasActedOrPassed.Contains(faction),
            HasTechTokenCharity = P1.TechTokens.Contains(TechToken.Resources),
            HasTechTokenRevival = P1.TechTokens.Contains(TechToken.Graveyard),
            HasTechTokenShip = P1.TechTokens.Contains(TechToken.Ships),
        };

        Player2 = DetermineKnownPlayerInfo(P2);
        Player3 = DetermineKnownPlayerInfo(P3);
        Player4 = DetermineKnownPlayerInfo(P4);
        Player5 = DetermineKnownPlayerInfo(P5);
        Player6 = DetermineKnownPlayerInfo(P6);

        InitLocationInfo();

        Homeworlds = Game.Applicable(Rule.Homeworlds);
        AllyBlocksAdvisors = !Game.Applicable(Rule.AdvisorsDontConflictWithAlly);
        LatestAtreidesOrAllyBidAmount = Game.Bids.Values.LastOrDefault(x=> !x.Passed && (x.Initiator == Faction.Green || x.Initiator == atr?.Ally))?.TotalAmount ?? -1;
        TreacheryCardOnBidId = Game.HasBiddingPrescience(P1) && !Game.CardsOnAuction.IsEmpty? Game.CardsOnAuction.Top.Id  : -1;
        PredictedFaction = faction is Faction.Blue ? P1.PredictedFaction : Faction.None;
        PredictedTurn = faction is Faction.Blue ? P1.PredictedTurn : -1;
        KwizatsAvailable = atr?.MessiahAvailable == true;
    }
    
    private void InitLocationInfo()
    {
        foreach (var l in Game.Map.Locations(true))
        {
            Locations.Add(new LocationInfo
            {
                Id = l.Id,
                Spice = Game.ResourcesOnPlanet.GetValueOrDefault(l, 0),
                Terror = Game.TerrorIn(l.Territory).FirstOrDefault(),
                Ambassador = Game.AmbassadorIn(l.Territory),
                InStorm = Game.IsInStorm(l),
                WillSufferStormNextTurn = l.IsProtectedFromStorm 
                    ? 0 
                    : Game.HasStormPrescience(P1) 
                        ? Game.NextStormWillPassOver(l, Game.NextStormMoves) ? 1 : 0
                        : 1f / Game.DistanceFromStorm(l),
                HasWormNextTurn = Game.HasResourceDeckPrescience(P1) && !Game.ResourceCardDeck.IsEmpty && Game.ResourceCardDeck.Top.Territory == l.Territory,
            
                Player1Forces = P1.ForcesIn(l),
                Player1SpecialForces = P1.SpecialForcesIn(l),
                Player2Forces = P2?.ForcesIn(l) ?? 0,
                Player2SpecialForces = P2?.SpecialForcesIn(l) ?? 0,
                Player3Forces = P2?.ForcesIn(l) ?? 0,
                Player3SpecialForces = P3?.SpecialForcesIn(l) ?? 0,
                Player4Forces = P4?.ForcesIn(l) ?? 0,
                Player4SpecialForces = P4?.SpecialForcesIn(l) ?? 0,
                Player5Forces = P5?.ForcesIn(l) ?? 0,
                Player5SpecialForces = P2?.SpecialForcesIn(l) ?? 0,
                Player6Forces = P6?.ForcesIn(l) ?? 0,
                Player6SpecialForces = P6?.SpecialForcesIn(l) ?? 0,
            });
        }
    }

    private PlayerInfo DetermineKnownPlayerInfo(Player player)
    {
        if (player == null) 
            return PlayerInfo.Empty;
        
        return new PlayerInfo
        {
            Faction = player.Faction,
            Ally = player.Ally,
            Spice = player.Resources,
            
            CardIds = player.TreacheryCards.Where(c => P1.KnownCards.Contains(c)).Select(x => x.Id).ToHashSet(),
            TraitorIds = player.Traitors.Where(c => P1.RevealedTraitors.Contains(c)).Select(x => x.Id).ToHashSet(),
            FaceDancerIds = player.FaceDancers.Where(c => P1.RevealedDancers.Contains(c)).Select(x => x.Id).ToHashSet(),
            
            LivingLeaderIds = player.Leaders.Where(Game.IsAlive).Select(x => x.Id).ToHashSet(),
            MustSupportForcesInBattle = Battle.MustPayForAnyForcesInBattle(Game, player),
            CanUseAdvancedKarama = !Game.KarmaPrevented(player.Faction) && !player.SpecialKarmaPowerUsed && Game.Applicable(Rule.AdvancedKarama),
            ForcesInReserve = player.ForcesInReserve,
            SpecialForcesInReserve = player.SpecialForcesInReserve,
            CanShipAndMoveThisTurn = Game.CurrentMainPhase is MainPhase.ShipmentAndMove && !Game.HasActedOrPassed.Contains(player.Faction),
            
            HasTechTokenCharity = player.TechTokens.Contains(TechToken.Resources),
            HasTechTokenRevival = player.TechTokens.Contains(TechToken.Graveyard),
            HasTechTokenShip = player.TechTokens.Contains(TechToken.Ships),
        };
    }
}

public record LocationInfo
{
    public required int Id { get; init; }
    public required int Spice { get; init; }
    public required int Player1Forces { get; init; }
    public required int Player1SpecialForces { get; init; }

    public required int Player2Forces { get; init; }
    public required int Player2SpecialForces { get; init; }
    
    public required int Player3Forces { get; init; }
    public required int Player3SpecialForces { get; init; }
    
    public required int Player4Forces { get; init; }
    public required int Player4SpecialForces { get; init; }
    
    public required int Player5Forces { get; init; }
    public required int Player5SpecialForces { get; init; }
    
    public required int Player6Forces { get; init; }
    public required int Player6SpecialForces { get; init; }
    public required bool InStorm { get; init; }
    public required float WillSufferStormNextTurn { get; init; }
    public required bool HasWormNextTurn { get; init; }
    public required Ambassador? Ambassador { get; init; }
    public required TerrorType? Terror { get; init; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            Spice,
            Player1Forces,
            Player1SpecialForces,
            Player2Forces,
            Player2Forces,
            Player3Forces,
            Player3Forces,
            Player4Forces,
            Player4Forces,
            Player5Forces,
            Player5Forces,
            Player6Forces,
            Player6Forces,
            B(InStorm),
            WillSufferStormNextTurn.ToString(CultureInfo.InvariantCulture),
            B(HasWormNextTurn),
            Ambassador == null ? "-1" : (int)Ambassador,
            Terror == null ? "-1" : (int)Terror
        );
    
    public static string GetCommaSeparatedHeaders(string locId) =>
        string.Join(";",
            "Loc" + locId + nameof(Spice),
            "Loc" + locId + nameof(Player1Forces),
            "Loc" + locId + nameof(Player1SpecialForces),
            "Loc" + locId + nameof(Player2Forces),
            "Loc" + locId + nameof(Player2SpecialForces),
            "Loc" + locId + nameof(Player3Forces),
            "Loc" + locId + nameof(Player3SpecialForces),
            "Loc" + locId + nameof(Player4Forces),
            "Loc" + locId + nameof(Player4SpecialForces),
            "Loc" + locId + nameof(Player5Forces),
            "Loc" + locId + nameof(Player5SpecialForces),
            "Loc" + locId + nameof(Player6Forces),
            "Loc" + locId + nameof(Player6SpecialForces),
            "Loc" + locId + nameof(InStorm),
            "Loc" + locId + nameof(WillSufferStormNextTurn),
            "Loc" + locId + nameof(HasWormNextTurn),
            "Loc" + locId + nameof(Ambassador),
            "Loc" + locId + nameof(Terror)
        );
    
    private static string B(bool value) => value ? "1" : "0";
}

public record PlayerInfo
{
    public required Faction Faction { get; set; }
    public required Faction Ally { get; set; }
    public required int Spice { get; set; }
    
    /*
    public required bool HasProjectile;
    public required bool HasShield;
    public required bool HasPoison;
    public required bool HasSnooper;
    public required bool HasWorthless;
    public required
    */
    
    public required HashSet<int> CardIds { get; set; } = [];
    public required HashSet<int> TraitorIds { get; set; } = [];
    public required HashSet<int> FaceDancerIds { get; set; } = [];
    public required HashSet<int> LivingLeaderIds { get; set; } = [];
    public required bool HasTechTokenCharity { get; set; }
    public required bool HasTechTokenShip { get; set; }
    public required bool HasTechTokenRevival { get; set; }

    public required bool MustSupportForcesInBattle { get; set; } 
    public required bool CanUseAdvancedKarama { get; set; }
    public required int ForcesInReserve { get; set; }
    public required int SpecialForcesInReserve { get; set; }
    public required bool CanShipAndMoveThisTurn { get; set; }

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
            who + nameof(HasTechTokenCharity),
            who + nameof(HasTechTokenRevival),
            who + nameof(HasTechTokenShip),
            HeaderSet($"{who}Card", 1, 59),
            HeaderSet($"{who}Traitor", 1, 62),
            HeaderSet($"{who}Fd", 1, 62),
            HeaderSet($"{who}Leader", 1, 62));

    private static string B(bool value) => value ? "1" : "0";

    private static string Set(int min, int max, HashSet<int> values) => string.Join(';',
        Enumerable.Range(min, max).Select(x => values.Contains(x) ? "1" : "0"));
    
    private static string HeaderSet(string header, int min, int max) => string.Join(';',
        Enumerable.Range(min, max).Select(x => $"{header}{x}"));
    
    public static readonly PlayerInfo Empty = new()
    {
        Faction = Faction.None,
        Ally = Faction.None,
        Spice = 0,
        CardIds = [],
        TraitorIds = [],
        FaceDancerIds = [],
        LivingLeaderIds = [],
        HasTechTokenCharity = false,
        HasTechTokenShip = false,
        HasTechTokenRevival = false,
        MustSupportForcesInBattle = false,
        CanUseAdvancedKarama = false,
        ForcesInReserve = 0,
        SpecialForcesInReserve = 0,
        CanShipAndMoveThisTurn = false
    };
}

public class DecisionInfo(int gameId, PlayerKnowledge state, GameEvent decision)
{
    public int GameId { get; } = gameId;
    public PlayerKnowledge State { get; } = state;
    public GameEvent Decision { get; } = decision;
}