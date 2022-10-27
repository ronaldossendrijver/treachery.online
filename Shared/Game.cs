/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public const int LowestSupportedVersion = 100;
        public const int LatestVersion = 151;
        public const int ExpansionLevel = 3;

        public bool BotInfologging = true;
        
        #region GameState
        public int Seed { get; private set; } = -1;
        public int MaximumNumberOfTurns { get; private set; }
        public int MaximumNumberOfPlayers { get; private set; }
        public string Name { get; private set; }
        public List<Milestone> RecentMilestones { get; private set; } = new();
        public int Version { get; private set; }
        public Map Map { get; private set; } = new Map();
        public List<Rule> Rules { get; private set; } = new();
        public List<Rule> RulesForBots { get; private set; } = new();
        public List<Rule> AllRules { get; private set; } = new();
        public List<GameEvent> History { get; private set; } = new();
        public bool TrackStatesForReplay { get; private set; } = true;
        public List<Game> States { get; private set; } = new();
        public int CurrentTurn { get; private set; } = 0;
        public MainPhase CurrentMainPhase { get; private set; } = MainPhase.Started;
        public MainPhaseMoment CurrentMoment { get; private set; } = MainPhaseMoment.None;
        public Phase CurrentPhase { get; private set; } = Phase.None;
        public List<Faction> HasActedOrPassed { get; private set; } = new();
        public List<Player> Players { get; private set; } = new();
        public Report CurrentReport { get; private set; }
        public Deck<TreacheryCard> TreacheryDeck { get; private set; }
        public Deck<TreacheryCard> TreacheryDiscardPile { get; private set; }
        public List<TreacheryCard> RemovedTreacheryCards { get; private set; } = new ();
        public List<TreacheryCard> WhiteCache { get; private set; } = new ();
        public Deck<ResourceCard> ResourceCardDeck { get; private set; }
        public Deck<ResourceCard> ResourceCardDiscardPileA { get; private set; }
        public Deck<ResourceCard> ResourceCardDiscardPileB { get; private set; }
        public int SectorInStorm { get; private set; } = -1;
        public int NextStormMoves { get; private set; } = -1;
        public bool ShieldWallDestroyed { get; private set; } = false;
        public Territory AtomicsAftermath { get; private set; } = null;
        public BrownEconomicsStatus EconomicsStatus { get; private set; } = BrownEconomicsStatus.None;
        public Dictionary<Location, int> ResourcesOnPlanet { get; private set; } = new();
        public Dictionary<TerrorType, Territory> TerrorOnPlanet { get; private set; } = new();
        public Dictionary<IHero, LeaderState> LeaderState { get; private set; } = new();
        public Deck<LeaderSkill> SkillDeck { get; private set; }
        public Deck<Faction> NexusCardDeck { get; private set; }
        public List<Faction> NexusDiscardPile { get; private set; } = new();
        public Dictionary<Player, Dictionary<MainPhase, TimeSpan>> Timers { get; private set; } = new();
        private Random Random { get; set; }

        #endregion GameState

        #region Initialization

        public Game() : this(LatestVersion, true)
        {

        }

        public Game(bool trackStatesForReplay) : this(LatestVersion, trackStatesForReplay)
        {

        }

        public Game(int version, bool trackStatesForReplay)
        {
            if (version < LowestSupportedVersion)
            {
                throw new ArgumentException(string.Format("Game version {0} is not supported. The lowest supported version is: {1}.", version, LowestSupportedVersion));
            }

            Version = version;
            TrackStatesForReplay = trackStatesForReplay;
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

        public void PerformPreEventTasks(GameEvent e)
        {
            UpdateTimers(e);

            if (!(e is AllyPermission || e is PlayerReplaced || e is DealOffered || e is DealAccepted))
            {
                RecentlyDiscarded.Clear();
            }
        }

        public void PerformPostEventTasks(GameEvent e, bool justEnteredStartOfPhase)
        {
            if (!justEnteredStartOfPhase && e is not AllyPermission && e is not DealOffered && e is not DealAccepted) MainPhaseMiddle();

            History.Add(e);

            if (TrackStatesForReplay)
            {
                States.Add(Clone());
            }
        }

        public Game Undo(int untilEventNr)
        {
            var result = new Game(Version, TrackStatesForReplay);
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

        public void HandleEndPhaseEvent()
        {
            switch (CurrentPhase)
            {
                case Phase.SelectingFactions:
                    AssignFactionsAndEnterFactionTrade();
                    break;

                case Phase.TradingFactions:
                    EstablishDecks();
                    break;

                case Phase.MetheorAndStormSpell:
                    EnterNormalStormPhase();
                    break;

                case Phase.StormReport:
                    EnterSpiceBlowPhase();
                    break;

                case Phase.Thumper:
                    EnterBlowA();
                    break;

                case Phase.HarvesterA:
                case Phase.HarvesterB:
                    MoveToNextPhaseAfterResourceBlow();
                    break;

                case Phase.AllianceA:
                case Phase.AllianceB:
                    EndNexus();
                    break;

                case Phase.BlowReport:
                    if (Applicable(Rule.HasCharityPhase))
                    {
                        EnterCharityPhase();
                    }
                    else
                    {
                        EnterBiddingPhase();
                    }
                    break;

                case Phase.BeginningOfCharity:
                    StartClaimingCharity();
                    break;

                case Phase.ClaimingCharity:
                    EndCharityPhase();
                    break;

                case Phase.CharityReport:
                    EnterBiddingPhase();
                    break;

                case Phase.WaitingForNextBiddingRound:
                    PutNextCardOnAuction();
                    break;

                case Phase.BiddingReport:
                    EnterRevivalPhase();
                    break;

                case Phase.BeginningOfResurrection:
                    StartResurrection();
                    break;

                case Phase.Resurrection:
                    EndResurrectionPhase();
                    break;

                case Phase.ResurrectionReport:
                    EnterShipmentAndMovePhase();
                    break;

                case Phase.BeginningOfShipAndMove:
                    StartShipAndMoveSequence();
                    break;

                case Phase.ShipmentAndMoveConcluded:
                    EnterBattlePhase();
                    break;

                case Phase.BeginningOfBattle:
                    Enter(Phase.BattlePhase);
                    break;

                case Phase.BattleReport:
                    ResetBattle();
                    Enter(NextPlayerToBattle != null, Phase.BattlePhase, EnterSpiceCollectionPhase);
                    break;

                case Phase.BeginningOfCollection:
                    StartCollection();
                    break;

                case Phase.CollectionReport:
                    EnterMentatPhase();
                    break;

                case Phase.Extortion:
                    ContinueMentatPhase();
                    break;

                case Phase.Contemplate:
                    ContinueMentatPhase();
                    break;

                case Phase.TurnConcluded:
                    if (Version < 108) AddBribesToPlayerResources();
                    EnterStormPhase();
                    break;
            }
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
            if (types.Length == 0) return History[History.Count - 1];

            for (int i = History.Count - 1; i >= 0; i--)
            {
                if (types.Contains(History[i].GetType()))
                {
                    return History[i];
                }
            }

            return null;
        }

        public TimeSpan Duration => History.Count > 0 ? History[History.Count - 1].Time.Subtract(History[0].Time) : TimeSpan.Zero;

        public TimeSpan TimeSpent(Player player, MainPhase phase)
        {
            if (Timers.ContainsKey(player) && Timers[player].ContainsKey(phase))
            {
                return Timers[player][phase];
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

        private void MainPhaseStart(MainPhase phase, bool clearReport = true)
        {
            CurrentMainPhase = phase;
            CurrentMoment = MainPhaseMoment.Start;
            if (clearReport) CurrentReport = new Report(phase);
            CurrentKarmaPrevention = null;
            CurrentJuice = null;
            BureaucratWasUsedThisPhase = false;
            BankerWasUsedThisPhase = false;
        }

        private void MainPhaseMiddle()
        {
            if (CurrentMoment == MainPhaseMoment.Start) CurrentMoment = MainPhaseMoment.Middle;
        }

        private void MainPhaseEnd()
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

            if (Version >= 103) AllowAllPreventedFactionAdvantages(exceptionsToAllowing);
        }

        private void Enter(Phase phase)
        {
            CurrentPhase = phase;
            RemoveEndedDeals(phase);
        }

        private void Enter(bool condition, Phase phaseIfTrue, Phase phaseIfFalse)
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

        private void Enter(bool condition, Phase phaseIfTrue, Action methodOtherwise)
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

        private void Enter(bool condition, Action methodIfTrue, Action methodOtherwise)
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

        private void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, Action methodOtherwise)
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

        private void Enter(bool condition1, Phase phaseIf1True, bool condition2, Action methodIf2True, Action methodOtherwise)
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

        private void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, Phase phaseOtherwise)
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

        private void Enter(bool condition1, Action methodIf1True, bool condition2, Phase phaseIf2True, Phase phaseOtherwise)
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

        private void RegisterKnown(TreacheryCard c)
        {
            foreach (var p in Players)
            {
                RegisterKnown(p, c);
            }
        }

        private static void RegisterKnown(Player p, TreacheryCard c)
        {
            if (c != null && !p.KnownCards.Contains(c))
            {
                p.KnownCards.Add(c);
            }
        }

        private void RegisterKnown(Faction f, TreacheryCard c)
        {
            var p = GetPlayer(f);
            if (p != null)
            {
                RegisterKnown(p, c);
            }
        }

        private void UnregisterKnown(TreacheryCard c)
        {
            foreach (var p in Players)
            {
                UnregisterKnown(p, c);
            }
        }

        private static void UnregisterKnown(Player p, TreacheryCard c)
        {
            p.KnownCards.Remove(c);
        }

        private static void UnregisterKnown(Player p, IEnumerable<TreacheryCard> cards)
        {
            foreach (var c in cards)
            {
                UnregisterKnown(p, c);
            }
        }

        #endregion

        #region Forces

        public bool IsInStorm(Location l) => l.Sector == SectorInStorm;

        public bool IsOccupied(Location l) => Players.Any(p => p.Occupies(l));

        public bool IsOccupied(Territory t) => Players.Any(p => p.Occupies(t));

        public Dictionary<Location, List<Battalion>> Forces(bool includeHomeworlds = false)
        {
            Dictionary<Location, List<Battalion>> result = new();

            foreach (var l in Map.Locations())
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
            if (OccupyingForcesOnPlanet.ContainsKey(l))
            {
                return OccupyingForcesOnPlanet[l].Where(of => of.Faction != p.Faction).Count();
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
            foreach (Location l in t.Locations)
            {
                if (OccupyingForcesOnPlanet.ContainsKey(l))
                {
                    foreach (var b in OccupyingForcesOnPlanet[l])
                    {
                        if (b.Faction != p.Faction && !counted.Contains(b.Faction))
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
            CurrentFlightUsed != null && CurrentFlightUsed.MoveThreeTerritories;

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
            return Map.Locations().Where(l => l.Sector != SectorInStorm && p.AnyForcesIn(l) > 0);
        }

        private void FlipBeneGesseritWhenAlone()
        {
            var bg = GetPlayer(Faction.Blue);
            if (bg != null)
            {
                var territoriesWhereAdvisorsAreAlone = Map.Territories(true).Where(t => bg.SpecialForcesIn(t) > 0 && !Players.Any(p => p.Faction != Faction.Blue && p.AnyForcesIn(t) > 0));
                foreach (var t in territoriesWhereAdvisorsAreAlone)
                {
                    bg.FlipForces(t, false);
                    Log(Faction.Blue, " are alone and flip to ", FactionForce.Blue, " in ", t);
                }
            }
        }

        #endregion

        #region Resources

        private void ChangeResourcesOnPlanet(Location location, int amount)
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
            else
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

        public Player GetPlayer(Faction f)
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
            return (!l.Territory.IsStronghold || (p.Is(Faction.Blue) && p.SpecialForcesIn(l) > 0) || NrOfOccupantsExcludingPlayer(l, p) < 2);
        }

        public static Payment Payment(int amount)
        {
            return new Payment() { Amount = amount };
        }

        public static Payment Payment(int amount, Faction by)
        {
            return new Payment() { Amount = amount, By = by };
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
            result.LeaderState = Utilities.CloneDictionary(LeaderState);
            result.SecretsRemainHidden = new List<Faction>(SecretsRemainHidden);
            result.PreventedAdvantages = new List<FactionAdvantage>(PreventedAdvantages);
            return result;
        }

        private bool EveryoneActedOrPassed => HasActedOrPassed.Count == Players.Count;

        public bool AssistedNotekeepingEnabled(Player p) => Applicable(Rule.AssistedNotekeeping) || p.Is(Faction.Green) && Applicable(Rule.AssistedNotekeepingForGreen);

        public bool HasStormPrescience(Player p)
        {
            return
                p != null &&
                Applicable(Rule.YellowSeesStorm) &&
                !Prevented(FactionAdvantage.YellowStormPrescience) &&
                (p.Faction == Faction.Yellow || (p.Ally == Faction.Yellow && YellowSharesPrescience) || HasDeal(p.Faction, DealType.ShareStormPrescience));
        }

        public bool HasResourceDeckPrescience(Player p)
        {
            return
                p != null &&
                !Prevented(FactionAdvantage.GreenSpiceBlowPrescience) &&
                (p.Faction == Faction.Green || (p.Ally == Faction.Green && GreenSharesPrescience) || HasDeal(p.Faction, DealType.ShareResourceDeckPrescience));
        }

        public bool HasBiddingPrescience(Player p)
        {
            bool isPubliclyKnown = CurrentAuctionType == AuctionType.WhiteOnceAround || CurrentAuctionType == AuctionType.WhiteSilent;

            return
                isPubliclyKnown ||
                (p != null &&
                !Prevented(FactionAdvantage.GreenBiddingPrescience) &&
                (p.Faction == Faction.Green || (p.Ally == Faction.Green && GreenSharesPrescience) || HasDeal(p.Faction, DealType.ShareBiddingPrescience)));
        }

        public HomeworldStatus GetStatusOf(Homeworld w)
        {
            var player = Players.FirstOrDefault(p => p.Homeworlds.Contains(w));

            if (player != null)
            {
                int nrOfForces = 0;
                if (w.IsHomeOfNormalForces) nrOfForces += player.ForcesInReserve;
                if (w.IsHomeOfSpecialForces) nrOfForces += player.SpecialForcesInReserve;

                if (nrOfForces >= w.Threshold)
                {
                    return new HomeworldStatus(true, player.Faction);
                }
                else
                {
                    var enemyOccupant = BattalionsIn(w).FirstOrDefault(b => b.Faction != player.Faction);
                    if (enemyOccupant != null)
                    {
                        return new HomeworldStatus(false, enemyOccupant.Faction);
                    }
                    else
                    {
                        return new HomeworldStatus(false, player.Faction);
                    }
                }
            }

            return null;
        }

        public int NumberOfHumanPlayers => Players.Count(p => !p.IsBot);

        private TreacheryCard Discard(Player player, TreacheryCardType cardType)
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

        private void Discard(TreacheryCard card)
        {
            var player = Players.SingleOrDefault(p => p.TreacheryCards.Contains(card));
            Discard(player, card);
            RegisterKnown(card);
        }

        public Dictionary<TreacheryCard, Faction> RecentlyDiscarded { get; private set; } = new Dictionary<TreacheryCard, Faction>();
        private void Discard(Player player, TreacheryCard card)
        {
            if (player != null && card != null)
            {
                Log(player.Faction, " discard ", card);
                player.TreacheryCards.Remove(card);
                TreacheryDiscardPile.PutOnTop(card);
                RegisterKnown(card);
                RecentlyDiscarded.Add(card, player.Faction);
            }
        }

        public Player OwnerOf(TreacheryCard karmaCard)
        {
            return karmaCard != null ? Players.FirstOrDefault(p => p.TreacheryCards.Contains(karmaCard)) : null;
        }

        public Player OwnerOf(TreacheryCardType cardType)
        {
            return Players.FirstOrDefault(p => p.TreacheryCards.Any(c => c.Type == cardType));
        }

        public Player OwnerOf(Location stronghold)
        {
            return StrongholdOwnership.ContainsKey(stronghold) ? GetPlayer(StrongholdOwnership[stronghold]) : null;
        }

        public static Message TryLoad(GameState state, bool performValidation, bool isHost, out Game result, bool trackStatesForReplay)
        {
            try
            {
                result = new Game(state.Version, trackStatesForReplay);

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

        #endregion SupportMethods

        

    }
}
