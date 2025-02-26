/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;

namespace Treachery.Shared;

[JsonDerivedType(typeof(AcceptOrCancelPurpleRevival),nameof(AcceptOrCancelPurpleRevival))]
[JsonDerivedType(typeof(AllianceBroken),nameof(AllianceBroken))]
[JsonDerivedType(typeof(AllianceByAmbassador),nameof(AllianceByAmbassador))]
[JsonDerivedType(typeof(AllianceByTerror),nameof(AllianceByTerror))]
[JsonDerivedType(typeof(AllianceOffered),nameof(AllianceOffered))]
[JsonDerivedType(typeof(AllyPermission),nameof(AllyPermission))]
[JsonDerivedType(typeof(AmalPlayed),nameof(AmalPlayed))]
[JsonDerivedType(typeof(AmbassadorActivated),nameof(AmbassadorActivated))]
[JsonDerivedType(typeof(AmbassadorPlaced),nameof(AmbassadorPlaced))]
[JsonDerivedType(typeof(AuditCancelled),nameof(AuditCancelled))]
[JsonDerivedType(typeof(Audited),nameof(Audited))]
[JsonDerivedType(typeof(Battle),nameof(Battle))]
[JsonDerivedType(typeof(BattleClaimed),nameof(BattleClaimed))]
[JsonDerivedType(typeof(BattleConcluded),nameof(BattleConcluded))]
[JsonDerivedType(typeof(BattleInitiated),nameof(BattleInitiated))]
[JsonDerivedType(typeof(BattleRevision),nameof(BattleRevision))]
[JsonDerivedType(typeof(Bid),nameof(Bid))]
[JsonDerivedType(typeof(BlackMarketBid),nameof(BlackMarketBid))]
[JsonDerivedType(typeof(BlueAccompanies),nameof(BlueAccompanies))]
[JsonDerivedType(typeof(BlueBattleAnnouncement),nameof(BlueBattleAnnouncement))]
[JsonDerivedType(typeof(BlueFlip),nameof(BlueFlip))]
[JsonDerivedType(typeof(BluePrediction),nameof(BluePrediction))]
[JsonDerivedType(typeof(BrownDiscarded),nameof(BrownDiscarded))]
[JsonDerivedType(typeof(BrownEconomics),nameof(BrownEconomics))]
[JsonDerivedType(typeof(BrownExtraMove),nameof(BrownExtraMove))]
[JsonDerivedType(typeof(BrownFreeRevivalPrevention),nameof(BrownFreeRevivalPrevention))]
[JsonDerivedType(typeof(BrownKarmaPrevention),nameof(BrownKarmaPrevention))]
[JsonDerivedType(typeof(BrownMovePrevention),nameof(BrownMovePrevention))]
[JsonDerivedType(typeof(BrownRemoveForce),nameof(BrownRemoveForce))]
[JsonDerivedType(typeof(Bureaucracy),nameof(Bureaucracy))]
[JsonDerivedType(typeof(Captured),nameof(Captured))]
[JsonDerivedType(typeof(Caravan),nameof(Caravan))]
[JsonDerivedType(typeof(CardGiven),nameof(CardGiven))]
[JsonDerivedType(typeof(CardsDetermined),nameof(CardsDetermined))]
[JsonDerivedType(typeof(CardTraded),nameof(CardTraded))]
[JsonDerivedType(typeof(CharityClaimed),nameof(CharityClaimed))]
[JsonDerivedType(typeof(ClairVoyanceAnswered),nameof(ClairVoyanceAnswered))]
[JsonDerivedType(typeof(ClairVoyancePlayed),nameof(ClairVoyancePlayed))]
[JsonDerivedType(typeof(DealAccepted),nameof(DealAccepted))]
[JsonDerivedType(typeof(DealOffered),nameof(DealOffered))]
[JsonDerivedType(typeof(Diplomacy),nameof(Diplomacy))]
[JsonDerivedType(typeof(Discarded),nameof(Discarded))]
[JsonDerivedType(typeof(DiscardedSearched),nameof(DiscardedSearched))]
[JsonDerivedType(typeof(DiscardedSearchedAnnounced),nameof(DiscardedSearchedAnnounced))]
[JsonDerivedType(typeof(DiscardedTaken),nameof(DiscardedTaken))]
[JsonDerivedType(typeof(DiscoveryEntered),nameof(DiscoveryEntered))]
[JsonDerivedType(typeof(DiscoveryRevealed),nameof(DiscoveryRevealed))]
[JsonDerivedType(typeof(DistransUsed),nameof(DistransUsed))]
[JsonDerivedType(typeof(DivideResources),nameof(DivideResources))]
[JsonDerivedType(typeof(DivideResourcesAccepted),nameof(DivideResourcesAccepted))]
[JsonDerivedType(typeof(Donated),nameof(Donated))]
[JsonDerivedType(typeof(EndPhase),nameof(EndPhase))]
[JsonDerivedType(typeof(EstablishPlayers),nameof(EstablishPlayers))]
[JsonDerivedType(typeof(ExtortionPrevented),nameof(ExtortionPrevented))]
[JsonDerivedType(typeof(FaceDanced),nameof(FaceDanced))]
[JsonDerivedType(typeof(FaceDancerReplaced),nameof(FaceDancerReplaced))]
[JsonDerivedType(typeof(FaceDancerRevealed),nameof(FaceDancerRevealed))]
[JsonDerivedType(typeof(FactionSelected),nameof(FactionSelected))]
[JsonDerivedType(typeof(FactionTradeOffered),nameof(FactionTradeOffered))]
[JsonDerivedType(typeof(FlightDiscoveryUsed),nameof(FlightDiscoveryUsed))]
[JsonDerivedType(typeof(FlightUsed),nameof(FlightUsed))]
[JsonDerivedType(typeof(GameAdmission),nameof(GameAdmission))]
[JsonDerivedType(typeof(GreyRemovedCardFromAuction),nameof(GreyRemovedCardFromAuction))]
[JsonDerivedType(typeof(GreySelectedStartingCard),nameof(GreySelectedStartingCard))]
[JsonDerivedType(typeof(GreySwappedCardOnBid),nameof(GreySwappedCardOnBid))]
[JsonDerivedType(typeof(HarvesterPlayed),nameof(HarvesterPlayed))]
[JsonDerivedType(typeof(HideSecrets),nameof(HideSecrets))]
[JsonDerivedType(typeof(HMSAdvantageChosen),nameof(HMSAdvantageChosen))]
[JsonDerivedType(typeof(JuicePlayed),nameof(JuicePlayed))]
[JsonDerivedType(typeof(Karma),nameof(Karma))]
[JsonDerivedType(typeof(KarmaBrownDiscard),nameof(KarmaBrownDiscard))]
[JsonDerivedType(typeof(KarmaFreeRevival),nameof(KarmaFreeRevival))]
[JsonDerivedType(typeof(KarmaHandSwap),nameof(KarmaHandSwap))]
[JsonDerivedType(typeof(KarmaHandSwapInitiated),nameof(KarmaHandSwapInitiated))]
[JsonDerivedType(typeof(KarmaHmsMovement),nameof(KarmaHmsMovement))]
[JsonDerivedType(typeof(KarmaMonster),nameof(KarmaMonster))]
[JsonDerivedType(typeof(KarmaPinkDial),nameof(KarmaPinkDial))]
[JsonDerivedType(typeof(KarmaPrescience),nameof(KarmaPrescience))]
[JsonDerivedType(typeof(KarmaRevivalPrevention),nameof(KarmaRevivalPrevention))]
[JsonDerivedType(typeof(KarmaShipmentPrevention),nameof(KarmaShipmentPrevention))]
[JsonDerivedType(typeof(KarmaWhiteBuy),nameof(KarmaWhiteBuy))]
[JsonDerivedType(typeof(LoserConcluded),nameof(LoserConcluded))]
[JsonDerivedType(typeof(MetheorPlayed),nameof(MetheorPlayed))]
[JsonDerivedType(typeof(Move),nameof(Move))]
[JsonDerivedType(typeof(MulliganPerformed),nameof(MulliganPerformed))]
[JsonDerivedType(typeof(NexusCardDrawn),nameof(NexusCardDrawn))]
[JsonDerivedType(typeof(NexusPlayed),nameof(NexusPlayed))]
[JsonDerivedType(typeof(NexusVoted),nameof(NexusVoted))]
[JsonDerivedType(typeof(OrangeDelay),nameof(OrangeDelay))]
[JsonDerivedType(typeof(PerformBluePlacement),nameof(PerformBluePlacement))]
[JsonDerivedType(typeof(PerformCyanSetup),nameof(PerformCyanSetup))]
[JsonDerivedType(typeof(PerformHmsMovement),nameof(PerformHmsMovement))]
[JsonDerivedType(typeof(PerformHmsPlacement),nameof(PerformHmsPlacement))]
[JsonDerivedType(typeof(PerformSetup),nameof(PerformSetup))]
[JsonDerivedType(typeof(PerformYellowSetup),nameof(PerformYellowSetup))]
[JsonDerivedType(typeof(Planetology),nameof(Planetology))]
[JsonDerivedType(typeof(PlayerReplaced),nameof(PlayerReplaced))]
[JsonDerivedType(typeof(PoisonToothCancelled),nameof(PoisonToothCancelled))]
[JsonDerivedType(typeof(PortableAntidoteUsed),nameof(PortableAntidoteUsed))]
[JsonDerivedType(typeof(Prescience),nameof(Prescience))]
[JsonDerivedType(typeof(RaiseDeadPlayed),nameof(RaiseDeadPlayed))]
[JsonDerivedType(typeof(RecruitsPlayed),nameof(RecruitsPlayed))]
[JsonDerivedType(typeof(RedBidSupport),nameof(RedBidSupport))]
[JsonDerivedType(typeof(RedDiscarded),nameof(RedDiscarded))]
[JsonDerivedType(typeof(ReplacedCardWon),nameof(ReplacedCardWon))]
[JsonDerivedType(typeof(RequestPurpleRevival),nameof(RequestPurpleRevival))]
[JsonDerivedType(typeof(ResidualPlayed),nameof(ResidualPlayed))]
[JsonDerivedType(typeof(ResourcesAudited),nameof(ResourcesAudited))]
[JsonDerivedType(typeof(ResourcesTransferred),nameof(ResourcesTransferred))]
[JsonDerivedType(typeof(Retreat),nameof(Retreat))]
[JsonDerivedType(typeof(Revival),nameof(Revival))]
[JsonDerivedType(typeof(RockWasMelted),nameof(RockWasMelted))]
[JsonDerivedType(typeof(SetIncreasedRevivalLimits),nameof(SetIncreasedRevivalLimits))]
[JsonDerivedType(typeof(SetShipmentPermission),nameof(SetShipmentPermission))]
[JsonDerivedType(typeof(Shipment),nameof(Shipment))]
[JsonDerivedType(typeof(SkillAssigned),nameof(SkillAssigned))]
[JsonDerivedType(typeof(StormDialled),nameof(StormDialled))]
[JsonDerivedType(typeof(StormSpellPlayed),nameof(StormSpellPlayed))]
[JsonDerivedType(typeof(SwitchedSkilledLeader),nameof(SwitchedSkilledLeader))]
[JsonDerivedType(typeof(TakeLosses),nameof(TakeLosses))]
[JsonDerivedType(typeof(TerrorPlanted),nameof(TerrorPlanted))]
[JsonDerivedType(typeof(TerrorRevealed),nameof(TerrorRevealed))]
[JsonDerivedType(typeof(TestingStationUsed),nameof(TestingStationUsed))]
[JsonDerivedType(typeof(Thought),nameof(Thought))]
[JsonDerivedType(typeof(ThoughtAnswered),nameof(ThoughtAnswered))]
[JsonDerivedType(typeof(ThumperPlayed),nameof(ThumperPlayed))]
[JsonDerivedType(typeof(TraitorDiscarded),nameof(TraitorDiscarded))]
[JsonDerivedType(typeof(TraitorsSelected),nameof(TraitorsSelected))]
[JsonDerivedType(typeof(TreacheryCalled),nameof(TreacheryCalled))]
[JsonDerivedType(typeof(Voice),nameof(Voice))]
[JsonDerivedType(typeof(WhiteAnnouncesAuction),nameof(WhiteAnnouncesAuction))]
[JsonDerivedType(typeof(WhiteAnnouncesBlackMarket),nameof(WhiteAnnouncesBlackMarket))]
[JsonDerivedType(typeof(WhiteGaveCard),nameof(WhiteGaveCard))]
[JsonDerivedType(typeof(WhiteKeepsUnsoldCard),nameof(WhiteKeepsUnsoldCard))]
[JsonDerivedType(typeof(WhiteRevealedNoField),nameof(WhiteRevealedNoField))]
[JsonDerivedType(typeof(WhiteSpecifiesAuction),nameof(WhiteSpecifiesAuction))]
[JsonDerivedType(typeof(YellowRidesMonster),nameof(YellowRidesMonster))]
[JsonDerivedType(typeof(YellowSentMonster),nameof(YellowSentMonster))]
public abstract class GameEvent
{
    #region Construction

    protected GameEvent()
    {
    }

    protected GameEvent(Game game)
    {
        Initialize(game, Faction.None);
    }
    
    protected GameEvent(Game game, Faction initiator)
    {
        Initialize(game, initiator);
    }

    #endregion Construction

    #region Properties

    private const char IdStringSeparator = ';';

    public Faction Initiator { get; set; }

    public DateTimeOffset Time { get; set; }

    [JsonIgnore]
    public Game Game { get; private set; }

    [JsonIgnore]
    public Player Player { get; private set; }

    #endregion Properties

    #region Validation

    public abstract Message Validate();

    [JsonIgnore]
    public bool IsValid => Validate() == null;

    public bool IsApplicable(bool isHost)
    {
        if (Game == null) throw new ArgumentException("Cannot check applicability of a GameEvent without a Game.");

        return Game.GetApplicableEvents(Player, isHost).Contains(GetType());
    }

    #endregion Validation

    #region Execution

    public void Initialize(Game game, Faction initiator)
    {
        Game = game;
        Initiator = initiator;
        Player = game.GetPlayer(initiator);
    }

    public void Initialize(Game game)
    {
        Initialize(game, Initiator);
    }

    public virtual Message Execute(bool performValidation, bool isHost)
    {
        if (Game == null) throw new ArgumentException("Cannot execute a GameEvent without a Game.");

        try
        {
            var momentJustBeforeEvent = Game.CurrentMoment;

            Message result = null;

            if (performValidation)
            {
                if (!IsApplicable(isHost)) 
                    return Message.Express("Event '", GetMessage(), "' is not applicable");

                result = Validate();
            }

            if (result == null)
            {
                Game.RecentMilestones.Clear();
                Game.PerformPreEventTasks(this);
                ExecuteConcreteEvent();
                Game.PerformPostEventTasks(this, momentJustBeforeEvent != MainPhaseMoment.Start && Game.CurrentMoment == MainPhaseMoment.Start);
            }

            return result;
        }
        catch (Exception e)
        {
            return Message.Express("Game Error: ", e.Message, ". Technical description: ", e, ".");
        }
    }

    protected abstract void ExecuteConcreteEvent();

    public virtual Message GetMessage()
    {
        return Message.Express(GetType().Name, " by ", Initiator);
    }

    public virtual Message GetShortMessage()
    {
        return GetMessage();
    }

    #endregion Execution

    #region Support

    public static List<T> IdStringToObjects<T>(string ids, IFetcher<T> lookup)
    {
        var result = new List<T>();

        if (ids != null && ids.Length > 0)
            foreach (var id in ids.Split(IdStringSeparator)) result.Add(lookup.Find(Convert.ToInt32(id)));

        return result;
    }

    public static string ObjectsToIdString<T>(IEnumerable<T> objs, IFetcher<T> lookup)
    {
        return string.Join(IdStringSeparator, objs.Select(pj => Convert.ToString(lookup.GetId(pj))));
    }

    public bool By(Faction f)
    {
        return Initiator == f;
    }

    public bool ByAllyOf(Faction f)
    {
        return Player.Ally == f;
    }

    public bool By(Player p)
    {
        return Player == p;
    }

    public GameEvent Clone()
    {
        return (GameEvent)MemberwiseClone();
    }

    protected void Log()
    {
        Game.CurrentReport.Express(GetMessage());
    }

    protected void Log(params object[] expression)
    {
        Game.Log(expression);
    }

    protected void LogIf(bool condition, params object[] expression)
    {
        Game.LogIf(condition, expression);
    }

    protected void LogTo(Faction faction, params object[] expression)
    {
        Game.LogTo(faction, expression);
    }

    protected Player GetPlayer(Faction f)
    {
        return Game.GetPlayer(f);
    }

    protected bool IsPlaying(Faction f)
    {
        return Game.IsPlaying(f);
    }

    #endregion Support
}