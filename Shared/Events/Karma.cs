/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */


namespace Treachery.Shared;

public class Karma : GameEvent
{
    #region Construction

    public Karma(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Karma()
    {
    }

    #endregion Construction

    #region Properties

    public FactionAdvantage Prevented { get; set; }

    public int _cardId;

    [JsonIgnore]
    public TreacheryCard Card
    {
        get => TreacheryCardManager.Get(_cardId);
        set => _cardId = TreacheryCardManager.GetId(value);
    }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!ValidFactionAdvantages(Game, Player).Contains(Prevented)) return Message.Express("You cannot prevent ", Prevented, " at this moment");

        return null;
    }

    public static IEnumerable<FactionAdvantage> ValidFactionAdvantages(Game g, Player p)
    {
        var result = new List<FactionAdvantage>
        {
            FactionAdvantage.None
        };

        if (p.Faction != Faction.Green && g.IsPlaying(Faction.Green))
        {

            if (In(g, MainPhase.Bidding)) result.Add(FactionAdvantage.GreenBiddingPrescience);
            if (In(g, MainPhase.Resurrection) || In(g, MainPhase.ShipmentAndMove)) result.Add(FactionAdvantage.GreenSpiceBlowPrescience);

            if (In(g, MainPhase.Battle))
            {
                if (g.Applicable(Rule.GreenMessiah)) result.Add(FactionAdvantage.GreenUseMessiah);
                result.Add(FactionAdvantage.GreenBattlePlanPrescience);
            }
        }

        if (p.Faction != Faction.Black && g.IsPlaying(Faction.Black))
        {
            if (In(g, MainPhase.Bidding)) result.Add(FactionAdvantage.BlackFreeCard);

            if (In(g, MainPhase.Battle))
            {
                if (g.Applicable(Rule.BlackCapturesOrKillsLeaders)) result.Add(FactionAdvantage.BlackCaptureLeader);

                result.Add(FactionAdvantage.BlackCallTraitorForAlly);
            }
        }

        if (p.Faction != Faction.Yellow && g.IsPlaying(Faction.Yellow))
        {
            if (In(g, MainPhase.Storm))
            {
                if (g.Applicable(Rule.YellowDeterminesStorm)) result.Add(FactionAdvantage.YellowStormPrescience);
                if (g.Applicable(Rule.YellowStormLosses)) result.Add(FactionAdvantage.YellowProtectedFromStorm);
            }

            if (In(g, MainPhase.Blow))
            {
                result.Add(FactionAdvantage.YellowControlsMonster);
                result.Add(FactionAdvantage.YellowProtectedFromMonster);
            }

            if (In(g, MainPhase.ShipmentAndMove)) result.Add(FactionAdvantage.YellowExtraMove);

            if (In(g, MainPhase.Battle))
            {
                if (g.Applicable(Rule.YellowSpecialForces)) result.Add(FactionAdvantage.YellowSpecialForceBonus);
                if (g.Applicable(Rule.AdvancedCombat)) result.Add(FactionAdvantage.YellowNotPayingForBattles);
            }
        }

        if (p.Faction != Faction.Red && g.IsPlaying(Faction.Red))
        {
            result.Add(FactionAdvantage.RedGiveSpiceToAlly);

            if (In(g, MainPhase.Bidding)) result.Add(FactionAdvantage.RedReceiveBid);

            if (In(g, MainPhase.Resurrection)) result.Add(FactionAdvantage.RedLetAllyReviveExtraForces);

            if (In(g, MainPhase.Battle) && g.Applicable(Rule.RedSpecialForces)) result.Add(FactionAdvantage.RedSpecialForceBonus);
        }

        if (p.Faction != Faction.Orange && g.IsPlaying(Faction.Orange))
            if (In(g, MainPhase.ShipmentAndMove))
            {
                var guildDetermineShipmentMayStillBePrevented = g.Version < 156 || (!g.HasActedOrPassed.Contains(Faction.Orange) && g.CurrentPhase != Phase.OrangeShip);
                if (g.Applicable(Rule.OrangeDetermineShipment) && guildDetermineShipmentMayStillBePrevented) result.Add(FactionAdvantage.OrangeDetermineMoveMoment);
                result.Add(FactionAdvantage.OrangeSpecialShipments);
                result.Add(FactionAdvantage.OrangeShipmentsDiscount);
                result.Add(FactionAdvantage.OrangeShipmentsDiscountAlly);
                result.Add(FactionAdvantage.OrangeReceiveShipment);
            }

        if (p.Faction != Faction.Blue && g.IsPlaying(Faction.Blue))
        {
            if (In(g, MainPhase.Charity))
                if (g.Applicable(Rule.BlueAutoCharity)) result.Add(FactionAdvantage.BlueCharity);

            if (g.Applicable(Rule.BlueWorthlessAsKarma)) result.Add(FactionAdvantage.BlueWorthlessAsKarma);
            if (g.Applicable(Rule.BlueAdvisors)) result.Add(FactionAdvantage.BlueAnnouncesBattle);

            if (In(g, MainPhase.ShipmentAndMove))
            {
                if (g.Applicable(Rule.BlueAdvisors)) result.Add(FactionAdvantage.BlueIntrusion);
                result.Add(FactionAdvantage.BlueAccompanies);
            }

            if (In(g, MainPhase.Battle)) result.Add(FactionAdvantage.BlueUsingVoice);
        }

        if (p.Faction != Faction.Grey && g.IsPlaying(Faction.Grey))
        {
            if (In(g, MainPhase.Storm)) result.Add(FactionAdvantage.GreyMovingHms);

            if (In(g, MainPhase.Bidding))
            {
                result.Add(FactionAdvantage.GreySelectingCardsOnAuction);
                result.Add(FactionAdvantage.GreyAllyDiscardingCard);
                if (g.Applicable(Rule.GreySwappingCardOnBid)) result.Add(FactionAdvantage.GreySwappingCard);
            }

            if (In(g, MainPhase.ShipmentAndMove)) result.Add(FactionAdvantage.GreyCyborgExtraMove);

            if (In(g, MainPhase.Battle))
            {
                result.Add(FactionAdvantage.GreySpecialForceBonus);
                result.Add(FactionAdvantage.GreyReplacingSpecialForces);
            }
        }

        if (p.Faction != Faction.Purple && g.IsPlaying(Faction.Purple))
        {
            if (In(g, MainPhase.Bidding) || In(g, MainPhase.Resurrection)) result.Add(FactionAdvantage.PurpleIncreasingRevivalLimits);

            if (In(g, MainPhase.Resurrection))
            {
                result.Add(FactionAdvantage.PurpleRevivalDiscount);
                result.Add(FactionAdvantage.PurpleRevivalDiscountAlly);
                result.Add(FactionAdvantage.PurpleReceiveRevive);
                result.Add(FactionAdvantage.PurpleEarlyLeaderRevive);
                if (g.Applicable(Rule.PurpleGholas)) result.Add(FactionAdvantage.PurpleReviveGhola);
            }

            if (In(g, MainPhase.Contemplate)) result.Add(FactionAdvantage.PurpleReplacingFaceDancer);
        }

        if (p.Faction != Faction.Brown && g.IsPlaying(Faction.Brown))
        {
            if (In(g, MainPhase.Charity)) result.Add(FactionAdvantage.BrownControllingCharity);

            result.Add(FactionAdvantage.BrownDiscarding);

            if (In(g, MainPhase.Resurrection)) result.Add(FactionAdvantage.BrownRevival);

            if (In(g, MainPhase.Battle))
            {
                result.Add(FactionAdvantage.BrownReceiveForcePayment);
                if (g.Applicable(Rule.BrownAuditor)) result.Add(FactionAdvantage.BrownAudit);
            }

            if (In(g, MainPhase.Contemplate)) result.Add(FactionAdvantage.BrownEconomics);
        }

        if (p.Faction != Faction.White && g.IsPlaying(Faction.White))
        {
            if (In(g, MainPhase.Bidding))
            {
                result.Add(FactionAdvantage.WhiteAuction);
                if (g.Applicable(Rule.WhiteBlackMarket)) result.Add(FactionAdvantage.WhiteBlackMarket);
            }

            if (In(g, MainPhase.ShipmentAndMove)) result.Add(FactionAdvantage.WhiteNofield);
        }

        if (p.Faction != Faction.Pink && g.IsPlaying(Faction.Pink))
        {
            if (In(g, MainPhase.Resurrection)) result.Add(FactionAdvantage.PinkAmbassadors);
            if (In(g, MainPhase.Battle)) result.Add(FactionAdvantage.PinkOccupation);
            if (In(g, MainPhase.Collection)) result.Add(FactionAdvantage.PinkCollection);
        }

        if (p.Faction != Faction.Cyan && g.IsPlaying(Faction.Cyan))
        {
            result.Add(FactionAdvantage.CyanEnemyOfEnemy);
            if (In(g, MainPhase.ShipmentAndMove)) result.Add(FactionAdvantage.CyanGainingVidal);
            if (In(g, MainPhase.Contemplate)) result.Add(FactionAdvantage.CyanPlantingTerror);
        }

        return result;
    }

    private static bool In(Game g, MainPhase phase)
    {
        return g.Version < 150 || g.CurrentMainPhase == phase;
    }

    public static IEnumerable<TreacheryCard> ValidKarmaCards(Game g, Player p)
    {
        return p.TreacheryCards.Where(c =>
            c.Type == TreacheryCardType.Karma ||
            (c.Type == TreacheryCardType.Useless && p.Is(Faction.Blue) && g.Applicable(Rule.BlueWorthlessAsKarma)) ||
            (c.Type == TreacheryCardType.Clairvoyance && p.Occupies(g.Map.Shrine)));
    }

    public static bool CanKarma(Game g, Player p)
    {
        return ValidKarmaCards(g, p).Any();
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.Discard(Card);
        Log();
        Game.Stone(Milestone.Karma);

        if (Prevented is not FactionAdvantage.None) Game.Prevent(Initiator, Prevented);

        RevokeBattlePlansIfNeeded();

        if (Prevented is FactionAdvantage.BlueUsingVoice) Game.CurrentVoice = null;

        if (Prevented is FactionAdvantage.GreenBattlePlanPrescience) Game.CurrentPrescience = null;
        
        if (Prevented is FactionAdvantage.YellowRidesMonster && Game.CurrentPhase is Phase.YellowRidingMonsterA or Phase.YellowRidingMonsterB)
        {
            Game.DetermineNextShipmentAndMoveSubPhase();
        }
    }

    private void RevokeBattlePlansIfNeeded()
    {
        if (Game.CurrentMainPhase == MainPhase.Battle)
        {
            if (Prevented == FactionAdvantage.YellowNotPayingForBattles ||
                Prevented == FactionAdvantage.YellowSpecialForceBonus)
                Game.RevokePlanIfNeeded(Faction.Yellow);

            if (Prevented == FactionAdvantage.RedSpecialForceBonus) Game.RevokePlanIfNeeded(Faction.Red);

            if (Prevented == FactionAdvantage.GreySpecialForceBonus) Game.RevokePlanIfNeeded(Faction.Grey);

            if (Prevented == FactionAdvantage.GreenUseMessiah) Game.RevokePlanIfNeeded(Faction.Green);
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(
            Initiator,
            " play ",
            MessagePart.ExpressIf(Card.Type != TreacheryCardType.Karma, Card, " as "),
            " a ",
            TreacheryCardType.Karma,
            " card");
    }

    #endregion Execution


}