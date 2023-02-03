/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Karma : GameEvent
    {
        public int _cardId;
        public FactionAdvantage Prevented;

        public Karma(Game game) : base(game)
        {
        }

        public Karma()
        {
        }

        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Get(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (!ValidFactionAdvantages(Game, Player).Contains(Prevented)) return Message.Express("You cannot prevent ", Prevented, " at this moment");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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
                    if (g.Applicable(Rule.YellowSeesStorm)) result.Add(FactionAdvantage.YellowStormPrescience);
                    if (g.Applicable(Rule.YellowStormLosses)) result.Add(FactionAdvantage.YellowProtectedFromStorm);
                }

                if (In(g, MainPhase.Blow))
                {
                    result.Add(FactionAdvantage.YellowControlsMonster);
                    result.Add(FactionAdvantage.YellowProtectedFromMonster);
                }

                if (In(g, MainPhase.ShipmentAndMove))
                {
                    result.Add(FactionAdvantage.YellowExtraMove);
                }

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
            {
                if (In(g, MainPhase.ShipmentAndMove))
                {
                    if (g.Applicable(Rule.OrangeDetermineShipment)) result.Add(FactionAdvantage.OrangeDetermineMoveMoment);
                    result.Add(FactionAdvantage.OrangeSpecialShipments);
                    result.Add(FactionAdvantage.OrangeShipmentsDiscount);
                    result.Add(FactionAdvantage.OrangeShipmentsDiscountAlly);
                    result.Add(FactionAdvantage.OrangeReceiveShipment);
                }
            }

            if (p.Faction != Faction.Blue && g.IsPlaying(Faction.Blue))
            {
                if (In(g, MainPhase.Charity))
                {
                    if (g.Applicable(Rule.BlueAutoCharity)) result.Add(FactionAdvantage.BlueCharity);
                }

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
                if (In(g, MainPhase.Storm)) result.Add(FactionAdvantage.GreyMovingHMS);

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

                if (In(g, MainPhase.Contemplate))
                {
                    result.Add(FactionAdvantage.PurpleReplacingFaceDancer);
                }
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

        private static bool In(Game g, MainPhase phase) => g.Version < 150 || g.CurrentMainPhase == phase;

        public static IEnumerable<TreacheryCard> ValidKarmaCards(Game g, Player p)
        {
            return
                p.TreacheryCards.Where(c => c.Type == TreacheryCardType.Karma ||
                (p.Is(Faction.Blue) && c.Type == TreacheryCardType.Useless && g.Applicable(Rule.BlueWorthlessAsKarma)));
        }

        public static bool CanKarma(Game g, Player p)
        {
            return ValidKarmaCards(g, p).Any();
        }
    }
}
