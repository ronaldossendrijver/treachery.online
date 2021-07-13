/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public const int LowestSupportedVersion = 68;
        public const int LatestVersion = 104;

        public bool BotInfologging = false;

        #region GameState

        public int MaximumNumberOfTurns = 10;
        public int MaximumNumberOfPlayers = 6;

        public int Seed = -1;
        public string Name;
        private Random Random { get; set; }
        public IList<Milestone> RecentMilestones { get; private set; } = new List<Milestone>();
        public int Version { get; set; }
        public Map Map { get; set; } = new Map();
        public Rule[] Rules { get; set; } = new Rule[0];
        public Rule[] RulesForBots { get; set; } = new Rule[0];
        public Rule[] AllRules { get; set; } = new Rule[0];
        public IList<GameEvent> History { get; set; } = new List<GameEvent>();
        public IList<Game> States { get; set; } = new List<Game>();
        public int CurrentTurn { get; set; } = 0;
        public MainPhase CurrentMainPhase { get; set; } = MainPhase.Started;
        public MainPhaseMoment CurrentMoment { get; set; } = MainPhaseMoment.None;
        public Phase CurrentPhase { get; set; } = Phase.None;
        public IList<Faction> HasActedOrPassed { get; set; } = new List<Faction>();
        public IList<Player> Players { get; set; } = new List<Player>();
        public Report CurrentReport { get; set; }
        public PlayerSequence ShipmentAndMoveSequence { get; set; }
        public PlayerSequence BattleSequence { get; set; }
        public PlayerSequence BidSequence { get; set; }
        public PlayerSequence CheckWinSequence { get; set; }
        public PlayerSequence TechTokenSequence { get; set; }
        public Deck<TreacheryCard> TreacheryDeck { get; set; }
        public Deck<TreacheryCard> TreacheryDiscardPile { get; set; }
        public Deck<TreacheryCard> WhiteDeck { get; set; }
        public Deck<ResourceCard> ResourceCardDeck { get; set; }
        public Deck<ResourceCard> ResourceCardDiscardPileA { get; set; }
        public Deck<ResourceCard> ResourceCardDiscardPileB { get; set; }
        public int SectorInStorm { get; set; } = -1;
        public int NextStormMoves { get; set; } = -1;
        public bool ShieldWallDestroyed { get; set; } = false;
        public int FirstPlayerPosition { get; set; } = -1;
        public BrownEconomicsStatus EconomicsStatus { get; set; } = BrownEconomicsStatus.None;
        public IDictionary<Location, int> ResourcesOnPlanet { get; set; } = new Dictionary<Location, int>();
        public IDictionary<IHero, LeaderState> LeaderState { get; set; } = new Dictionary<IHero, LeaderState>();

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
                LeaderState.Add(l, new LeaderState() { DeathCounter = 0, CurrentTerritory = null });
            }
            LeaderState.Add(LeaderManager.Messiah, new LeaderState() { DeathCounter = 0, CurrentTerritory = null });
        }

        #endregion Initialization

        #region EventHandling

        public void PerformPostEventTasks(GameEvent e)
        {
            MainPhaseMiddle();
            History.Add(e);
            States.Add(Clone());
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

        public static string TryLoad(GameState state, bool performValidation, bool isHost, ref Game result)
        {
            try
            {
                result = new Game(state.Version);
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
                    EnterCharityPhase();
                    break;

                case Phase.ClaimingCharity:
                    EnterBiddingPhase();
                    break;

                case Phase.WaitingForNextBiddingRound:
                    PutNextCardOnAuction();
                    break;

                case Phase.BiddingReport:
                    EnterRevivalPhase();
                    break;

                case Phase.Resurrection:
                    EnterShipmentAndMovePhase();
                    break;

                case Phase.ShipmentAndMoveConcluded:
                    EnterBattlePhase();
                    break;

                case Phase.BattleReport:
                    ResetBattle();
                    Enter(Aggressor != null, Phase.BattlePhase, EnterSpiceCollectionPhase);
                    break;

                case Phase.CollectionReport:
                    EnterMentatPhase();
                    break;

                case Phase.Contemplate:
                    ContinueMentatPhase();
                    break;

                case Phase.TurnConcluded:
                    AddBribesToPlayerResources();
                    EnterStormPhase();
                    break;
            }
        }

        public event EventHandler<ChatMessage> MessageHandler;

        public void SendMessage(ChatMessage message)
        {
            MessageHandler?.Invoke(this, message);
        }

        #endregion EventHandling

        #region PhaseTransitions

        private void MainPhaseStart(MainPhase phase, bool clearReport = true)
        {
            CurrentMainPhase = phase;
            CurrentMoment = MainPhaseMoment.Start;
            if (clearReport) CurrentReport = new Report(phase);
            CurrentKarmaPrevention = null;
        }

        private void MainPhaseMiddle()
        {
            if (CurrentMoment == MainPhaseMoment.Start) CurrentMoment = MainPhaseMoment.Middle;
        }

        private void MainPhaseEnd()
        {
            CurrentMoment = MainPhaseMoment.End;
            if (Version >= 103) AllowAllPreventedFactionAdvantages();
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

        #region LeaderState

        public void KillHero(IHero l)
        {
            if (l is Leader || l is Messiah)
            {
                LeaderState[l].Kill();

                if (Version >= 46)
                {
                    ReturnGholaToOriginalFaction(l);
                }
            }
        }

        private void ReturnGholaToOriginalFaction(IHero l)
        {
            var purple = GetPlayer(Faction.Purple);
            if (purple != null && l is Leader && purple.Leaders.Contains(l) && l.Faction != Faction.Purple)
            {
                var originalOwner = GetPlayer(l.Faction);
                purple.Leaders.Remove(l as Leader);
                originalOwner.Leaders.Add(l as Leader);
            }
        }

        public void AssassinateLeader(Leader l)
        {
            LeaderState[l].Assassinate();
        }


        public bool IsAlive(IHero l)
        {
            return LeaderState[l].Alive;
        }

        public bool MessiahIsAlive => IsAlive(LeaderManager.Messiah);

        public Territory CurrentTerritory(IHero l)
        {
            return LeaderState[l].CurrentTerritory;
        }

        public bool IsFaceUp(IHero l)
        {
            return !LeaderState[l].IsFaceDownDead;
        }

        public Player GetPlayer(Faction f)
        {
            return Players.FirstOrDefault(p => p.Faction == f);
        }

        #endregion

        #region MapInfo

        public bool IsInStorm(Location l)
        {
            return l.Sector == SectorInStorm;
        }

        public bool IsOccupied(Location l)
        {
            return Players.Any(p => p.Occupies(l));
        }

        public bool IsOccupied(Territory t)
        {
            return Players.Any(p => p.Occupies(t));
        }

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

        public List<Battalion> GetOccupyingForces(Location l)
        {
            if (OccupyingForcesOnPlanet.ContainsKey(l))
            {
                return OccupyingForcesOnPlanet[l];
            }
            else
            {
                return new List<Battalion>();
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

        public int DetermineMaximumMoveDistance(Player p, IEnumerable<Battalion> moved)
        {
            bool hasOrnithopters = Applicable(Rule.MovementBonusRequiresOccupationBeforeMovement) ? FactionsWithOrnithoptersAtStartOfMovement.Contains(p.Faction) : OccupiesArrakeenOrCarthag(p);
            int brownExtraMoveBonus = p.Faction == Faction.Brown && BrownHasExtraMove ? 1 : 0;

            int result = 1;

            if (hasOrnithopters)
            {
                result = 3;
            }
            else if (p.Is(Faction.Yellow) && !Prevented(FactionAdvantage.YellowExtraMove))
            {
                result = 2;
            }
            else if (p.Is(Faction.Grey) && !Prevented(FactionAdvantage.GreyCyborgExtraMove) && moved.Any(b => b.AmountOfSpecialForces > 0))
            {
                result = 2;
            }

            return result + brownExtraMoveBonus;
        }

        public IEnumerable<Location> LocationsWithAnyForcesNotInStorm(Player p)
        {
            return Map.Locations.Where(l => l.Sector != SectorInStorm && p.AnyForcesIn(l) > 0);
        }

        #endregion

        #region Resources

        public void ChangeSpiceOnPlanet(Location location, int amount)
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

        private void AllowAllPreventedFactionAdvantages()
        {
            foreach (var adv in Enumerations.GetValuesExceptDefault(typeof(FactionAdvantage), FactionAdvantage.None))
            {
                Allow(adv);
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

        public bool EveryoneButOneActedOrPassed => HasActedOrPassed.Count == Players.Count - 1;

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
            return
                p != null &&
                !Prevented(FactionAdvantage.GreenBiddingPrescience) &&
                (p.Faction == Faction.Green || (p.Ally == Faction.Green && GreenSharesPrescience) || HasDeal(p.Faction, DealType.ShareBiddingPrescience));
        }

        public Player GetPlayer(string name)
        {
            return Players.FirstOrDefault(p => p.Name == name);
        }

        public override string ToString()
        {
            return Skin.Current.Format("Players: {0}, Phase: {1}", Players.Count, CurrentPhase);
        }

        private TreacheryCard DiscardTreacheryCard(Player player, TreacheryCardType cardType)
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
                player.TreacheryCards.Remove(card);
                TreacheryDiscardPile.PutOnTop(card);
            }
            else
            {
                CurrentReport.Add("{0} not found", cardType);
            }

            return card;
        }

        private void Discard(TreacheryCard card)
        {
            var player = Players.SingleOrDefault(p => p.TreacheryCards.Contains(card));
            Discard(player, card);
            RegisterKnown(card);
        }
        
        private void Discard(Player player, TreacheryCard card)
        {
            if (player != null && card != null)
            {
                CurrentReport.Add(player.Faction, "{0} discard {1}.", player.Faction, card);
                player.TreacheryCards.Remove(card);
                TreacheryDiscardPile.PutOnTop(card);
                RegisterKnown(card);
            }
        }

        public Player OwnerOf(TreacheryCard karmaCard)
        {
            return Players.FirstOrDefault(p => p.TreacheryCards.Contains(karmaCard));
        }

        private bool HasSomethingToRevive(Player player)
        {
            if (player.ForcesKilled > 0 || player.SpecialForcesKilled > 0 || Revival.ValidRevivalHeroes(this, player).Any())
            {
                return true;
            }
            else if (player.Is(Faction.Purple) && player.Ally != Faction.None)
            {
                var ally = GetPlayer(player.Ally);
                return HasSomethingToRevive(ally);
            }

            return false;
        }

        #endregion SupportMethods

        #region Validation

        public IEnumerable<Faction> ValidTargets(Player p)
        {
            return Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

        public IEnumerable<IHero> ValidFreeRevivalHeroes(Player p)
        {
            var result = new List<IHero>();
            result.AddRange(p.Leaders.Where(l => !IsAlive(l)));

            if (p.Is(Faction.Green) && !IsAlive(LeaderManager.Messiah))
            {
                result.Add(LeaderManager.Messiah);
            }

            return result;
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

        #endregion Validation
    }
}
