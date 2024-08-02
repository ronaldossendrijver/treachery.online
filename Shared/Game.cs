/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

public partial class Game
{
    #region Settings

    private const int LowestSupportedVersion = 100;
    public const int LatestVersion = 170;
    public const int ExpansionLevel = 3;
    
    #endregion Settings

    #region GameState

    public int Seed { get; internal set; } = -1;
    internal Random Random { get; set; }
    public int MaximumTurns => Settings.MaximumTurns;
    public int MaximumPlayers => Settings.MaximumPlayers;
    public string Name { get; internal set; }
    public List<Milestone> RecentMilestones { get; } = [];
    public int Version { get; }
    public Map Map { get; } = new();
    public Ruleset Ruleset { get; internal set; }
    public List<Rule> Rules { get; internal set; } = [];
    public List<Rule> RulesForBots { get; internal set; } = [];
    public List<Rule> AllRules { get; internal set; } = [];
    public List<GameEvent> History { get; } = [];
    public List<Moment> Moments { get; } = [];
    public int CurrentTurn { get; private set; }
    public MainPhase CurrentMainPhase { get; internal set; } = MainPhase.Started;
    public MainPhaseMoment CurrentMoment { get; private set; } = MainPhaseMoment.None;
    public Phase CurrentPhase { get; private set; } = Phase.None;
    public List<Faction> HasActedOrPassed { get; } = [];
    public Report CurrentReport { get; internal set; }
    public Deck<TreacheryCard> TreacheryDeck { get; internal set; }
    public Deck<TreacheryCard> TreacheryDiscardPile { get; internal set; }
    public List<TreacheryCard> RemovedTreacheryCards { get; } = [];
    public Deck<ResourceCard> ResourceCardDeck { get; internal set; }
    public Deck<ResourceCard> ResourceCardDiscardPileA { get; internal set; }
    public Deck<ResourceCard> ResourceCardDiscardPileB { get; internal set; }
    public int SectorInStorm { get; internal set; } = -1;
    public int NextStormMoves { get; internal set; } = -1;
    public bool ShieldWallDestroyed { get; internal set; }
    public Dictionary<Location, int> ResourcesOnPlanet { get; } = new();
    private Dictionary<Player, Timer<MainPhase>> Timers { get; } = new();

    public Dictionary<TreacheryCard, Faction> RecentlyDiscarded { get; } = new();
    internal Phase PhaseBeforeDiscarding { get; private set; }
    public List<Faction> FactionsThatMustDiscard { get; } = [];
    public List<Payment> RecentlyPaid { get; private set; } = [];
    internal List<Payment> StoredRecentlyPaid { get; private set; } = [];

    public Deck<LeaderSkill> SkillDeck { get; private set; }
    public List<TreacheryCard> WhiteCache { get; internal set; } = new();
    public BrownEconomicsStatus EconomicsStatus { get; internal set; } = BrownEconomicsStatus.None;

    public Dictionary<TerrorType, Territory> TerrorOnPlanet { get; private set; } = new();
    public Deck<Ambassador> UnassignedAmbassadors { get; internal set; }
    public Territory AtomicsAftermath { get; internal set; }
    public List<Ambassador> AmbassadorsSetAside { get; } = [];
    public Deck<DiscoveryToken> YellowDiscoveryTokens { get; set; }
    public Deck<DiscoveryToken> OrangeDiscoveryTokens { get; set; }
    public Dictionary<Location, Discovery> DiscoveriesOnPlanet { get; } = new();
    public Dictionary<Territory, Ambassador> AmbassadorsOnPlanet { get; private set; } = new();
    public Deck<Faction> NexusCardDeck { get; internal set; }
    public List<Faction> NexusDiscardPile { get; } = [];
    private Dictionary<Homeworld, Faction> HomeworldOccupation { get; set; } = new();
    internal Phase PhaseBeforeDiscardingTraitor { get; set; }
    internal Faction FactionThatMustDiscardTraitor { get; set; }
    internal int NumberOfTraitorsToDiscard { get; set; }

    public List<Faction> FactionsInPlay { get; internal set; }
    public List<TerrorType> UnplacedTerrorTokens { get; internal set; } = [];
    internal Deck<IHero> TraitorDeck { get; private set; }
    public Leader PinkLoyalLeader { get; private set; }

    #endregion GameState

    #region Initialization

    public Game() : this(LatestVersion)
    {
    }

    public Game(int version)
    {
        if (version < LowestSupportedVersion) throw new ArgumentException(string.Format("Game version {0} is not supported. The lowest supported version is: {1}.", version, LowestSupportedVersion));

        Version = version;

        InitializeLeaderState();
        EnterPhaseAwaitingPlayers();
    }

    private void InitializeLeaderState()
    {
        foreach (var l in LeaderManager.Leaders) LeaderState.Add(l, new LeaderState { DeathCounter = 0, CurrentTerritory = null, Skill = LeaderSkill.None, InFrontOfShield = false });

        LeaderState.Add(LeaderManager.Messiah, new LeaderState { DeathCounter = 0, CurrentTerritory = null });
    }

    #endregion Initialization

    #region EventHandling

    public void PerformPreEventTasks(GameEvent e)
    {
        UpdateTimers(e);

        if (!(e is AllyPermission || e is PlayerReplaced || e is NexusPlayed np && np.IsBetrayal && np.Faction is Faction.White))
        {
            ClearRecentPayments();

            if (!(e is DealOffered || e is DealAccepted)) RecentlyDiscarded.Clear();
        }
    }

    public void PerformPostEventTasks(GameEvent e, bool justEnteredStartOfPhase)
    {
        if (!justEnteredStartOfPhase && e is not AllyPermission && e is not DealOffered && e is not DealAccepted) MainPhaseMiddle();

        History.Add(e);
        Moments.Add(new Moment(CurrentTurn, CurrentMainPhase));
    }

    public Game Undo(int untilEventNr)
    {
        var result = new Game(Version);
        var maxEventNr = Math.Min(untilEventNr, History.Count);

        for (var i = 0; i < maxEventNr; i++)
        {
            History[i].Initialize(result);
            History[i].Execute(false, false);
        }

        return result;
    }

    private void UpdateTimers(GameEvent e)
    {
        if (e.Time != default && History.Count > 0 && InTimedPhase && e.Player != null)
        {
            GameEvent previousEvent;
            if (e is Battle) previousEvent = FindMostRecentEvent(typeof(BattleInitiated));
            else if (e is Move) previousEvent = FindMostRecentEvent(typeof(Shipment), typeof(EndPhase));
            else if (e is Shipment) previousEvent = FindMostRecentEvent(typeof(Move), typeof(EndPhase));
            else previousEvent = FindMostRecentEvent();

            var elapsedTime = e.Time.Subtract(previousEvent.Time);
            if (elapsedTime.TotalHours < 1)
            {
                if (!Timers.TryGetValue(e.Player, out var timer))
                {
                    timer = new Timer<MainPhase>();
                    Timers.Add(e.Player, timer);
                }

                timer.Add(CurrentMainPhase, elapsedTime);
            }
        }
    }

    public TimeSpan Duration => History.Count > 0 ? History[^1].Time.Subtract(History[0].Time) : TimeSpan.Zero;

    public TimeSpan TimeSpent(Player player, MainPhase phase)
    {
        if (Timers.TryGetValue(player, out var timer))
            return timer.TimeSpent(phase);
        return TimeSpan.Zero;
    }

    public DateTime Started
    {
        get
        {
            if (History.Count > 0)
                return History[0].Time;
            return default;
        }
    }

    private bool InTimedPhase =>
        CurrentPhase == Phase.Bidding ||
        CurrentPhase == Phase.OrangeShip ||
        CurrentPhase == Phase.OrangeMove ||
        CurrentPhase == Phase.NonOrangeShip ||
        CurrentPhase == Phase.NonOrangeMove ||
        CurrentPhase == Phase.BattlePhase;


    public GameEvent LatestEvent(Type eventType)
    {
        return History.LastOrDefault(e => e.GetType() == eventType);
    }

    public GameEvent LatestEvent()
    {
        return History.Count > 0 ? History[^1] : null;
    }

    public int EventCount => History.Count;

    public GameEvent FindMostRecentEvent(params Type[] types)
    {
        if (types.Length == 0) return History[^1];

        for (var i = History.Count - 1; i >= 0; i--)
            if (types.Contains(History[i].GetType())) return History[i];

        return null;
    }

    #endregion EventHandling

    #region PhaseTransitions

    internal void MainPhaseStart(MainPhase phase, bool clearReport = true)
    {
        CurrentMainPhase = phase;
        CurrentMoment = MainPhaseMoment.Start;
        if (clearReport) CurrentReport = new Report(phase);
        CurrentKarmaPrevention = null;
        CurrentJuice = null;
        BureaucratWasUsedThisPhase = false;
        BankerWasUsedThisPhase = false;
    }

    internal void MainPhaseMiddle()
    {
        if (CurrentMoment == MainPhaseMoment.Start) CurrentMoment = MainPhaseMoment.Middle;
    }

    internal void MainPhaseEnd()
    {
        CurrentMoment = MainPhaseMoment.End;

        List<FactionAdvantage> exceptionsToAllowing = new();

        if (CurrentMainPhase == MainPhase.Bidding && Prevented(FactionAdvantage.PurpleIncreasingRevivalLimits)) exceptionsToAllowing.Add(FactionAdvantage.PurpleIncreasingRevivalLimits);

        if (CurrentMainPhase == MainPhase.Resurrection && Prevented(FactionAdvantage.GreenSpiceBlowPrescience)) exceptionsToAllowing.Add(FactionAdvantage.GreenSpiceBlowPrescience);

        WasVictimOfBureaucracy = Faction.None;

        if (Version >= 103) AllowAllPreventedFactionAdvantages(exceptionsToAllowing);
    }

    internal void Enter(Phase phase, bool cleanupDeals = true)
    {
        CurrentPhase = phase;

        if (cleanupDeals) RemoveEndedDeals(phase);
    }

    internal void Enter(bool condition, Phase phaseIfTrue, Phase phaseIfFalse)
    {
        if (condition)
            Enter(phaseIfTrue);
        else
            Enter(phaseIfFalse);
    }

    internal void Enter(bool condition, Phase phaseIfTrue, Action methodOtherwise)
    {
        if (condition)
            Enter(phaseIfTrue);
        else
            methodOtherwise();
    }

    internal static void Enter(bool condition, Action methodIfTrue, Action methodOtherwise)
    {
        if (condition)
            methodIfTrue();
        else
            methodOtherwise();
    }


    internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Action methodIf2True, Action methodOtherwise)
    {
        if (condition1)
            Enter(phaseIf1True);
        else
            Enter(condition2, methodIf2True, methodOtherwise);
    }

    internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, Phase phaseOtherwise)
    {
        if (condition1)
            Enter(phaseIf1True);
        else
            Enter(condition2, phaseIf2True, phaseOtherwise);
    }

    internal void Enter(bool condition1, Action methodIf1True, bool condition2, Phase phaseIf2True, Phase phaseOtherwise)
    {
        if (condition1)
            methodIf1True();
        else
            Enter(condition2, phaseIf2True, phaseOtherwise);
    }

    internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, bool condition3, Action methodIf3True, Action methodOtherwise)
    {
        if (condition1)
            Enter(phaseIf1True);
        else if (condition2)
            Enter(phaseIf2True);
        else
            Enter(condition3, methodIf3True, methodOtherwise);
    }

    internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, bool condition3, Phase phaseIf3True, Action methodOtherwise)
    {
        if (condition1)
            Enter(phaseIf1True);
        else if (condition2)
            Enter(phaseIf2True);
        else
            Enter(condition3, phaseIf3True, methodOtherwise);
    }

    #endregion

    #region CardKnowledge

    public List<TreacheryCard> KnownCards(Player p)
    {
        var result = new List<TreacheryCard>(p.TreacheryCards);
        result.AddRange(p.KnownCards);

        if (p.HasAlly)
        {
            var ally = GetPlayer(p.Ally);
            result.AddRange(ally.TreacheryCards);
            result.AddRange(ally.KnownCards);
        }

        return result.Distinct().ToList();
    }

    internal void RegisterKnown(TreacheryCard c)
    {
        foreach (var p in Players) RegisterKnown(p, c);
    }

    internal static void RegisterKnown(Player p, TreacheryCard c)
    {
        if (c != null && !p.KnownCards.Contains(c)) p.KnownCards.Add(c);
    }

    internal void RegisterKnown(Faction f, TreacheryCard c)
    {
        var p = GetPlayer(f);
        if (p != null) RegisterKnown(p, c);
    }

    internal void UnregisterKnown(TreacheryCard c)
    {
        foreach (var p in Players) UnregisterKnown(p, c);
    }

    internal static void UnregisterKnown(Player p, TreacheryCard c)
    {
        p.KnownCards.Remove(c);
    }

    internal static void UnregisterKnown(Player p, IEnumerable<TreacheryCard> cards)
    {
        foreach (var c in cards) UnregisterKnown(p, c);
    }

    #endregion

    #region ForceInformation

    public Dictionary<Location, List<Battalion>> Forces(bool includeHomeworlds = false)
    {
        Dictionary<Location, List<Battalion>> result = new();

        foreach (var l in Map.Locations(includeHomeworlds)) result.Add(l, new List<Battalion>());

        foreach (var p in Players)
        {
            if (includeHomeworlds)
                foreach (var w in p.HomeWorlds) result.Add(w, new List<Battalion>());

            var forces = includeHomeworlds ? p.ForcesInLocations : p.ForcesOnPlanet;

            foreach (var locationAndBattaltion in forces) result[locationAndBattaltion.Key].Add(locationAndBattaltion.Value);
        }

        return result;
    }

    public Dictionary<Location, List<Battalion>> ForcesOnPlanetExcludingEmptyLocations(bool includeHomeworlds = false)
    {
        Dictionary<Location, List<Battalion>> result = new();

        foreach (var p in Players)
        {
            var forces = includeHomeworlds ? p.ForcesInLocations : p.ForcesOnPlanet;

            foreach (var locationAndBattaltion in forces)
            {
                if (!result.ContainsKey(locationAndBattaltion.Key)) result.Add(locationAndBattaltion.Key, new List<Battalion>());

                result[locationAndBattaltion.Key].Add(locationAndBattaltion.Value);
            }
        }

        return result;
    }

    public bool AnyForcesIn(Territory t)
    {
        foreach (var p in Players)
        foreach (var l in t.Locations)
            if (p.ForcesInLocations.ContainsKey(l)) return true;

        return false;
    }

    public IEnumerable<Battalion> BattalionsIn(Location l)
    {
        var result = new List<Battalion>();
        foreach (var p in Players)
            if (p.BattalionIn(l, out var batallion)) result.Add(batallion);

        return result;
    }

    public Dictionary<Location, List<Battalion>> OccupyingForcesOnPlanet
    {
        get
        {
            Dictionary<Location, List<Battalion>> result = new();

            foreach (var p in Players)
            foreach (var locationAndBattaltion in p.ForcesInLocations.Where(kvp => p.Occupies(kvp.Key)))
            {
                if (!result.ContainsKey(locationAndBattaltion.Key)) result.Add(locationAndBattaltion.Key, new List<Battalion>());

                result[locationAndBattaltion.Key].Add(locationAndBattaltion.Value);
            }

            return result;
        }
    }

    #endregion ForceInformation

    #region Resources

    internal int ResourcesIn(Location l)
    {
        return ResourcesOnPlanet.TryGetValue(l, out var value) ? value : 0;
    }

    internal void ChangeResourcesOnPlanet(Location location, int amount)
    {
        if (ResourcesOnPlanet.ContainsKey(location))
        {
            if (ResourcesOnPlanet[location] + amount == 0)
                ResourcesOnPlanet.Remove(location);
            else
                ResourcesOnPlanet[location] = ResourcesOnPlanet[location] + amount;
        }
        else if (amount != 0)
        {
            ResourcesOnPlanet.Add(location, amount);
        }
    }

    private int RemoveResources(Location location)
    {
        var result = 0;
        if (ResourcesOnPlanet.ContainsKey(location))
        {
            result = ResourcesOnPlanet[location];
            ResourcesOnPlanet.Remove(location);
        }

        return result;
    }

    private int RemoveResources(Territory territory)
    {
        var result = 0;
        foreach (var l in territory.Locations) result += RemoveResources(l);

        return result;
    }

    #endregion Resources

    #region Support

    internal void FlipBeneGesseritWhenAlone()
    {
        var bg = GetPlayer(Faction.Blue);
        if (bg != null)
        {
            var territoriesWhereAdvisorsAreInSinkOrAloneOrWithPink = Map.Territories(true).Where(t => bg.SpecialForcesIn(t) > 0 &&
                (Version >= 160 && t == Map.PolarSink.Territory || 
                 !Players.Any(p => p.Faction != Faction.Blue && p.AnyForcesIn(t) > 0)));
            
            foreach (var t in territoriesWhereAdvisorsAreInSinkOrAloneOrWithPink)
            {
                bg.FlipForces(t, false);
                Log(Faction.Blue, " are alone and flip to ", FactionForce.Blue, " in ", t);
            }
        }
    }

    private void AllowAllPreventedFactionAdvantages(List<FactionAdvantage> exceptions)
    {
        foreach (var adv in Enumerations.GetValuesExceptDefault(FactionAdvantage.None))
            if (exceptions == null || !exceptions.Contains(adv)) Allow(adv);
    }

    private void DetermineOccupationAtStartOrEndOfTurn()
    {
        var currentOccupierOfPinkHomeworld = OccupierOf(World.Pink);
        var updatedOccupation = new Dictionary<Homeworld, Faction>();

        foreach (var hw in Map.Homeworlds)
        foreach (var player in Players)
            if (player.Controls(this, hw, false) && !player.IsNative(hw))
            {
                updatedOccupation.Add(hw, player.Faction);

                if (!Occupies(player.Faction, hw.World)) Log(player.Faction, " now occupy ", hw);
            }

        foreach (var kvp in HomeworldOccupation)
            if (!updatedOccupation.Contains(kvp)) Log(kvp.Value, " no longer occupies ", kvp.Key);

        HomeworldOccupation = updatedOccupation;

        CheckIfShipmentPermissionsShouldBeRevoked();
        CheckIfOccupierTakesVidal(currentOccupierOfPinkHomeworld);
        LetFactionsDiscardSurplusCards();
    }

    internal void CheckIfShipmentPermissionsShouldBeRevoked()
    {
        if (!HasHighThreshold(Faction.Orange) && ShipmentPermissions.Any())
        {
            ShipmentPermissions.Clear();
            Log("Only ", Faction.Orange, " can ship cross/from planet now");
        }
    }

    internal void LetFactionsDiscardSurplusCards()
    {
        FactionsThatMustDiscard.AddRange(Players.Where(p => p.HandSizeExceeded).Select(p => p.Faction));
        if (FactionsThatMustDiscard.Any())
        {
            PhaseBeforeDiscarding = CurrentPhase;
            Enter(Phase.Discarding);
        }
    }

    internal void CheckIfOccupierTakesVidal(Player previousOccupierOfPinkHomeworld)
    {
        var occupierOfPinkHomeworld = OccupierOf(World.Pink);
        if (occupierOfPinkHomeworld != null)
        {
            if (!occupierOfPinkHomeworld.Leaders.Contains(Vidal)) TakeVidal(occupierOfPinkHomeworld, VidalMoment.WhilePinkWorldIsOccupied);
        }
        else if (previousOccupierOfPinkHomeworld != null && previousOccupierOfPinkHomeworld.Leaders.Contains(Vidal))
        {
            previousOccupierOfPinkHomeworld.Leaders.Remove(Vidal);
            Log(previousOccupierOfPinkHomeworld.Faction, " lose ", Vidal);
        }
    }

    private void SetAsideVidal()
    {
        if (IsAlive(Vidal) && PlayerToSetAsideVidal.Leaders.Contains(Vidal))
        {
            PlayerToSetAsideVidal.Leaders.Remove(Vidal);
            Log(Vidal, " is set aside");
        }

        PlayerToSetAsideVidal = null;
        WhenToSetAsideVidal = VidalMoment.None;
    }

    internal void Discard(Player player, TreacheryCardType cardType)
    {
        TreacheryCard card;
        if (cardType == TreacheryCardType.Karma && player.Is(Faction.Blue))
            card = player.TreacheryCards.First(x => x.Type == TreacheryCardType.Karma || x.Type == TreacheryCardType.Useless);
        else
            card = player.TreacheryCards.First(x => x.Type == cardType);

        Discard(player, card);
    }

    internal void Discard(TreacheryCard card)
    {
        var player = Players.SingleOrDefault(p => p.Has(card));
        Discard(player, card);
        RegisterKnown(card);
    }

    internal void Discard(Player player, TreacheryCard card)
    {
        if (player != null && card != null)
        {
            Log(player.Faction, " discard ", card);
            player.TreacheryCards.Remove(card);
            TreacheryDiscardPile.PutOnTop(card);
            RegisterKnown(card);
            RecentlyDiscarded.Add(card, player.Faction);
            Stone(Milestone.Discard);

            if (card.Type == TreacheryCardType.Poison || card.Type == TreacheryCardType.ProjectileAndPoison || card.Type == TreacheryCardType.PoisonTooth)
            {
                var pink = GetPlayer(Faction.Pink);
                if (pink != null && pink.HasHighThreshold())
                {
                    Log(Faction.Pink, " get ", Payment.Of(3), " from the discarded ", card);
                    pink.Resources += 3;
                }
            }
        }
    }

    #endregion SupportMethods

    #region TechnicalSupport

    public static Message TryLoad(GameState state, GameParticipation participation, bool performValidation, bool isHost, out Game result)
    {
        try
        {
            result = new Game(state.Version);

            var nr = 0;
            foreach (var e in state.Events)
            {
                e.Initialize(result);
                var message = e.Execute(performValidation, isHost);
                if (message != null)
                {
                    return Message.Express(e.GetType().Name, "(", nr, "):", message); 
                }
                nr++;
            }

            result.SetParticipation(participation);
            return null;
        }
        catch (Exception e)
        {
            result = null;
            return Message.Express(e.Message);
        }
    }

    internal void Log(params object[] expression)
    {
        CurrentReport.Express(expression);
    }

    internal void LogIf(bool condition, params object[] expression)
    {
        if (condition) CurrentReport.Express(expression);
    }

    internal void LogTo(Faction faction, params object[] expression)
    {
        CurrentReport.ExpressTo(faction, expression);
    }

    internal void Stone(Milestone m)
    {
        RecentMilestones.Add(m);
    }

    public Game Clone()
    {
        var result = (Game)MemberwiseClone();

        result.Players = Utilities.CloneList(Players);
        result.LeaderState = Utilities.CloneObjectDictionary(LeaderState);
        result.SecretsRemainHidden = new List<Faction>(SecretsRemainHidden);
        result.PreventedAdvantages = new List<FactionAdvantage>(PreventedAdvantages);
        result.AmbassadorsOnPlanet = Utilities.CloneEnumDictionary(AmbassadorsOnPlanet);
        result.TerrorOnPlanet = Utilities.CloneObjectDictionary(TerrorOnPlanet);
        result.StrongholdOwnership = Utilities.CloneEnumDictionary(StrongholdOwnership);

        return result;
    }

    #endregion TechnicalSupport

    #region Payments

    public void SetRecentPayment(int amount, Faction by, Faction to, GameEvent reason)
    {
        if (amount > 0) RecentlyPaid.Add(new Payment(amount, by, to, reason));
    }

    public void SetRecentPayment(int amount, Faction by, GameEvent reason)
    {
        SetRecentPayment(amount, by, Faction.None, reason);
    }

    public void ClearRecentPayments()
    {
        StoredRecentlyPaid = RecentlyPaid;
        RecentlyPaid = new List<Payment>();
    }

    public bool HasRecentPaymentFor(Type t)
    {
        return RecentlyPaid.Any(p => p.Reason != null && p.Reason.GetType() == t);
    }
    
    public bool HasQuiteRecentPaymentTo(Faction f)
    {
        return RecentlyPaid.Any(p => p.To == f) ||
               StoredRecentlyPaid.Any(p => p.To == f);
    }
    
    public Payment QuiteRecentPaymentTo(Faction f)
    {
        var res = RecentlyPaid.FirstOrDefault(p => p.To == f) ??
                          StoredRecentlyPaid.FirstOrDefault(p => p.To == f);
        return res;
    }

    public int RecentlyPaidTotalAmount => RecentlyPaid.Sum(p => p.Amount);


    #endregion Payments

    #region Information

    public bool Occupies(Faction f, World w)
    {
        if (f != Faction.None)
        {
            var hwOccupation = HomeworldOccupation.Keys.FirstOrDefault(hw => hw.World == w);
            if (hwOccupation != null) return HomeworldOccupation[hwOccupation] == f;
        }

        return false;
    }

    public Player OccupierOf(World w)
    {
        var hwOccupation = HomeworldOccupation.Keys.FirstOrDefault(hw => hw.World == w);
        if (hwOccupation != null) return GetPlayer(HomeworldOccupation[hwOccupation]);

        return null;
    }

    public HomeworldStatus GetStatusOf(Homeworld w)
    {
        var player = Players.FirstOrDefault(p => p.IsNative(w));

        if (player != null)
        {
            var occupier = OccupierOf(w.World);
            return new HomeworldStatus(player.HasHighThreshold(w.World), occupier != null ? occupier.Faction : Faction.None);
        }

        return null;
    }

    public bool IsInStorm(Location l)
    {
        return l.Sector == SectorInStorm;
    }

    public bool IsInStorm(Territory t)
    {
        return t.Locations.Any(l => IsInStorm(l));
    }

    public bool IsOccupied(Location l)
    {
        return Players.Any(p => p.Occupies(l));
    }

    public bool IsOccupied(Territory t)
    {
        return Players.Any(p => p.Occupies(t));
    }

    public bool IsOccupied(World world)
    {
        return OccupierOf(world) != null;
    }

    public bool IsOccupiedByFactionOrTheirAlly(World world, Player p)
    {
        var occupier = OccupierOf(world);
        return occupier != null && (occupier == p || occupier.Ally == p.Faction);
    }

    public bool IsOccupiedByFactionOrTheirAlly(World world, Faction f)
    {
        var occupier = OccupierOf(world);
        return occupier != null && (occupier.Is(f) || occupier.Ally == f);
    }

    public bool ContainsConflictingAlly(Player initiator, Location to)
    {
        if (initiator.Ally == Faction.None || Map.PolarSink.Equals(to) || initiator.Faction == Faction.Pink || initiator.Ally == Faction.Pink || to == null) return false;

        var ally = initiator.AlliedPlayer;

        if (initiator.Ally == Faction.Blue && Applicable(Rule.AdvisorsDontConflictWithAlly))
            return ally.ForcesIn(to.Territory) > 0;
        return ally.AnyForcesIn(to.Territory) > 0;
    }

    public Player OwnerOf(IHero hero)
    {
        return Players.FirstOrDefault(p => p.Leaders.Contains(hero));
    }

    public Player OwnerOf(TreacheryCard karmaCard)
    {
        return karmaCard != null ? Players.FirstOrDefault(p => p.Has(karmaCard)) : null;
    }

    public Player OwnerOf(TreacheryCardType cardType)
    {
        return Players.FirstOrDefault(p => p.TreacheryCards.Any(c => c.Type == cardType));
    }

    public Player OwnerOf(Location stronghold)
    {
        return StrongholdOwnership.TryGetValue(stronghold, out var value) ? GetPlayer(value) : null;
    }

    public int NrOfOccupantsExcludingFaction(Location l, Faction toExclude)
    {
        return CountDifferentFactions(BattalionsIn(l).Where(b => b.CanOccupy).Select(b => b.Faction), toExclude);
    }

    public int NrOfOccupantsExcludingFaction(Territory t, Faction toExclude)
    {
        return CountDifferentFactions(
            t.Locations.SelectMany(l => BattalionsIn(l).Where(b => b.CanOccupy).Select(b => b.Faction)), toExclude);
    }

    private int CountDifferentFactions(IEnumerable<Faction> factions, Faction toExclude)
    {
        var result = 0;
        var pinkAlly = GetAlly(Faction.Pink);
        var pinkOrPinkAllyCounted = false;

        foreach (var faction in factions.Distinct())
            if (faction == Faction.Pink || faction == pinkAlly)
            {
                if (!pinkOrPinkAllyCounted && toExclude != Faction.Pink && toExclude != pinkAlly) result++;

                pinkOrPinkAllyCounted = true;
            }
            else if (faction != toExclude)
            {
                result++;
            }

        return result;
    }

    public bool HasOrnithopters(Player p)
    {
        return (Applicable(Rule.MovementBonusRequiresOccupationBeforeMovement)
                   ? FactionsWithOrnithoptersAtStartOfMovement.Contains(p.Faction)
                   : OccupiesArrakeenOrCarthag(p)) ||
               (CurrentFlightUsed != null && CurrentFlightUsed.MoveThreeTerritories && CurrentFlightUsed.Player == p) ||
               (CurrentFlightDiscoveryUsed != null && CurrentFlightDiscoveryUsed.Player == p);
    }

    public int DetermineMaximumMoveDistance(Player p, IEnumerable<Battalion> moved)
    {
        var brownExtraMoveBonus = p.Faction == Faction.Brown && BrownHasExtraMove ? 1 : 0;

        var planetologyBonus = CurrentPlanetology != null && CurrentPlanetology.AddOneToMovement && CurrentPlanetology.Initiator == p.Faction ? 1 : 0;

        var result = 1 + planetologyBonus;

        if (HasOrnithopters(p))
            result = 3;
        else if (p.Is(Faction.Yellow) && !Prevented(FactionAdvantage.YellowExtraMove))
            result = 2 + planetologyBonus;
        else if (p.Is(Faction.Grey) && !Prevented(FactionAdvantage.GreyCyborgExtraMove) && moved.Any(b => b.AmountOfSpecialForces > 0)) result = 2 + planetologyBonus;

        return result + brownExtraMoveBonus;
    }

    public IEnumerable<Location> LocationsWithAnyForcesNotInStorm(Player p)
    {
        return Map.Locations(true).Where(l => l.Sector != SectorInStorm && p.AnyForcesIn(l) > 0);
    }

    public bool IsNotFull(Player p, Location l)
    {
        return
            !(l.Territory.IsStronghold || l.Territory.IsHomeworld) ||
            (p.Is(Faction.Blue) && p.SpecialForcesIn(l) > 0) ||
            (p.Is(Faction.Pink) && p.HasAlly && p.AlliedPlayer.AnyForcesIn(l.Territory) > 0) ||
            (p.Ally == Faction.Pink && p.AlliedPlayer.AnyForcesIn(l.Territory) > 0) ||
            NrOfOccupantsExcludingFaction(l, p.Faction) < 2;
    }

    internal bool EveryoneActedOrPassed => HasActedOrPassed.Count == Players.Count;

    public bool AssistedNotekeepingEnabled(Player p)
    {
        return Applicable(Rule.AssistedNotekeeping) ||
               (p.Is(Faction.Green) && Applicable(Rule.AssistedNotekeepingForGreen));
    }

    public bool HasStormPrescience(Player p)
    {
        return
            p != null &&
            Applicable(Rule.YellowSeesStorm) &&
            !Prevented(FactionAdvantage.YellowStormPrescience) &&
            (p.Faction == Faction.Yellow || (p.Ally == Faction.Yellow && YellowSharesPrescience) || HasDeal(p.Faction, DealType.ShareStormPrescience));
    }

    public bool HasHighThreshold(Faction f)
    {
        var player = GetPlayer(f);
        return player != null && player.HasHighThreshold();
    }

    public bool HasHighThreshold(Faction f, World w)
    {
        var player = GetPlayer(f);
        return player != null && player.HasHighThreshold(w);
    }

    public bool HasLowThreshold(Faction f)
    {
        var player = GetPlayer(f);
        return player != null && player.HasLowThreshold();
    }

    public bool HasResourceDeckPrescience(Player p)
    {
        return
            p != null &&
            !Prevented(FactionAdvantage.GreenSpiceBlowPrescience) &&
            !HasLowThreshold(Faction.Green) &&
            (p.Faction == Faction.Green || (p.Ally == Faction.Green && GreenSharesPrescience) || HasDeal(p.Faction, DealType.ShareResourceDeckPrescience));
    }

    public bool HasBiddingPrescience(Player p)
    {
        var isPubliclyKnown = CurrentAuctionType == AuctionType.WhiteOnceAround || CurrentAuctionType == AuctionType.WhiteSilent;

        return
            isPubliclyKnown ||

            Occupies(p.Faction, World.Green) ||
            Occupies(p.Ally, World.Green) ||

            (!Prevented(FactionAdvantage.GreenBiddingPrescience) &&
             (p.Faction == Faction.Green || (p.Ally == Faction.Green && GreenSharesPrescience) || HasDeal(p.Faction, DealType.ShareBiddingPrescience)));
    }

    public IEnumerable<IHero> TraitorsInPlay
    {
        get
        {
            var result = new List<IHero>();

            var factionsInPlay = Players.Select(p => p.Faction);
            result.AddRange(LeaderManager.Leaders.Where(l =>
                factionsInPlay.Contains(l.Faction) &&
                l.HeroType != HeroType.Vidal &&
                (Version <= 140 || l.HeroType != HeroType.Auditor || Applicable(Rule.BrownAuditor))
            ));

            if (Applicable(Rule.CheapHeroTraitor)) result.Add(TreacheryCardManager.GetCardsInPlay(this).First(c => c.Type == TreacheryCardType.Mercenary));

            return result;
        }
    }

    #endregion Information
    
    
}