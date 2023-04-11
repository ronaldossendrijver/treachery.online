/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public const int LowestSupportedVersion = 100;
        public const int LatestVersion = 154;

        public const int ExpansionLevel = 2;
        public bool BotInfologging = false;

        #region GameState
        public int Seed { get; internal set; } = -1;
        public int MaximumNumberOfTurns { get; internal set; }
        public int MaximumNumberOfPlayers { get; internal set; }
        public string Name { get; internal set; }
        public List<Milestone> RecentMilestones { get; } = new();
        public int Version { get; private set; }
        public Map Map { get; private set; } = new();
        public List<Rule> Rules { get; internal set; } = new();
        public List<Rule> RulesForBots { get; internal set; } = new();
        public List<Rule> AllRules { get; internal set; } = new();
        public List<GameEvent> History { get; private set; } = new();
        public List<Moment> Moments { get; private set; } = new();
        public int CurrentTurn { get; private set; } = 0;
        public MainPhase CurrentMainPhase { get; internal set; } = MainPhase.Started;
        public MainPhaseMoment CurrentMoment { get; private set; } = MainPhaseMoment.None;
        public Phase CurrentPhase { get; private set; } = Phase.None;
        public List<Faction> HasActedOrPassed { get; private set; } = new();
        public List<Player> Players { get; private set; } = new();
        public Report CurrentReport { get; internal set; }
        public Deck<TreacheryCard> TreacheryDeck { get; internal set; }
        public Deck<TreacheryCard> TreacheryDiscardPile { get; internal set; }
        public List<TreacheryCard> RemovedTreacheryCards { get; internal set; } = new();
        public List<TreacheryCard> WhiteCache { get; internal set; } = new();
        public Deck<ResourceCard> ResourceCardDeck { get; internal set; }
        public Deck<ResourceCard> ResourceCardDiscardPileA { get; internal set; }
        public Deck<ResourceCard> ResourceCardDiscardPileB { get; internal set; }
        public int SectorInStorm { get; private set; } = -1;
        public int NextStormMoves { get; private set; } = -1;
        public bool ShieldWallDestroyed { get; internal set; } = false;
        public Territory AtomicsAftermath { get; private set; } = null;
        public BrownEconomicsStatus EconomicsStatus { get; internal set; } = BrownEconomicsStatus.None;
        public Dictionary<Location, int> ResourcesOnPlanet { get; private set; } = new();
        public Dictionary<TerrorType, Territory> TerrorOnPlanet { get; private set; } = new();
        public Deck<Ambassador> UnassignedAmbassadors { get; internal set; }

        public List<Ambassador> AmbassadorsSetAside { get; private set; } = new();

        public Deck<DiscoveryToken> YellowDiscoveryTokens { get; set; }
        public Deck<DiscoveryToken> OrangeDiscoveryTokens { get; set; }
        public Dictionary<Location, Discovery> DiscoveriesOnPlanet { get; private set; } = new();

        public Dictionary<Territory, Ambassador> AmbassadorsOnPlanet { get; private set; } = new();
        public Dictionary<IHero, LeaderState> LeaderState { get; private set; } = new();
        public Deck<LeaderSkill> SkillDeck { get; private set; }
        public Deck<Faction> NexusCardDeck { get; internal set; }
        public List<Faction> NexusDiscardPile { get; private set; } = new();
        public Dictionary<Player, Dictionary<MainPhase, TimeSpan>> Timers { get; private set; } = new();
        internal Random Random { get; set; }

        #endregion GameState

        #region Initialization

        public Game() : this(LatestVersion)
        {

        }

        public Game(int version)
        {
            if (version < LowestSupportedVersion)
            {
                throw new ArgumentException(string.Format("Game version {0} is not supported. The lowest supported version is: {1}.", version, LowestSupportedVersion));
            }

            Version = version;

            InitializeLeaderState();
            EnterPhaseAwaitingPlayers();
        }

        private void InitializeLeaderState()
        {
            foreach (var l in LeaderManager.Leaders)
            {
                LeaderState.Add(l, new LeaderState() { DeathCounter = 0, CurrentTerritory = null, Skill = LeaderSkill.None, InFrontOfShield = false });
            }

            LeaderState.Add(LeaderManager.Messiah, new LeaderState() { DeathCounter = 0, CurrentTerritory = null });
        }

        #endregion Initialization

        #region EventHandling

        public List<Payment> RecentlyPaid { get; private set; } = new();

        public void SetRecentPayment(int amount, Faction by, Faction to, GameEvent reason)
        {
            if (amount > 0)
            {
                RecentlyPaid.Add(new Payment(amount, by, to, reason));
            }
        }

        public void SetRecentPayment(int amount, Faction by, GameEvent reason)
        {
            SetRecentPayment(amount, by, Faction.None, reason);
        }

        public bool HasRecentPaymentFor(Type t) => RecentlyPaid.Any(p => p.Reason != null && p.Reason.GetType() == t);

        public bool HasReceivedPaymentFor(Faction receiver, Type t) => RecentlyPaid.Any(p => p.To == receiver && p.Reason != null && p.Reason.GetType() == t);

        public int RecentlyPaidTotalAmount => RecentlyPaid.Sum(p => p.Amount);

        internal List<Payment> StoredRecentlyPaid { get; set; } = new();
        public void ClearRecentPayments()
        {
            StoredRecentlyPaid = RecentlyPaid;
            RecentlyPaid = new();
        }

        public void PerformPreEventTasks(GameEvent e)
        {
            UpdateTimers(e);

            if (!(e is AllyPermission || e is PlayerReplaced))
            {
                ClearRecentPayments();

                if (!(e is DealOffered || e is DealAccepted))
                {
                    RecentlyDiscarded.Clear();
                }
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
            for (int i = 0; i < untilEventNr; i++)
            {
                var clone = History[i].Clone();
                clone.Game = result;
                clone.ExecuteWithoutValidation();
            }
            return result;
        }

        public GameEvent LatestEvent(Type eventType) => History.LastOrDefault(e => e.GetType() == eventType);

        public GameEvent LatestEvent(params Type[] eventType)
        {
            for (int i = History.Count - 1; i >= 0; i--)
            {
                if (eventType.Any(t => t == History[i].GetType()))
                {

                    return History[i];
                }
            }

            return null;
        }

        public GameEvent LatestEvent() => History.Count > 0 ? History[^1] : null;

        public int EventCount => History.Count;

        internal Phase PhaseBeforeDiscarding { get; set; }
        public List<Faction> FactionsThatMustDiscard { get; internal set; } = new();

        internal Phase PhaseBeforeDiscardingTraitor { get; set; }
        internal Faction FactionThatMustDiscardTraitor { get; set; }
        internal int NumberOfTraitorsToDiscard { get; set; }
        public void HandleEvent(TraitorDiscarded e)
        {
            Log(e);
            TraitorDeck.Items.Add(e.Traitor);
            e.Player.Traitors.Remove(e.Traitor);
            NumberOfTraitorsToDiscard--;

            if (NumberOfTraitorsToDiscard == 0)
            {
                CurrentPhase = PhaseBeforeDiscardingTraitor;
            }

        }

        public void HandleEvent(RedDiscarded e)
        {
            Log(e);
            e.Player.Resources -= 2;
            Discard(e.Player, e.Card);
        }

        #endregion EventHandling

        #region Timers

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
                    Timers[e.Player][CurrentMainPhase] += e.Time.Subtract(previousEvent.Time);
                }
            }
        }

        private GameEvent FindMostRecentEvent(params Type[] types)
        {
            if (types.Length == 0) return History[^1];

            for (int i = History.Count - 1; i >= 0; i--)
            {
                if (types.Contains(History[i].GetType()))
                {
                    return History[i];
                }
            }

            return null;
        }

        public TimeSpan Duration => History.Count > 0 ? History[^1].Time.Subtract(History[0].Time) : TimeSpan.Zero;

        public TimeSpan TimeSpent(Player player, MainPhase phase)
        {
            if (Timers.TryGetValue(player, out Dictionary<MainPhase, TimeSpan> timersIfPlayer) && timersIfPlayer.TryGetValue(phase, out TimeSpan value))
            {
                return value;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        public DateTime Started
        {
            get
            {
                if (History.Count > 0)
                {
                    return History[0].Time;
                }
                else
                {
                    return default;
                }
            }
        }

        private bool InTimedPhase =>
            CurrentPhase == Phase.Bidding ||
            CurrentPhase == Phase.OrangeShip ||
            CurrentPhase == Phase.OrangeMove ||
            CurrentPhase == Phase.NonOrangeShip ||
            CurrentPhase == Phase.NonOrangeMove ||
            CurrentPhase == Phase.BattlePhase;

        #endregion

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

            if (CurrentMainPhase == MainPhase.Bidding && Prevented(FactionAdvantage.PurpleIncreasingRevivalLimits))
            {
                exceptionsToAllowing.Add(FactionAdvantage.PurpleIncreasingRevivalLimits);
            }

            if (CurrentMainPhase == MainPhase.Resurrection && Prevented(FactionAdvantage.GreenSpiceBlowPrescience))
            {
                exceptionsToAllowing.Add(FactionAdvantage.GreenSpiceBlowPrescience);
            }

            WasVictimOfBureaucracy = Faction.None;

            if (Version >= 103) AllowAllPreventedFactionAdvantages(exceptionsToAllowing);
        }

        internal void Enter(Phase phase)
        {
            CurrentPhase = phase;
            RemoveEndedDeals(phase);
        }

        internal void Enter(bool condition, Phase phaseIfTrue, Phase phaseIfFalse)
        {
            if (condition)
            {
                Enter(phaseIfTrue);
            }
            else
            {
                Enter(phaseIfFalse);
            }
        }

        internal void Enter(bool condition, Phase phaseIfTrue, Action methodOtherwise)
        {
            if (condition)
            {
                Enter(phaseIfTrue);
            }
            else
            {
                methodOtherwise();
            }
        }

        internal static void Enter(bool condition, Action methodIfTrue, Action methodOtherwise)
        {
            if (condition)
            {
                methodIfTrue();
            }
            else
            {
                methodOtherwise();
            }
        }

        internal void Enter(bool condition1, Action actionIf1True, bool condition2, Phase phaseIf2True, Action methodOtherwise)
        {
            if (condition1)
            {
                actionIf1True();
            }
            else if (condition2)
            {
                Enter(phaseIf2True);
            }
            else
            {
                methodOtherwise();
            }
        }

        internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, bool condition3, Phase phaseIf3True, Action methodOtherwise)
        {
            if (condition1)
            {
                Enter(phaseIf1True);
            }
            else if (condition2)
            {
                Enter(phaseIf2True);
            }
            else if (condition3)
            {
                Enter(phaseIf3True);
            }
            else
            {
                methodOtherwise();
            }
        }

        internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, bool condition3, Phase phaseIf3True)
        {
            if (condition1)
            {
                Enter(phaseIf1True);
            }
            else if (condition2)
            {
                Enter(phaseIf2True);
            }
            else if (condition3)
            {
                Enter(phaseIf3True);
            }
        }

        internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, bool condition3, Action methodIf3True, Action methodOtherwise)
        {
            if (condition1)
            {
                Enter(phaseIf1True);
            }
            else if (condition2)
            {
                Enter(phaseIf2True);
            }
            else if (condition3)
            {
                methodIf3True();
            }
            else
            {
                methodOtherwise();
            }
        }

        internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, Action methodOtherwise)
        {
            if (condition1)
            {
                Enter(phaseIf1True);
            }
            else if (condition2)
            {
                Enter(phaseIf2True);
            }
            else
            {
                methodOtherwise();
            }
        }

        internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Action methodIf2True, Action methodOtherwise)
        {
            if (condition1)
            {
                Enter(phaseIf1True);
            }
            else if (condition2)
            {
                methodIf2True();
            }
            else
            {
                methodOtherwise();
            }
        }

        internal void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, Phase phaseOtherwise)
        {
            if (condition1)
            {
                Enter(phaseIf1True);
            }
            else if (condition2)
            {
                Enter(phaseIf2True);
            }
            else
            {
                Enter(phaseOtherwise);
            }
        }

        internal void Enter(bool condition1, Action methodIf1True, bool condition2, Phase phaseIf2True, Phase phaseOtherwise)
        {
            if (condition1)
            {
                methodIf1True();
            }
            else if (condition2)
            {
                Enter(phaseIf2True);
            }
            else
            {
                Enter(phaseOtherwise);
            }
        }

        #endregion

        #region CardKnowledge

        public IEnumerable<TreacheryCard> KnownCards(Faction f)
        {
            return KnownCards(GetPlayer(f));
        }

        public IEnumerable<TreacheryCard> KnownCards(Player p)
        {
            var result = new List<TreacheryCard>(p.TreacheryCards);
            result.AddRange(p.KnownCards);

            if (p.Ally != Faction.None)
            {
                var ally = GetPlayer(p.Ally);
                result.AddRange(ally.TreacheryCards);
                result.AddRange(ally.KnownCards);
            }

            return result.Distinct();
        }

        internal void RegisterKnown(TreacheryCard c)
        {
            foreach (var p in Players)
            {
                RegisterKnown(p, c);
            }
        }

        internal static void RegisterKnown(Player p, TreacheryCard c)
        {
            if (c != null && !p.KnownCards.Contains(c))
            {
                p.KnownCards.Add(c);
            }
        }

        internal void RegisterKnown(Faction f, TreacheryCard c)
        {
            var p = GetPlayer(f);
            if (p != null)
            {
                RegisterKnown(p, c);
            }
        }

        internal void UnregisterKnown(TreacheryCard c)
        {
            foreach (var p in Players)
            {
                UnregisterKnown(p, c);
            }
        }

        internal static void UnregisterKnown(Player p, TreacheryCard c)
        {
            p.KnownCards.Remove(c);
        }

        internal static void UnregisterKnown(Player p, IEnumerable<TreacheryCard> cards)
        {
            foreach (var c in cards)
            {
                UnregisterKnown(p, c);
            }
        }

        #endregion

        #region Forces

        public bool IsInStorm(Location l) => l.Sector == SectorInStorm;

        public bool IsInStorm(Territory t) => t.Locations.Any(l => IsInStorm(l));

        public bool IsOccupied(Location l) => Players.Any(p => p.Occupies(l));

        public bool IsOccupied(Territory t) => Players.Any(p => p.Occupies(t));

        public Dictionary<Location, List<Battalion>> Forces(bool includeHomeworlds = false)
        {
            Dictionary<Location, List<Battalion>> result = new();

            foreach (var l in Map.Locations(includeHomeworlds))
            {
                result.Add(l, new List<Battalion>());
            }

            foreach (var p in Players)
            {
                if (includeHomeworlds)
                {
                    foreach (var w in p.Homeworlds)
                    {
                        result.Add(w, new List<Battalion>());
                    }
                }

                var forces = includeHomeworlds ? p.ForcesInLocations : p.ForcesOnPlanet;

                foreach (var locationAndBattaltion in forces)
                {
                    result[locationAndBattaltion.Key].Add(locationAndBattaltion.Value);
                }
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
                    if (!result.ContainsKey(locationAndBattaltion.Key))
                    {
                        result.Add(locationAndBattaltion.Key, new List<Battalion>());
                    }

                    result[locationAndBattaltion.Key].Add(locationAndBattaltion.Value);
                }
            }

            return result;
        }

        public bool AnyForcesIn(Territory t)
        {
            foreach (var p in Players)
            {
                foreach (var l in t.Locations)
                {
                    if (p.ForcesInLocations.ContainsKey(l))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerable<Battalion> BattalionsIn(Location l)
        {
            var result = new List<Battalion>();
            foreach (var p in Players)
            {
                if (p.BattalionIn(l, out Battalion batallion))
                {
                    result.Add(batallion);
                }
            }

            return result;
        }

        public IEnumerable<Battalion> BattalionsIn(Territory t)
        {
            var result = new List<Battalion>();
            foreach (var l in t.Locations)
            {
                result.AddRange(BattalionsIn(l));
            }

            return result;
        }

        public Dictionary<Location, List<Battalion>> OccupyingForcesOnPlanet
        {
            get
            {
                Dictionary<Location, List<Battalion>> result = new();

                foreach (var p in Players)
                {
                    foreach (var locationAndBattaltion in p.ForcesInLocations.Where(kvp => p.Occupies(kvp.Key)))
                    {
                        if (!result.ContainsKey(locationAndBattaltion.Key))
                        {
                            result.Add(locationAndBattaltion.Key, new List<Battalion>());
                        }

                        result[locationAndBattaltion.Key].Add(locationAndBattaltion.Value);
                    }
                }

                return result;
            }
        }

        public int NrOfOccupantsExcludingPlayer(Location l, Player p)
        {
            Faction pinkOrPinkAllyToExclude = Faction.None;

            if (p.Is(Faction.Pink))
            {
                pinkOrPinkAllyToExclude = p.Ally;
            }
            else if (p.Ally == Faction.Pink)
            {
                pinkOrPinkAllyToExclude = p.Faction;
            }

            if (OccupyingForcesOnPlanet.TryGetValue(l, out List<Battalion> value))
            {
                return value.Where(of => of.Faction != p.Faction && of.Faction != pinkOrPinkAllyToExclude).Count();
            }
            else
            {
                return 0;
            }
        }

        public int NrOfOccupantsExcludingPlayer(Territory t, Player p)
        {
            int result = 0;
            var counted = new List<Faction>();

            Faction pinkOrPinkAllyToExclude = Faction.None;

            if (p.Is(Faction.Pink))
            {
                pinkOrPinkAllyToExclude = p.Ally;
            }
            else if (p.Ally == Faction.Pink)
            {
                pinkOrPinkAllyToExclude = p.Faction;
            }

            foreach (Location l in t.Locations)
            {
                if (OccupyingForcesOnPlanet.TryGetValue(l, out List<Battalion> batallionsInLocation))
                {
                    foreach (var b in batallionsInLocation)
                    {
                        if (b.Faction != p.Faction && !counted.Contains(b.Faction) && b.Faction != pinkOrPinkAllyToExclude)
                        {
                            result++;
                            counted.Add(b.Faction);
                        }
                    }
                }
            }

            return result;
        }

        public bool HasOrnithopters(Player p) =>
            (Applicable(Rule.MovementBonusRequiresOccupationBeforeMovement) ? FactionsWithOrnithoptersAtStartOfMovement.Contains(p.Faction) : OccupiesArrakeenOrCarthag(p)) ||
            CurrentFlightUsed != null && CurrentFlightUsed.MoveThreeTerritories && CurrentFlightUsed.Player == p ||
            CurrentFlightDiscoveryUsed != null && CurrentFlightDiscoveryUsed.Player == p;

        public int DetermineMaximumMoveDistance(Player p, IEnumerable<Battalion> moved)
        {
            int brownExtraMoveBonus = p.Faction == Faction.Brown && BrownHasExtraMove ? 1 : 0;

            int planetologyBonus = CurrentPlanetology != null && CurrentPlanetology.AddOneToMovement && CurrentPlanetology.Initiator == p.Faction ? 1 : 0;

            int result = 1 + planetologyBonus;

            if (HasOrnithopters(p))
            {
                result = 3;
            }
            else if (p.Is(Faction.Yellow) && !Prevented(FactionAdvantage.YellowExtraMove))
            {
                result = 2 + planetologyBonus;
            }
            else if (p.Is(Faction.Grey) && !Prevented(FactionAdvantage.GreyCyborgExtraMove) && moved.Any(b => b.AmountOfSpecialForces > 0))
            {
                result = 2 + planetologyBonus;
            }

            return result + brownExtraMoveBonus;
        }

        public IEnumerable<Location> LocationsWithAnyForcesNotInStorm(Player p)
        {
            return Map.Locations(true).Where(l => l.Sector != SectorInStorm && p.AnyForcesIn(l) > 0);
        }

        internal void FlipBeneGesseritWhenAloneOrWithPinkAlly()
        {
            var bg = GetPlayer(Faction.Blue);
            if (bg != null)
            {
                var pink = GetPlayer(Faction.Pink);
                var territoriesWhereAdvisorsAreAloneOrWithPink = Map.Territories(true).Where(t => bg.SpecialForcesIn(t) > 0 &&
                    (!Players.Any(p => p.Faction != Faction.Blue && p.AnyForcesIn(t) > 0) || bg.Ally == Faction.Pink && pink.AnyForcesIn(t) > 0));

                foreach (var t in territoriesWhereAdvisorsAreAloneOrWithPink)
                {
                    bg.FlipForces(t, false);
                    Log(Faction.Blue, " are alone and flip to ", FactionForce.Blue, " in ", t);
                }
            }
        }

        #endregion

        #region Resources

        internal void ChangeResourcesOnPlanet(Location location, int amount)
        {
            if (ResourcesOnPlanet.ContainsKey(location))
            {
                if (ResourcesOnPlanet[location] + amount == 0)
                {
                    ResourcesOnPlanet.Remove(location);
                }
                else
                {
                    ResourcesOnPlanet[location] = ResourcesOnPlanet[location] + amount;
                }
            }
            else if (amount != 0)
            {
                ResourcesOnPlanet.Add(location, amount);
            }
        }

        private int RemoveResources(Location location)
        {
            int result = 0;
            if (ResourcesOnPlanet.ContainsKey(location))
            {
                result = ResourcesOnPlanet[location];
                ResourcesOnPlanet.Remove(location);
            }

            return result;
        }

        private int RemoveResources(Territory territory)
        {
            int result = 0;
            foreach (var l in territory.Locations)
            {
                result += RemoveResources(l);
            }

            return result;
        }

        #endregion

        #region PlayersAndFactions

        public bool IsPlaying(Faction faction) => Players.Any(p => p.Faction == faction);

        public Player GetPlayer(string name)
        {
            return Players.FirstOrDefault(p => p.Name == name);
        }

        public Player GetPlayer(Faction? f)
        {
            return Players.FirstOrDefault(p => p.Faction == f);
        }

        public Faction GetAlly(Faction f)
        {
            var player = GetPlayer(f);
            return player != null ? player.Ally : Faction.None;
        }

        public Player GetAlliedPlayer(Faction f)
        {
            return GetPlayer(f)?.AlliedPlayer;
        }

        public IEnumerable<Faction> PlayersOtherThan(Player p)
        {
            return Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

        #endregion

        #region SupportMethods

        public bool IsNotFull(Player p, Location l)
        {
            return
                !(l.Territory.IsStronghold || l.Territory.IsHomeworld) ||
                p.Is(Faction.Blue) && p.SpecialForcesIn(l) > 0 ||
                p.Is(Faction.Pink) && p.HasAlly && p.AlliedPlayer.AnyForcesIn(l.Territory) > 0 ||
                p.Ally == Faction.Pink && p.AlliedPlayer.AnyForcesIn(l.Territory) > 0 ||
                NrOfOccupantsExcludingPlayer(l, p) < 2;
        }



        private void AllowAllPreventedFactionAdvantages(IEnumerable<FactionAdvantage> exceptions)
        {
            foreach (var adv in Enumerations.GetValuesExceptDefault(typeof(FactionAdvantage), FactionAdvantage.None))
            {
                if (exceptions == null || !exceptions.Contains(adv))
                {
                    Allow(adv);
                }
            }
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

        private bool EveryoneActedOrPassed => HasActedOrPassed.Count == Players.Count;

        public bool AssistedNotekeepingEnabled(Player p) => Applicable(Rule.AssistedNotekeeping) || p.Is(Faction.Green) && Applicable(Rule.AssistedNotekeepingForGreen);

        public bool HasRoomForLeaders(Player p) =>
            p.Leaders.Count(l => IsAlive(l)) -
            (p.Is(Faction.Black) ? p.Leaders.Count(l => l.Faction != Faction.Black) : 0)
            < 5;

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
            bool isPubliclyKnown = CurrentAuctionType == AuctionType.WhiteOnceAround || CurrentAuctionType == AuctionType.WhiteSilent;

            return
                isPubliclyKnown ||

                Occupies(p.Faction, World.Green) ||
                Occupies(p.Ally, World.Green) ||

                (p != null &&
                !Prevented(FactionAdvantage.GreenBiddingPrescience) &&
                (p.Faction == Faction.Green || (p.Ally == Faction.Green && GreenSharesPrescience) || HasDeal(p.Faction, DealType.ShareBiddingPrescience)));
        }

        public Dictionary<Homeworld, Faction> HomeworldOccupation { get; private set; } = new();
        public void DetermineOccupationAfterLocationEvent(ILocationEvent e)
        {
            var currentOccupierOfPinkHomeworld = OccupierOf(World.Pink);
            var player = GetPlayer(e.Initiator);

            if (e.To is Homeworld hw && !player.Homeworlds.Contains(hw) && player.Controls(this, hw, false))
            {
                if (!Occupies(e.Initiator, hw.World))
                {
                    HomeworldOccupation.Remove(hw);
                    HomeworldOccupation.Add(hw, e.Initiator);
                    Log(e.Initiator, " now occupy ", hw);
                }
            }

            CheckIfShipmentPermissionsShouldBeRevoked();
            CheckIfOccupierTakesVidal(currentOccupierOfPinkHomeworld);
            LetFactionsDiscardSurplusCards();
        }

        public void DetermineOccupationAtStartOrEndOfTurn()
        {
            var currentOccupierOfPinkHomeworld = OccupierOf(World.Pink);
            var updatedOccupation = new Dictionary<Homeworld, Faction>();

            foreach (var hw in Map.Homeworlds)
            {
                foreach (var player in Players)
                {
                    if (player.Controls(this, hw, false) && !player.IsNative(hw))
                    {
                        updatedOccupation.Add(hw, player.Faction);

                        if (!Occupies(player.Faction, hw.World))
                        {
                            Log(player.Faction, " now occupy ", hw);
                        }
                    }
                }
            }

            foreach (var kvp in HomeworldOccupation)
            {
                if (!updatedOccupation.Contains(kvp))
                {
                    Log(kvp.Value, " no longer occupies ", kvp.Key);
                }
            }

            HomeworldOccupation = updatedOccupation;

            CheckIfShipmentPermissionsShouldBeRevoked();
            CheckIfOccupierTakesVidal(currentOccupierOfPinkHomeworld);
            LetFactionsDiscardSurplusCards();
        }

        private void CheckIfShipmentPermissionsShouldBeRevoked()
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

        private void CheckIfOccupierTakesVidal(Player previousOccupierOfPinkHomeworld)
        {
            var occupierOfPinkHomeworld = OccupierOf(World.Pink);
            if (occupierOfPinkHomeworld != null)
            {
                if (!occupierOfPinkHomeworld.Leaders.Contains(Vidal))
                {
                    TakeVidal(occupierOfPinkHomeworld, VidalMoment.WhilePinkWorldIsOccupied);
                }
            }
            else
            {
                previousOccupierOfPinkHomeworld?.Leaders.Remove(Vidal);
            }
        }

        public bool Occupies(Faction f, World w)
        {
            if (f != Faction.None)
            {
                var hwOccupation = HomeworldOccupation.Keys.FirstOrDefault(hw => hw.World == w);
                if (hwOccupation != null)
                {
                    return HomeworldOccupation[hwOccupation] == f;
                }
            }

            return false;
        }

        public bool Occupies(Faction f, Homeworld w) => f != Faction.None && HomeworldOccupation.TryGetValue(w, out Faction value) && value == f;

        public Player OccupierOf(Homeworld w) => HomeworldOccupation.TryGetValue(w, out Faction value) ? GetPlayer(value) : null;

        public Player OccupierOf(World w)
        {
            var hwOccupation = HomeworldOccupation.Keys.FirstOrDefault(hw => hw.World == w);
            if (hwOccupation != null)
            {
                return GetPlayer(HomeworldOccupation[hwOccupation]);
            }

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

        public int NumberOfHumanPlayers => Players.Count(p => !p.IsBot);

        internal TreacheryCard Discard(Faction faction, TreacheryCardType cardType) => Discard(GetPlayer(faction), cardType);

        internal TreacheryCard Discard(Player player, TreacheryCardType cardType)
        {
            TreacheryCard card = null;
            if (cardType == TreacheryCardType.Karma && player.Is(Faction.Blue))
            {
                card = player.TreacheryCards.First(x => x.Type == TreacheryCardType.Karma || x.Type == TreacheryCardType.Useless);
            }
            else
            {
                card = player.TreacheryCards.First(x => x.Type == cardType);
            }

            if (card != null)
            {
                Discard(player, card);
            }
            else
            {
                Log(cardType, " card not found");
            }

            return card;
        }

        internal void Discard(TreacheryCard card)
        {
            var player = Players.SingleOrDefault(p => p.TreacheryCards.Contains(card));
            Discard(player, card);
            RegisterKnown(card);
        }

        public Dictionary<TreacheryCard, Faction> RecentlyDiscarded { get; private set; } = new Dictionary<TreacheryCard, Faction>();
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

        public Player OwnerOf(IHero hero) => Players.FirstOrDefault(p => p.Leaders.Contains(hero));

        public Player OwnerOf(TreacheryCard karmaCard) => karmaCard != null ? Players.FirstOrDefault(p => p.TreacheryCards.Contains(karmaCard)) : null;

        public Player OwnerOf(TreacheryCardType cardType) => Players.FirstOrDefault(p => p.TreacheryCards.Any(c => c.Type == cardType));

        public Player OwnerOf(Location stronghold) => StrongholdOwnership.TryGetValue(stronghold, out Faction value) ? GetPlayer(value) : null;

        public static Message TryLoad(GameState state, bool performValidation, bool isHost, out Game result)
        {
            try
            {
                result = new Game(state.Version);

                int nr = 0;
                foreach (var e in state.Events)
                {
                    e.Game = result;
                    var message = e.Execute(performValidation, isHost);
                    if (message != null)
                    {
                        return Message.Express(e.GetType().Name, "(", nr, "):", message); ;
                    }
                    nr++;
                }

                return null;
            }
            catch (Exception e)
            {
                result = null;
                return Message.Express(e.Message);
            }
        }

        private void Log(GameEvent e)
        {
            CurrentReport.Express(e);
        }

        private void Log(params object[] expression)
        {
            CurrentReport.Express(expression);
        }

        private void LogIf(bool condition, params object[] expression)
        {
            if (condition)
            {
                CurrentReport.Express(expression);
            }
        }

        private void LogTo(Faction faction, params object[] expression)
        {
            CurrentReport.ExpressTo(faction, expression);
        }

        internal void Stone(Milestone m)
        {
            RecentMilestones.Add(m);
        }

        #endregion SupportMethods
    }
}
