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
        public const int LatestVersion = 131;

        public bool BotInfologging = false;

        #region GameState

        public int Seed { get; private set; } = -1;
        public int MaximumNumberOfTurns { get; private set; }
        public int MaximumNumberOfPlayers { get; private set; }
        public string Name { get; private set; }
        public IList<Milestone> RecentMilestones { get; private set; } = new List<Milestone>();
        public int Version { get; private set; }
        public Map Map { get; private set; } = new Map();
        public List<Rule> Rules { get; private set; } = new List<Rule>();
        public List<Rule> RulesForBots { get; private set; } = new List<Rule>();
        public List<Rule> AllRules { get; private set; } = new List<Rule>();
        public IList<GameEvent> History { get; private set; } = new List<GameEvent>();
        public bool TrackStatesForReplay { get; private set; } = true;
        public IList<Game> States { get; private set; } = new List<Game>();
        public int CurrentTurn { get; private set; } = 0;
        public MainPhase CurrentMainPhase { get; private set; } = MainPhase.Started;
        public MainPhaseMoment CurrentMoment { get; private set; } = MainPhaseMoment.None;
        public Phase CurrentPhase { get; private set; } = Phase.None;
        public IList<Faction> HasActedOrPassed { get; private set; } = new List<Faction>();
        public IList<Player> Players { get; private set; } = new List<Player>();
        public Report CurrentReport { get; private set; }
        public Deck<TreacheryCard> TreacheryDeck { get; private set; }
        public Deck<TreacheryCard> TreacheryDiscardPile { get; private set; }
        public List<TreacheryCard> RemovedTreacheryCards { get; private set; } = new List<TreacheryCard>();
        public List<TreacheryCard> WhiteCache { get; private set; } = new List<TreacheryCard>();
        public Deck<ResourceCard> ResourceCardDeck { get; private set; }
        public Deck<ResourceCard> ResourceCardDiscardPileA { get; private set; }
        public Deck<ResourceCard> ResourceCardDiscardPileB { get; private set; }
        public int SectorInStorm { get; private set; } = -1;
        public int NextStormMoves { get; private set; } = -1;
        public bool ShieldWallDestroyed { get; private set; } = false;
        public BrownEconomicsStatus EconomicsStatus { get; private set; } = BrownEconomicsStatus.None;
        public IDictionary<Location, int> ResourcesOnPlanet { get; private set; } = new Dictionary<Location, int>();
        public IDictionary<IHero, LeaderState> LeaderState { get; private set; } = new Dictionary<IHero, LeaderState>();
        public Deck<LeaderSkill> SkillDeck { get; private set; }
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

        public void PerformPreEventTasks()
        {
            RecentlyDiscarded.Clear();
        }

        public void PerformPostEventTasks(GameEvent e, bool justEnteredStartOfPhase)
        {
            if (!justEnteredStartOfPhase && !(e is AllyPermission) && !(e is DealOffered) && !(e is DealAccepted)) MainPhaseMiddle();

            History.Add(e);

            if (TrackStatesForReplay && !Applicable(Rule.DisableEndOfGameReport))
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

        public static string TryLoad(GameState state, bool performValidation, bool isHost, ref Game result, bool trackStatesForReplay)
        {
            try
            {
                result = new Game(state.Version, trackStatesForReplay);
                string message;

                int nr = 0;
                foreach (var e in state.Events)
                {
                    e.Game = result;
                    message = e.Execute(performValidation, isHost);
                    if (message != "")
                    {
                        return string.Format("{0} ({1}): {2}", e.GetType().Name, nr, message);
                    }
                    nr++;
                }

                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public int EventCount
        {
            get
            {
                return History.Count;
            }
        }

        public void HandleEvent(EndPhase e)
        {
            switch (CurrentPhase)
            {
                case Phase.SelectingFactions:
                    AssignFactionsAndEnterFactionTrade();
                    break;

                case Phase.TradingFactions:
                    EnterSetupPhase();
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

            List<FactionAdvantage> expectionsToAllowing = new List<FactionAdvantage>();
            if (CurrentMainPhase == MainPhase.Bidding && Prevented(FactionAdvantage.PurpleIncreasingRevivalLimits))
            {
                expectionsToAllowing.Add(FactionAdvantage.PurpleIncreasingRevivalLimits);
            }
            if (Version >= 103) AllowAllPreventedFactionAdvantages(expectionsToAllowing);
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

        private void Enter(bool condition, Phase phaseIfTrue, EnterPhaseMethod methodOtherwise)
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

        private void Enter(bool condition, EnterPhaseMethod methodIfTrue, EnterPhaseMethod methodOtherwise)
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

        private void Enter(bool condition1, Phase phaseIf1True, bool condition2, Phase phaseIf2True, EnterPhaseMethod methodOtherwise)
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

        private void Enter(bool condition1, Phase phaseIf1True, bool condition2, EnterPhaseMethod methodIf2True, EnterPhaseMethod methodOtherwise)
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

        private void Enter(bool condition1, EnterPhaseMethod methodIf1True, bool condition2, Phase phaseIf2True, Phase phaseOtherwise)
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

        private void RegisterKnown(Player p, TreacheryCard c)
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

        private void UnregisterKnown(Player p, TreacheryCard c)
        {
            p.KnownCards.Remove(c);
        }

        private void UnregisterKnown(Player p, IEnumerable<TreacheryCard> cards)
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

        public Dictionary<Location, List<Battalion>> ForcesOnPlanet
        {
            get
            {
                Dictionary<Location, List<Battalion>> result = new Dictionary<Location, List<Battalion>>();

                foreach (var l in Map.Locations)
                {
                    result.Add(l, new List<Battalion>());
                }

                foreach (var p in Players)
                {
                    foreach (var locationAndBattaltion in p.ForcesOnPlanet)
                    {
                        result[locationAndBattaltion.Key].Add(locationAndBattaltion.Value);
                    }
                }

                return result;
            }
        }

        public Dictionary<Location, List<Battalion>> ForcesOnPlanetExcludingEmptyLocations
        {
            get
            {
                Dictionary<Location, List<Battalion>> result = new Dictionary<Location, List<Battalion>>();

                foreach (var p in Players)
                {
                    foreach (var locationAndBattaltion in p.ForcesOnPlanet)
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

        public bool AnyForcesIn(Territory t)
        {
            foreach (var p in Players)
            {
                foreach (var l in t.Locations)
                {
                    if (p.ForcesOnPlanet.ContainsKey(l))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Dictionary<Location, List<Battalion>> OccupyingForcesOnPlanet
        {
            get
            {
                Dictionary<Location, List<Battalion>> result = new Dictionary<Location, List<Battalion>>();

                foreach (var p in Players)
                {
                    foreach (var locationAndBattaltion in p.ForcesOnPlanet.Where(kvp => p.Occupies(kvp.Key)))
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
            return Map.Locations.Where(l => l.Sector != SectorInStorm && p.AnyForcesIn(l) > 0);
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

        #region SupportMethods

        private Payment Payment(int amount)
        {
            return new Payment() { Amount = amount };
        }

        private Payment Payment(int amount, Faction by)
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
            return result;
        }

        public bool IsPlaying(Faction faction)
        {
            return Players.Any(p => p.Faction == faction);
        }

        private bool EveryoneActedOrPassed => HasActedOrPassed.Count == Players.Count;


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

        public Player GetPlayer(string name)
        {
            return Players.FirstOrDefault(p => p.Name == name);
        }

        public Player GetPlayer(Faction f)
        {
            return Players.FirstOrDefault(p => p.Faction == f);
        }

        public IEnumerable<Faction> ValidTargets(Player p)
        {
            return Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

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
                CurrentReport.Express(cardType, " card not found");
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
                CurrentReport.Express(player.Faction, " discard ", card);
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

        private void FlipBeneGesseritWhenAlone()
        {
            var bg = GetPlayer(Faction.Blue);
            if (bg != null)
            {
                var territoriesWhereAdvisorsAreAlone = Map.Territories.Where(t => bg.SpecialForcesIn(t) > 0 && !Players.Any(p => p.Faction != Faction.Blue && p.AnyForcesIn(t) > 0));
                foreach (var t in territoriesWhereAdvisorsAreAlone)
                {
                    bg.FlipForces(t, false);
                    CurrentReport.Express(Faction.Blue, " are alone and flip to ", FactionForce.Blue, " in ", t);
                }
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

        public override string ToString()
        {
            return Skin.Current.Format("Players: {0}, Phase: {1}", Players.Count, CurrentPhase);
        }

        #endregion SupportMethods
    }
}
