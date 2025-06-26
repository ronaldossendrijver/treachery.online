using System;
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

    // Player[0]: this player, Player[1..5]: other players
    
    private Player[] Player { get; set; } = new Player[6];
    private PlayerInfo[] PlayerInfo { get; set; } = new PlayerInfo[6];

    private Player Me => Player[0];

    private int NrOfPlayers => Game.Players.Count;
    
    public PlayerKnowledge(Game theGame, Faction faction)
    {
        Game = theGame;
        Init(faction);
    }
    
    // Public board state
    public List<LocationInfo> Locations { get; } = []; 
    
    // Known info about game
    public int TreacheryCardOnBidId { get; set; }
    public Faction PredictedFaction { get; set; }
    public int PredictedTurn { get; set; }
    public bool KwizatsAvailable { get; set; }
    public int LatestAtreidesOrAllyBidAmount { get; set; }
    private int[] CardIds { get; set; } = new int[59];
    private int[] TraitorIds { get; set; } = new int[1061];
    private int[] LivingLeaderIds { get; set; } = new int[1061];

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
            Set(1, 59, CardIds),
            Set(1, 62, TraitorIds),
            Set(1, 62, LivingLeaderIds),
            PlayerInfo[0].GetCommaSeparatedStateData(),
            PlayerInfo[1].GetCommaSeparatedStateData(),
            PlayerInfo[2].GetCommaSeparatedStateData(),
            PlayerInfo[3].GetCommaSeparatedStateData(),
            PlayerInfo[4].GetCommaSeparatedStateData(),
            PlayerInfo[5].GetCommaSeparatedStateData(),
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
            HeaderSet("TreacheryCard", 1, 59),
            HeaderSet("TraitorCard", 1, 62),
            HeaderSet("Leader", 1, 62),
            Test.PlayerInfo.GetCommaSeparatedHeaders("Player1"),
            Test.PlayerInfo.GetCommaSeparatedHeaders("Player2"),
            Test.PlayerInfo.GetCommaSeparatedHeaders("Player3"),
            Test.PlayerInfo.GetCommaSeparatedHeaders("Player4"),
            Test.PlayerInfo.GetCommaSeparatedHeaders("Player5"),
            Test.PlayerInfo.GetCommaSeparatedHeaders("Player6"),
            string.Join(";", Locations.Select(x => LocationInfo.GetCommaSeparatedHeaders(x.Id.ToString()))));
    
    private static string B(bool value) => value ? "1" : "0";
    
    private static string Set(int min, int max, int[] values) => string.Join(';',
        Enumerable.Range(min, max).Select(x => values.Contains(x) ? "1" : "0"));
    
    private static string HeaderSet(string header, int min, int max) => string.Join(';',
        Enumerable.Range(min, max).Select(x => $"{header}{x}"));
    
    private void Init(Faction faction)
    {
        Player[0] = Game.GetPlayer(faction);
        
        var addedPlayers = 1;
        var distance = 1;
        while (addedPlayers < NrOfPlayers)
        {
            var playerInSeat = Game.GetPlayerBySeat((Me.Seat + distance++) % NrOfPlayers);
            if (playerInSeat != null)
                Player[addedPlayers++] = playerInSeat; 
        }
            
        var atr = Game.GetPlayer(Faction.Green);

        PlayerInfo[0] = new PlayerInfo
        {
            Faction = faction,
            Ally = Me.Ally,
            Spice = Me.Resources,
            MustSupportForcesInBattle = Battle.MustPayForAnyForcesInBattle(Game, Me),
            CanUseAdvancedKarama = !Game.KarmaPrevented(faction) && !Me.SpecialKarmaPowerUsed &&
                                   Game.Applicable(Rule.AdvancedKarama),
            ForcesInReserve = Me.ForcesInReserve,
            SpecialForcesInReserve = Me.SpecialForcesInReserve,
            CanShipAndMoveThisTurn = Game.CurrentMainPhase is MainPhase.ShipmentAndMove &&
                                     !Game.HasActedOrPassed.Contains(faction),
            HasTechTokenCharity = Me.TechTokens.Contains(TechToken.Resources),
            HasTechTokenRevival = Me.TechTokens.Contains(TechToken.Graveyard),
            HasTechTokenShip = Me.TechTokens.Contains(TechToken.Ships),
        };

        for (var i = 1; i < NrOfPlayers; i++)
            PlayerInfo[i] = DetermineKnownPlayerInfo(Player[i]);

        InitCardAndLeaderTokenInfo();
        InitLocationInfo();

        Homeworlds = Game.Applicable(Rule.Homeworlds);
        AllyBlocksAdvisors = !Game.Applicable(Rule.AdvisorsDontConflictWithAlly);
        LatestAtreidesOrAllyBidAmount = Game.Bids.Values.LastOrDefault(x=> !x.Passed && (x.Initiator == Faction.Green || x.Initiator == atr?.Ally))?.TotalAmount ?? -1;
        TreacheryCardOnBidId = Game.HasBiddingPrescience(Me) && !Game.CardsOnAuction.IsEmpty? Game.CardsOnAuction.Top.Id  : -1;
        PredictedFaction = Me.PredictedFaction;
        PredictedTurn = Me.PredictedTurn;
        KwizatsAvailable = atr?.MessiahAvailable == true;
    }

    private void InitCardAndLeaderTokenInfo()
    {
        for (var i = 0; i < NrOfPlayers; i++)
            foreach (var c in Player[i].TreacheryCards.Where(x => i == 0 || Me.KnownCards.Contains(x)))
                if (c.Id < CardIds.Length)
                    CardIds[c.Id] = i;

        for (var i = 0; i < NrOfPlayers; i++)
        {
            foreach (var c in Player[i].Traitors.Where(x => i == 0 || Me.RevealedTraitors.Contains(x) || Me.ToldTraitors.Contains(x)))
                TraitorIds[c is TreacheryCard ? 0 : c.Id - LeaderManager.FirstId] = i;
            
            foreach (var c in Player[i].FaceDancers.Where(x => i == 0 || Me.RevealedFaceDancers.Contains(x) || Me.ToldFaceDancers.Contains(x)))
                TraitorIds[c is TreacheryCard ? 0 : c.Id - LeaderManager.FirstId] = i;
        }
        
        for (var i = 0; i < NrOfPlayers; i++)
            foreach (var c in Player[i].Leaders)
                LivingLeaderIds[c.Id - LeaderManager.FirstId] = (Game.IsAlive(c) ? 1 : -1) * i;
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
                    : Game.HasStormPrescience(Me) 
                        ? Game.NextStormWillPassOver(l, Game.NextStormMoves) ? 1 : 0
                        : 1f - (float)Math.Min(0.167 * Game.DistanceFromStorm(l), 1),
                HasWormNextTurn = Game.HasResourceDeckPrescience(Me) && !Game.ResourceCardDeck.IsEmpty && Game.ResourceCardDeck.Top.Territory == l.Territory,
            
                Player1Forces = Player[0].ForcesIn(l),
                Player1SpecialForces = Player[0].SpecialForcesIn(l),
                Player2Forces = Player[1]?.ForcesIn(l) ?? 0,
                Player2SpecialForces = Player[1]?.SpecialForcesIn(l) ?? 0,
                Player3Forces = Player[2]?.ForcesIn(l) ?? 0,
                Player3SpecialForces = Player[2]?.SpecialForcesIn(l) ?? 0,
                Player4Forces = Player[3]?.ForcesIn(l) ?? 0,
                Player4SpecialForces = Player[3]?.SpecialForcesIn(l) ?? 0,
                Player5Forces = Player[4]?.ForcesIn(l) ?? 0,
                Player5SpecialForces = Player[4]?.SpecialForcesIn(l) ?? 0,
                Player6Forces = Player[5]?.ForcesIn(l) ?? 0,
                Player6SpecialForces = Player[5]?.SpecialForcesIn(l) ?? 0,
            });
        }
    }

    private PlayerInfo DetermineKnownPlayerInfo(Player player)
    {
        if (player == null) 
            return Test.PlayerInfo.Empty;
        
        return new PlayerInfo
        {
            Faction = player.Faction,
            Ally = player.Ally,
            Spice = player.Resources,

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
    public required Faction Faction { get; init; }
    public required Faction Ally { get; init; }
    public required int Spice { get; init; }
    
    public required bool HasTechTokenCharity { get; init; }
    public required bool HasTechTokenShip { get; init; }
    public required bool HasTechTokenRevival { get; init; }

    public required bool MustSupportForcesInBattle { get; init; } 
    public required bool CanUseAdvancedKarama { get; init; }
    public required int ForcesInReserve { get; init; }
    public required int SpecialForcesInReserve { get; init; }
    public required bool CanShipAndMoveThisTurn { get; init; }

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
            B(HasTechTokenShip));
    
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
            who + nameof(HasTechTokenShip));

    private static string B(bool value) => value ? "1" : "0";
    
    public static readonly PlayerInfo Empty = new()
    {
        Faction = Faction.None,
        Ally = Faction.None,
        Spice = 0,
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