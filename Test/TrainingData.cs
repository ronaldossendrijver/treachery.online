using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Treachery.Shared;
using Treachery.Shared.Model;

namespace Treachery.Test;

public class TrainingData : IDisposable, IAsyncDisposable
{
    private ConcurrentDictionary<int,ConcurrentBag<DecisionInfo>> Decisions { get; } = [];

    private Lock FileLock { get; } = new();
    
    private StreamWriter LogFile { get; } = new(File.Open("training.csv", FileMode.Create), Encoding.ASCII, 100000);

    private Dictionary<Type, StreamWriter> Files { get; } = new()
    {
        { typeof(Shipment), new StreamWriter(File.Open($"{nameof(Shipment)}.csv", FileMode.Create), Encoding.ASCII, 100000) },
        { typeof(Move), new StreamWriter(File.Open($"{nameof(Move)}.csv", FileMode.Create), Encoding.ASCII, 100000) },
        { typeof(Battle), new StreamWriter(File.Open($"{nameof(Battle)}.csv", FileMode.Create), Encoding.ASCII, 100000) },
        { typeof(Bid), new StreamWriter(File.Open($"{nameof(Bid)}.csv", FileMode.Create), Encoding.ASCII, 100000) }
    };
    
    public TrainingData()
    {
        foreach (var fileForType in Files)
            fileForType.Value.WriteLine(GetHeaderCsv(fileForType.Key));
    }
    
    public void RegisterDecision(Game game)
    {
        var latest = game.LatestEvent();
       
        if (latest is Shipment or Move or Battle or Bid)
        {
            if (game.Players.Count is < 4 or > 6) return;
            
            if (!Decisions.TryGetValue(game.Seed, out var decisions))
            {
                lock (LogFile)
                {
                    LogFile.WriteLine($"{DateTime.Now.ToLongTimeString()};Start;{game.Seed};{Decisions.Count}");
                }
                
                decisions = [];
                Decisions.TryAdd(game.Seed, decisions);
            } 
                
            decisions.Add(new DecisionInfo(latest.GetType(), latest.Initiator, new PlayerKnowledge(game, latest.Initiator).GetCommaSeparatedStateData(), GetDateCsv(latest)));
        }
    }

    private static string GetDateCsv(GameEvent latest) => latest switch
    {
        Shipment shipment => $"{shipment.To.Id};{shipment.ForceAmount};{shipment.SpecialForceAmount}",
        _ => string.Empty
    };
    
    private static string GetHeaderCsv(Type type) 
    {
        if (type == typeof(Shipment)) return $"{PlayerKnowledge.GetCommaSeparatedHeaders()};ShipmentToId;ShipmentForces;ShipmentSpecialForces";
        if (type == typeof(Move)) return $"{PlayerKnowledge.GetCommaSeparatedHeaders()};MoveToId;MoveForces;MoveSpecialForces";
        if (type == typeof(Battle)) return $"{PlayerKnowledge.GetCommaSeparatedHeaders()};BattleLeader";
        if (type == typeof(Bid)) return $"{PlayerKnowledge.GetCommaSeparatedHeaders()};BidAmount";

        return string.Empty;
    }

    public void WriteTrainingDataIfGameEnded(Game game)
    {
        if (game.CurrentPhase != Phase.GameEnded) return;
        
        if (!Decisions.TryGetValue(game.Seed, out var decisionsForGame)) return;
        
        if (game.Players.Count is >= 4 and <= 6)
            Output(decisionsForGame.Where(d => game.Winners.Any(x => x.Faction == d.Faction)).ToArray());

        lock (LogFile)
        {
            LogFile.WriteLine($"{DateTime.Now.ToLongTimeString()};Finish;{game.Seed};{Decisions.Count}");  
        }
        
        Decisions.TryRemove(game.Seed, out _);
    }

    private void Output(DecisionInfo[] decisions)
    {
        lock (FileLock)
        {
            foreach (var decision in decisions)
            {
                Files[decision.Type].WriteLine($"{decision.State};{decision.Decision}");
            }
        }
    }

    public void Dispose()
    {
        foreach (var file in Files.Values)
            file.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var file in Files.Values)
            await file.DisposeAsync();
    }
}

public class PlayerKnowledge
{
    public const int MaxPlayers = 6;
    private Game Game { get; }
    private Faction MyFaction { get; }

    // Player[0]: this player, Player[1..5]: other players
    
    private Player[] Player { get; } = new Player[MaxPlayers];
    private PlayerInfo[] PlayerInfo { get; } = new PlayerInfo[MaxPlayers];

    private Player Me => Player[0];

    private int NrOfPlayers => Game.Players.Count;
    
    public PlayerKnowledge(Game game, Faction faction)
    {
        Game = game;
        MyFaction = faction;
        Init();
    }
    
    // Public board state
    private List<LocationInfo> Locations { get; } = []; 
    
    // Known info about game
    public int CurrentTurn { get; set; }
    public int MaximumTurns { get; set; }
    public int TreacheryCardOnBidId { get; set; }
    public Faction PredictedFaction { get; set; }
    public int PredictedTurn { get; set; }
    public bool KwizatsAvailable { get; set; }
    public int LatestAtreidesOrAllyBidAmount { get; set; }
    private int[] CardIds { get; } = new int[59];
    private int[] TraitorIds { get; } = new int[63];
    private int[] LivingLeaderIds { get; } = new int[62];

    // Rules
    public bool AllyBlocksAdvisors { get; set; }
    public bool Homeworlds { get; set; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            CurrentTurn,
            MaximumTurns,
            NrOfPlayers,
            TreacheryCardOnBidId,
            (int)PredictedFaction,
            PredictedTurn,
            B(KwizatsAvailable),
            B(AllyBlocksAdvisors),
            B(Homeworlds),
            LatestAtreidesOrAllyBidAmount,
            Set(CardIds),
            Set(TraitorIds),
            Set(LivingLeaderIds),
            string.Join(";", PlayerInfo.Select(p => p.GetCommaSeparatedStateData())),
            string.Join(";", Locations.Select(x => x.GetCommaSeparatedStateData())));
    
    public static string GetCommaSeparatedHeaders() =>
        string.Join(";",
            nameof(CurrentTurn),
            nameof(MaximumTurns),
            nameof(NrOfPlayers),
            nameof(TreacheryCardOnBidId),
            nameof(PredictedFaction),
            nameof(PredictedTurn),
            nameof(KwizatsAvailable),
            nameof(AllyBlocksAdvisors),
            nameof(Homeworlds),
            nameof(LatestAtreidesOrAllyBidAmount),
            HeaderSet("TreacheryCard", 1, 59),
            HeaderSet("TraitorCard", 0, 63),
            HeaderSet("Leader", 1, 62),
            string.Join(";", Enumerable.Range(1, MaxPlayers).Select(x => Test.PlayerInfo.GetCommaSeparatedHeaders($"Player{x}"))),
            string.Join(";", Enumerable.Range(0, 105).Select(x => LocationInfo.GetCommaSeparatedHeaders(x.ToString()))));
    
    private static string B(bool value) => value ? "1" : "0";
    
    private static string Set(int[] values) => string.Join(';', values);
    
    private static string HeaderSet(string header, int min, int max) => string.Join(';',
        Enumerable.Range(min, max).Select(x => $"{header}{x}"));
    
    private void Init()
    {
        InitPlayers();
        InitPlayerInfo();
        InitCardAndLeaderTokenInfo();
        InitLocationInfo();
        InitGeneralInfo();
    }

    private void InitGeneralInfo()
    {
        var atr = Game.GetPlayer(Faction.Green);
        Homeworlds = Game.Applicable(Rule.Homeworlds);
        AllyBlocksAdvisors = !Game.Applicable(Rule.AdvisorsDontConflictWithAlly);
        LatestAtreidesOrAllyBidAmount = Game.Bids.Values.LastOrDefault(x=> !x.Passed && (x.Initiator == Faction.Green || x.Initiator == atr?.Ally))?.TotalAmount ?? -1;
        TreacheryCardOnBidId = Game.HasBiddingPrescience(Me) && !Game.CardsOnAuction.IsEmpty? Game.CardsOnAuction.Top.Id  : -1;
        MaximumTurns = Game.MaximumTurns;
        CurrentTurn = Game.CurrentTurn;
        PredictedFaction = Me.PredictedFaction;
        PredictedTurn = Me.PredictedTurn;
        KwizatsAvailable = atr?.MessiahAvailable == true;
    }

    private void InitPlayerInfo()
    {
        // Me
        
        PlayerInfo[0] = new PlayerInfo
        {
            Faction = MyFaction,
            Ally = Me.Ally,
            Spice = Me.Resources,
            MustSupportForcesInBattle = Battle.MustPayForAnyForcesInBattle(Game, Me),
            CanUseAdvancedKarama = !Game.KarmaPrevented(MyFaction) && !Me.SpecialKarmaPowerUsed &&
                                   Game.Applicable(Rule.AdvancedKarama),
            ForcesInReserve = Me.ForcesInReserve,
            SpecialForcesInReserve = Me.SpecialForcesInReserve,
            CanShipAndMoveThisTurn = Game.CurrentMainPhase is MainPhase.ShipmentAndMove &&
                                     !Game.HasActedOrPassed.Contains(MyFaction),
            HasTechTokenCharity = Me.TechTokens.Contains(TechToken.Resources),
            HasTechTokenRevival = Me.TechTokens.Contains(TechToken.Graveyard),
            HasTechTokenShip = Me.TechTokens.Contains(TechToken.Ships),
        };
        
        // Others
        
        for (var i = 1; i < MaxPlayers; i++)
            PlayerInfo[i] = i < NrOfPlayers ? DetermineKnownPlayerInfo(Player[i]) : Test.PlayerInfo.Empty;
        
        //if (PlayerInfo.Count(x => x.Faction == Faction.Green) > 1)
            //Console.WriteLine("Hee!");
    }

    private void InitPlayers()
    {
        //if (Game.Seed == 19736931)
        //    Console.WriteLine("Hee!");
        
        // Me
        Player[0] = Game.GetPlayer(MyFaction);
        
        // Others
        
        var addedPlayers = 1;
        var distance = 1;
        while (addedPlayers < NrOfPlayers)
        {
            var playerInSeat = Game.GetPlayerBySeat((Me.Seat + distance++) % Game.MaximumPlayers);
            if (playerInSeat != null)
                Player[addedPlayers++] = playerInSeat; 
        }
    }

    private void InitCardAndLeaderTokenInfo()
    {
        for (var i = 0; i < NrOfPlayers; i++)
            foreach (var c in Player[i].TreacheryCards.Where(x => i == 0 || Me.KnownCards.Contains(x)))
                if (c.Id < CardIds.Length)
                    CardIds[c.Id] = i + 1;

        for (var i = 0; i < NrOfPlayers; i++)
        {
            foreach (var c in Player[i].Traitors.Where(x => i == 0 || Player[i].RevealedTraitors.Contains(x) || Me.ToldTraitors.Contains(x)))
                TraitorIds[c is TreacheryCard ? 0 : c.Id - LeaderManager.FirstId] = i + 1;
            
            foreach (var c in Player[i].FaceDancers.Where(x => i == 0 || Player[i].RevealedFaceDancers.Contains(x) || Me.ToldFaceDancers.Contains(x)))
                TraitorIds[c is TreacheryCard ? 0 : c.Id - LeaderManager.FirstId] = i + 1;
        }
        
        for (var i = 0; i < NrOfPlayers; i++)
            foreach (var c in Player[i].Leaders)
                LivingLeaderIds[c.Id - LeaderManager.FirstId - 1] = (Game.IsAlive(c) ? 1 : -1) * (i + 1);
    }

    private void InitLocationInfo()
    {
        foreach (var l in Game.Map.Locations(true))
        {
            var li = new LocationInfo
            {
                Spice = Game.ResourcesOnPlanet.GetValueOrDefault(l, 0),
                Terror = Game.TerrorIn(l.Territory).FirstOrDefault(),
                Ambassador = Game.AmbassadorIn(l.Territory),
                InStorm = Game.IsInStorm(l),
                WillSufferStormNextTurn = l.IsProtectedFromStorm
                    ? 0
                    : Game.HasStormPrescience(Me)
                        ? Game.NextStormWillPassOver(l, Game.NextStormMoves) ? 1 : 0
                        : Game.DistanceFromStorm(l) <= 6
                            ? 0.5f
                            : 0,
                HasWormNextTurn = Game.HasResourceDeckPrescience(Me) && !Game.ResourceCardDeck.IsEmpty &&
                                  Game.ResourceCardDeck.Top.Territory == l.Territory,
            };

            for (var i = 0; i < NrOfPlayers; i++)
            {
                li.PlayerForces[i] = Player[i].ForcesIn(l);
                li.PlayerSpecialForces[i] = Player[i].SpecialForcesIn(l);
            }

            Locations.Add(li);
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
    public required int Spice { get; init; }
    public int[] PlayerForces { get; } = new int[PlayerKnowledge.MaxPlayers]; 
    public int[] PlayerSpecialForces { get; } = new int[PlayerKnowledge.MaxPlayers];
    public required bool InStorm { get; init; }
    public required float WillSufferStormNextTurn { get; init; }
    public required bool HasWormNextTurn { get; init; }
    public required Ambassador? Ambassador { get; init; }
    public required TerrorType? Terror { get; init; }

    public string GetCommaSeparatedStateData() =>
        string.Join(";",
            Spice,
            string.Join(";", PlayerForces.Zip(PlayerSpecialForces, (normal, special) => $"{normal};{special}")),
            B(InStorm),
            WillSufferStormNextTurn.ToString(CultureInfo.InvariantCulture),
            B(HasWormNextTurn),
            Ambassador == null ? "-1" : (int)Ambassador,
            Terror == null ? "-1" : (int)Terror
        );
    
    public static string GetCommaSeparatedHeaders(string locId) =>
        string.Join(";",
            "Loc" + locId + nameof(Spice),
            string.Join(";", Enumerable.Range(0, PlayerKnowledge.MaxPlayers).Select(x => $"Loc{locId}Player{x}Forces;Loc{locId}Player{x}SpecialForces")),
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

public class DecisionInfo(Type type, Faction faction, string state, string decision)
{
    public Type Type { get; } = type;
    public Faction Faction { get; } = faction;
    public string State { get; } = state;
    public string Decision { get; } = decision;
}