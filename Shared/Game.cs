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
        public const int LatestVersion = 103;

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
        public Deck<ResourceCard> ResourceCardDeck { get; set; }
        public Deck<ResourceCard> ResourceCardDiscardPileA { get; set; }
        public Deck<ResourceCard> ResourceCardDiscardPileB { get; set; }
        public int SectorInStorm { get; set; } = -1;
        public int NextStormMoves { get; set; } = -1;
        public bool ShieldWallDestroyed { get; set; } = false;
        public int FirstPlayerPosition { get; set; } = -1;
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

        public void AddEvent(GameEvent e)
        {
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
        
        #endregion EventHandling

        #region EventValidity
        public IList<Type> GetApplicableEvents(Player player, bool isHost)
        {
            List<Type> result = new List<Type>();

            if (isHost)
            {
                AddHostActions(result);
            }

            if (player != null && (CurrentPhase == Phase.SelectingFactions || player.Faction != Faction.None))
            {
                AddPlayerActions(player, isHost, result);
            }

            return new List<Type>(result);
        }

        private void AddHostActions(List<Type> result)
        {
            switch (CurrentPhase)
            {
                case Phase.AwaitingPlayers:
                    result.Add(typeof(EstablishPlayers));
                    break;
                case Phase.SelectingFactions:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.TradingFactions:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.PerformCustomSetup:
                    result.Add(typeof(PerformSetup));
                    break;
                case Phase.HmsPlacement:
                    if (!IsPlaying(Faction.Grey) && Applicable(Rule.HMSwithoutGrey)) result.Add(typeof(PerformHmsPlacement));
                    break;
                case Phase.MetheorAndStormSpell:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.StormReport:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.Thumper:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.HarvesterA:
                case Phase.HarvesterB:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.AllianceA:
                case Phase.AllianceB:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.BlowReport:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.ClaimingCharity:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.WaitingForNextBiddingRound:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.BiddingReport:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.Resurrection:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.ShipmentAndMoveConcluded:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.BattleReport:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.CollectionReport:
                    result.Add(typeof(EndPhase));
                    break;
                case Phase.TurnConcluded:
                    result.Add(typeof(EndPhase));
                    break;
            }

            if (CurrentMainPhase >= MainPhase.Setup)
            {
                result.Add(typeof(PlayerReplaced));
            }
        }

        private void AddPlayerActions(Player player, bool isHost, List<Type> result)
        {
            var faction = player.Faction;

            switch (CurrentPhase)
            {
                case Phase.SelectingFactions:
                    if (player.Faction == Faction.None) result.Add(typeof(FactionSelected));
                    break;
                case Phase.DiallingStorm:
                    if (HasBattleWheel.Contains(player.Faction) && !HasActedOrPassed.Contains(player.Faction)) result.Add(typeof(StormDialled));
                    break;
                case Phase.TradingFactions:
                    if (Players.Count > 1) result.Add(typeof(FactionTradeOffered));
                    break;
                case Phase.PerformCustomSetup:
                    break;
                case Phase.BluePredicting:
                    if (faction == Faction.Blue) result.Add(typeof(BluePrediction));
                    break;
                case Phase.YellowSettingUp:
                    if (faction == Faction.Yellow) result.Add(typeof(PerformYellowSetup));
                    break;
                case Phase.BlueSettingUp:
                    if (faction == Faction.Blue) result.Add(typeof(PerformBluePlacement));
                    break;
                case Phase.BlackMulligan:
                    if (faction == Faction.Black) result.Add(typeof(MulliganPerformed));
                    break;
                case Phase.SelectingTraitors:
                    if (faction != Faction.Black && faction != Faction.Purple && !HasActedOrPassed.Contains(faction)) result.Add(typeof(TraitorsSelected));
                    break;
                case Phase.GreySelectingCard:
                    if (faction == Faction.Grey) result.Add(typeof(GreySelectedStartingCard));
                    break;
                case Phase.HmsPlacement:
                    if (faction == Faction.Grey) result.Add(typeof(PerformHmsPlacement));
                    break;
                case Phase.HmsMovement:
                    if (faction == Faction.Grey) result.Add(typeof(PerformHmsMovement));
                    break;
                case Phase.MetheorAndStormSpell:
                    if (player.Has(TreacheryCardType.StormSpell) && CurrentTurn > 1) result.Add(typeof(StormSpellPlayed));
                    if (MetheorPlayed.MayPlayMetheor(this, player)) result.Add(typeof(MetheorPlayed));
                    break;
                case Phase.Thumper:
                    if (player.Has(TreacheryCardType.Thumper) && CurrentTurn > 1) result.Add(typeof(ThumperPlayed));
                    if (player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.HarvesterA:
                case Phase.HarvesterB:
                    if (player.Has(TreacheryCardType.Harvester)) result.Add(typeof(HarvesterPlayed));
                    break;
                case Phase.StormLosses:
                    if (faction == Faction.Yellow) result.Add(typeof(TakeLosses));
                    break;
                case Phase.YellowSendingMonsterA:
                case Phase.YellowSendingMonsterB:
                    if (faction == Faction.Yellow) result.Add(typeof(YellowSentMonster));
                    break;
                case Phase.YellowRidingMonsterA:
                case Phase.YellowRidingMonsterB:
                    if (faction == Faction.Yellow) result.Add(typeof(YellowRidesMonster));
                    break;
                case Phase.AllianceA:
                case Phase.AllianceB:
                    if (player.Ally == Faction.None && Players.Count > 1) result.Add(typeof(AllianceOffered));
                    if (player.Ally != Faction.None) result.Add(typeof(AllianceBroken));
                    break;
                case Phase.BlowReport:
                    break;
                case Phase.ClaimingCharity:
                    if (!isHost && faction == Faction.Green) result.Add(typeof(EndPhase));
                    //if (player.Resources <= 1 && !(faction == Faction.Blue && !Prevented(FactionAdvantage.BlueCharity) && Applicable(Rule.BlueAutoCharity))) result.Add(typeof(CharityClaimed));
                    if (player.Resources <= 1 && !HasActedOrPassed.Contains(faction)) result.Add(typeof(CharityClaimed));
                    if (player.Has(TreacheryCardType.Amal) && (Version <= 82 || HasActedOrPassed.Count == 0)) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.Bidding:
                    if (player == BidSequence.CurrentPlayer)
                    {
                        result.Add(typeof(Bid));
                    }
                    if (player.Has(TreacheryCardType.Amal) && CardNumber == 1 && !Bids.Any()) result.Add(typeof(AmalPlayed));
                    if (faction == Faction.Red && Applicable(Rule.RedSupportingNonAllyBids)) result.Add(typeof(RedBidSupport));
                    break;
                case Phase.GreyRemovingCardFromBid:
                    if (faction == Faction.Grey) result.Add(typeof(GreyRemovedCardFromAuction));
                    if (player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.GreySwappingCard:
                    if (faction == Faction.Grey) result.Add(typeof(GreySwappedCardOnBid));
                    if (player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.ReplacingCardJustWon:
                    if (player.Ally == Faction.Grey) result.Add(typeof(ReplacedCardWon));
                    break;
                case Phase.WaitingForNextBiddingRound:
                    if (!isHost && faction == Faction.Green) result.Add(typeof(EndPhase));
                    if (Version < 46 && faction == Faction.Grey) result.Add(typeof(GreySwappedCardOnBid));
                    break;
                case Phase.BiddingReport:
                    if (faction == Faction.Purple && Players.Count > 1) result.Add(typeof(SetIncreasedRevivalLimits));
                    break;
                case Phase.Resurrection:
                    if (IsPlaying(Faction.Purple) && faction != Faction.Purple && 
                        (Version <= 78 || !HasActedOrPassed.Contains(faction)) && 
                        ValidFreeRevivalHeroes(player).Any() && 
                        (Version < 50 || !Revival.NormallyRevivableHeroes(this, player).Any()) &&
                        (Version < 102 || CurrentPurpleRevivalRequest == null)) result.Add(typeof(RequestPurpleRevival));

                    if (!HasActedOrPassed.Contains(faction) && HasSomethingToRevive(player)) result.Add(typeof(Revival));
                    if (faction == Faction.Purple && Players.Count > 1) result.Add(typeof(SetIncreasedRevivalLimits));
                    if (faction == Faction.Purple && (CurrentPurpleRevivalRequest != null || AllowedEarlyRevivals.Any())) result.Add(typeof(AcceptOrCancelPurpleRevival));
                    if (player.Has(TreacheryCardType.Amal) && (Version <= 82 || HasActedOrPassed.Count == 0)) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.OrangeShip:
                    if (faction == Faction.Orange)
                    {
                        if (!EveryoneButOneActedOrPassed && Applicable(Rule.OrangeDetermineShipment)) result.Add(typeof(OrangeDelay));
                        result.Add(typeof(Shipment));
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan)) result.Add(typeof(Caravan));
                    }
                    if (Version <= 96 && player.Has(TreacheryCardType.Amal) && HasActedOrPassed.Count == 0) result.Add(typeof(AmalPlayed));
                    if (Version >= 97 && player.Has(TreacheryCardType.Amal) && BeginningOfShipmentAndMovePhase) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.BlueAccompaniesOrange:
                    if (faction == Faction.Blue) result.Add(typeof(BlueAccompanies));
                    break;
                case Phase.BlueAccompaniesNonOrange:
                    if (faction == Faction.Blue) result.Add(typeof(BlueAccompanies));
                    break;
                case Phase.NonOrangeShip:
                    if (player == ShipmentAndMoveSequence.CurrentPlayer)
                    {
                        result.Add(typeof(Shipment));
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan)) result.Add(typeof(Caravan));
                    }
                    if (Version <= 96 && player.Has(TreacheryCardType.Amal) && HasActedOrPassed.Count == 0) result.Add(typeof(AmalPlayed));
                    if (Version >= 97 && player.Has(TreacheryCardType.Amal) && BeginningOfShipmentAndMovePhase) result.Add(typeof(AmalPlayed));
                    break;
                case Phase.OrangeMove:
                    if (faction == Faction.Orange)
                    {
                        result.Add(typeof(Move));
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan)) result.Add(typeof(Caravan));
                    }
                    break;
                case Phase.NonOrangeMove:
                    if (player == ShipmentAndMoveSequence.CurrentPlayer)
                    {
                        result.Add(typeof(Move));
                        if (player.TreacheryCards.Any(c => c.Type == TreacheryCardType.Caravan)) result.Add(typeof(Caravan));
                    }
                    break;
                case Phase.BlueIntrudedByOrangeShip:
                case Phase.BlueIntrudedByNonOrangeShip:
                case Phase.BlueIntrudedByOrangeMove:
                case Phase.BlueIntrudedByNonOrangeMove:
                case Phase.BlueIntrudedByYellowRidingMonsterA:
                case Phase.BlueIntrudedByYellowRidingMonsterB:
                case Phase.BlueIntrudedByCaravan:
                    if (faction == Faction.Blue) result.Add(typeof(BlueFlip));
                    break;
                case Phase.ShipmentAndMoveConcluded:
                    if (player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    break;

                case Phase.BattlePhase:
                    {
                        if (CurrentBattle == null && player == Aggressor)
                        {
                            result.Add(typeof(BattleInitiated));
                        }

                        if (CurrentBattle != null && player == Aggressor && AggressorBattleAction == null)
                        {
                            result.Add(typeof(Battle));
                        }
                        else if (CurrentBattle != null && faction == CurrentBattle.Target && DefenderBattleAction == null)
                        {
                            result.Add(typeof(Battle));
                        }

                        if (CurrentBattle != null && player == Aggressor && AggressorBattleAction != null)
                        {
                            result.Add(typeof(BattleRevision));
                        }
                        else if (CurrentBattle != null && faction == CurrentBattle.Target && DefenderBattleAction != null)
                        {
                            result.Add(typeof(BattleRevision));
                        }

                        if (Voice.MayUseVoice(this, player))
                        {
                            result.Add(typeof(Voice));
                        }

                        if (Prescience.MayUsePrescience(this, player))
                        {
                            result.Add(typeof(Prescience));
                        }

                        if (player.Has(TreacheryCardType.Amal) && NrOfBattlesFought == 0) result.Add(typeof(AmalPlayed));
                    }
                    break;

                case Phase.CallTraitorOrPass:
                    if (AggressorBattleAction != null && DefenderBattleAction != null &&
                            (AggressorTraitorAction == null && player == Aggressor ||
                             AggressorTraitorAction == null && faction == Faction.Black && GetPlayer(AggressorBattleAction.Initiator).Ally == Faction.Black && player.Traitors.Contains(DefenderBattleAction.Hero) ||
                             DefenderTraitorAction == null && faction == CurrentBattle.Target ||
                             DefenderTraitorAction == null && faction == Faction.Black && GetPlayer(DefenderBattleAction.Initiator).Ally == Faction.Black && player.Traitors.Contains(AggressorBattleAction.Hero)))
                    {
                        result.Add(typeof(TreacheryCalled));
                    }
                    if (faction == AggressorBattleAction.Initiator && AggressorBattleAction.Weapon != null && AggressorBattleAction.Weapon.Type == TreacheryCardType.PoisonTooth && !PoisonToothCancelled) result.Add(typeof(PoisonToothCancelled));
                    if (faction == DefenderBattleAction.Initiator && DefenderBattleAction.Weapon != null && DefenderBattleAction.Weapon.Type == TreacheryCardType.PoisonTooth && !PoisonToothCancelled) result.Add(typeof(PoisonToothCancelled));
                    break;

                case Phase.BattleConclusion:
                    if (Version < 43)
                    {
                        if ((faction == AggressorBattleAction.Initiator && !HasActedOrPassed.Contains(faction)) || (faction == DefenderBattleAction.Initiator && !HasActedOrPassed.Contains(faction)))
                        {
                            result.Add(typeof(BattleConcluded));
                        }
                    }
                    else
                    {
                        if (faction == BattleWinner) result.Add(typeof(BattleConcluded));
                    }
                    break;

                case Phase.Facedancing:
                    if (faction == Faction.Purple) result.Add(typeof(FaceDanced));
                    break;

                case Phase.BattleReport:
                    if (player.Has(TreacheryCardType.Amal) && Aggressor == null) result.Add(typeof(AmalPlayed));
                    break;

                case Phase.ReplacingFaceDancer:
                    if (faction == Faction.Purple) result.Add(typeof(FaceDancerReplaced));
                    break;

                case Phase.TurnConcluded:
                    if (player.Has(TreacheryCardType.Amal)) result.Add(typeof(AmalPlayed));
                    break;

                case Phase.PerformingKarmaHandSwap:
                    if (faction == Faction.Black) result.Add(typeof(KarmaHandSwap));
                    break;

                case Phase.Clairvoyance:
                    if (faction == LatestClairvoyance.Target) result.Add(typeof(ClairVoyanceAnswered));
                    break;
            }

            //Events that are (amost) always valid
            if (!SecretsRemainHidden.Contains(faction) && 
                (Version <= 97 && CurrentPhase < Phase.MetheorAndStormSpell) || (Version >= 98 && CurrentPhase == Phase.TradingFactions))
            {
                result.Add(typeof(HideSecrets));
            }

            if (CurrentPhase != Phase.Clairvoyance && CurrentPhase > Phase.TradingFactions && CurrentPhase < Phase.GameEnded)
            {
                if (player.Has(TreacheryCardType.RaiseDead))
                {
                    result.Add(typeof(RaiseDeadPlayed));
                }

                if (player.Has(TreacheryCardType.Clairvoyance))
                {
                    result.Add(typeof(ClairVoyancePlayed));
                }

                if (player.HasKarma)
                {
                    result.Add(typeof(Karma));
                }

                if (Players.Count > 1 &&
                    faction == Faction.Black &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    CurrentMainPhase == MainPhase.Bidding &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaHandSwapInitiated));
                }

                if (faction == Faction.Red &&
                    !player.SpecialKarmaPowerUsed &&
                    player.Has(TreacheryCardType.Karma) &&
                    CurrentMainPhase == MainPhase.Resurrection &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaFreeRevival));
                }

                if (faction == Faction.Grey &&
                    (!player.SpecialKarmaPowerUsed && player.Has(TreacheryCardType.Karma) || KarmaHmsMovesLeft == 1) &&
                    CurrentMainPhase == MainPhase.ShipmentAndMove &&
                    player == ShipmentAndMoveSequence.CurrentPlayer &&
                    player.AnyForcesIn(Map.HiddenMobileStronghold) > 0 &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaHmsMovement));
                }

                if (faction == Faction.Yellow && CurrentMainPhase == MainPhase.Blow && CurrentTurn > 1 &&
                    !player.SpecialKarmaPowerUsed && player.Has(TreacheryCardType.Karma)
                    && Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaMonster));
                }

                if (faction == Faction.Green && CurrentMainPhase == MainPhase.Battle &&
                    CurrentBattle != null && CurrentBattle.IsInvolved(player) &&
                    !player.SpecialKarmaPowerUsed && player.Has(TreacheryCardType.Karma) &&
                    Applicable(Rule.AdvancedKarama))
                {
                    result.Add(typeof(KarmaPrescience));
                }

                if (faction == Faction.Blue && CurrentPhase > Phase.AllianceB &&
                    CurrentPhase < Phase.NonOrangeShip &&
                    BlueBattleAnnouncement.ValidTerritories(this, player).Any())
                {
                    result.Add(typeof(BlueBattleAnnouncement));
                }

                if (player.Ally != Faction.None)
                {
                    result.Add(typeof(AllyPermission));
                }

                if (Players.Count > 1 &&
                    Donated.ValidTargets(this, player).Any() &&
                    player.Resources > 0 &&
                    Donated.MayDonate(this, player) &&
                    (AggressorBattleAction == null || faction != AggressorBattleAction.Initiator) &&
                    (DefenderBattleAction == null || faction != DefenderBattleAction.Initiator))
                {
                    result.Add(typeof(Donated));
                }
            }

            if (CurrentMainPhase > MainPhase.Setup && CurrentPhase < Phase.GameEnded)
            {
                result.Add(typeof(DealOffered));
                result.Add(typeof(DealAccepted));
            }

        }

        public static IEnumerable<Type> GetGameEventTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass => ass.GetTypes().Where(t => t.IsSubclassOf(typeof(GameEvent))).Distinct());
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
        }

        private void MainPhaseMiddle()
        {
            CurrentMoment = MainPhaseMoment.Middle;
        }

        private void MainPhaseEnd()
        {
            CurrentMoment = MainPhaseMoment.End;
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

            if (hasOrnithopters)
            {
                return 3;
            }
            else if (p.Is(Faction.Yellow) && !Prevented(FactionAdvantage.YellowExtraMove))
            {
                return 2;
            }
            else if (p.Is(Faction.Grey) && !Prevented(FactionAdvantage.GreyCyborgExtraMove) && moved.Any(b => b.AmountOfSpecialForces > 0))
            {
                return 2;
            }

            return 1;
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
