﻿/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bot;

public partial class ClassicBot
{
    protected virtual DealAccepted DetermineDealAccepted()
    {
        if (!DealAccepted.MayDeal(Game, this, 1) || Game.Applicable(Rule.DisableResourceTransfers)) return null;

        DealAccepted result = null;

        if (Game.CurrentPhase == Phase.ClaimingCharity && TechTokens.Any(tt => tt == TechToken.Resources) && Resources <= 3 && !Game.HasBiddingPrescience(this) && !(Ally != Faction.None && Game.HasBiddingPrescience(AlliedPlayer)))
        {
            var greenPrescienceDeal = DealAccepted.AcceptableDeals(Game, this).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
            if (greenPrescienceDeal != null) return greenPrescienceDeal.Acceptance(Faction);
        }

        if (Game.CurrentPhase == Phase.Bidding)
            if (!Game.HasBiddingPrescience(this) && !(Ally != Faction.None && Game.HasBiddingPrescience(AlliedPlayer)))
            {
                var biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, this).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
                LogInfo("biddingPrescienceOfferEntirePhaseYellow: {0}", biddingPrescienceDeal);
                if (biddingPrescienceDeal != null && Faction == Faction.Yellow && Game.CurrentTurn == 1 && Game.Applicable(Rule.TechTokens) && Game.CurrentPhase <= Phase.ClaimingCharity) return biddingPrescienceDeal.Acceptance(Faction);

                biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, this).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
                LogInfo("biddingPrescienceOfferEntirePhase: {0}", biddingPrescienceDeal);
                if (biddingPrescienceDeal != null && Game.CurrentMainPhase == MainPhase.Bidding && Game.CardNumber == 1 && biddingPrescienceDeal.Price < 1.4f * Game.CardsOnAuction.Items.Count() && ResourcesIncludingAllyContribution - biddingPrescienceDeal.Price > 16) return biddingPrescienceDeal.Acceptance(Faction);

                biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, this).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.Bidding && d.Price <= Resources);
                LogInfo("biddingPrescienceOfferOneCard: {0}", biddingPrescienceDeal);
                var maxPriceToPay = 0;
                if (Faction == Faction.Red && HasRoomForCards && ResourcesIncludingAllyContribution > 24)
                    maxPriceToPay = 1;
                else if (((Faction == Faction.Red && ResourcesIncludingAllyContribution > 6) || ResourcesIncludingAllyContribution > 16) && HasRoomForCards) maxPriceToPay = 1;

                if (biddingPrescienceDeal != null && Game.CurrentMainPhase == MainPhase.Bidding && biddingPrescienceDeal.Price <= maxPriceToPay) return biddingPrescienceDeal.Acceptance(Faction);
            }

        return result;
    }

    protected virtual DealOffered DetermineDealCancelled()
    {
        return DetermineOutdatedDealOffers();
    }

    protected virtual DealOffered DetermineDealOffered()
    {
        DealOffered result = null;

        if (!LastTurn && Game.CurrentMainPhase > MainPhase.Setup && !Game.Applicable(Rule.DisableResourceTransfers))
        {
            if (result == null) result = DetermineDealOffered_BiddingPrescienceEntirePhaseYellowTurn1();
            if (result == null) result = DetermineDealOffered_BiddingPrescienceEntirePhase();
            if (result == null) result = DetermineDealOffered_BiddingPrescienceOneCard();
            if (result == null) result = DetermineDealOffered_StormPrescience();
            if (result == null) result = DetermineDealOffered_ResourceDeckPrescience();
            if (result == null) result = DetermineDealOffered_TellDiscardedTraitors();
        }

        return result;
    }

    protected virtual DealOffered DetermineOutdatedDealOffers()
    {
        var outdated = FindInvalidDealOffers();
        if (outdated == null) outdated = FindOutdatedDealOffer(MainPhase.Bidding, DealType.ShareBiddingPrescience);
        if (outdated == null) outdated = FindOutdatedDealOffer(MainPhase.Battle, DealType.ShareResourceDeckPrescience);
        if (outdated == null) outdated = FindOutdatedDealOffer(MainPhase.Battle, DealType.ShareStormPrescience);
        if (outdated == null && LastTurn) outdated = Game.DealOffers.FirstOrDefault();

        if (outdated != null) return outdated.Cancellation();

        return null;
    }
    protected DealOffered FindInvalidDealOffers()
    {
        var green = Game.GetPlayer(Faction.Green);
        if (green != null && Game.CurrentMainPhase == MainPhase.Bidding && !Game.HasBiddingPrescience(green)) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction.Green && offer.Type == DealType.ShareBiddingPrescience);

        if (green != null && Game.CurrentMainPhase == MainPhase.ShipmentAndMove && !Game.HasResourceDeckPrescience(green)) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction.Green && offer.Type == DealType.ShareResourceDeckPrescience);

        var yellow = Game.GetPlayer(Faction.Yellow);
        if (yellow != null && !Game.HasStormPrescience(yellow)) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction.Yellow && offer.Type == DealType.ShareStormPrescience);

        return null;
    }

    protected DealOffered FindOutdatedDealOffer(MainPhase after, DealType type)
    {
        if (Game.CurrentMainPhase > after) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction && offer.Type == type);

        return null;
    }

    protected virtual DealOffered DetermineDealOffered_TellDiscardedTraitors()
    {
        if (Faction != Faction.Black && Faction != Faction.Purple &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.TellDiscardedTraitors))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.TellDiscardedTraitors,
                EndPhase = Phase.TurnConcluded,
                Price = D(3, 3),
                Text = "tell which traitors I discarded"
            };

        return null;
    }

    protected virtual DealOffered DetermineDealOffered_ResourceDeckPrescience()
    {
        if (Faction == Faction.Green &&
            Game.CurrentMainPhase >= MainPhase.ShipmentAndMove &&
            Game.CurrentMainPhase <= MainPhase.Battle &&
            Game.HasResourceDeckPrescience(this) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareResourceDeckPrescience && deal.EndPhase == Phase.TurnConcluded))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareResourceDeckPrescience,
                EndPhase = Phase.TurnConcluded,
                Price = D(1, 2),
                Text = "share spice card prescience this turn"
            };

        return null;
    }

    protected virtual DealOffered DetermineDealOffered_StormPrescience()
    {
        if (Faction == Faction.Yellow &&
            Game.CurrentMainPhase > MainPhase.Storm &&
            Game.CurrentMainPhase <= MainPhase.Battle &&
            Game.HasStormPrescience(this) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareStormPrescience && deal.EndPhase == Phase.TurnConcluded))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareStormPrescience,
                EndPhase = Phase.TurnConcluded,
                Price = D(1, 2),
                Text = "share storm prescience this turn"
            };

        return null;
    }

    protected virtual DealOffered DetermineDealOffered_BiddingPrescienceEntirePhaseYellowTurn1()
    {
        if (Faction == Faction.Green &&
            Game.IsPlaying(Faction.Yellow) &&
            Game.CurrentTurn == 1 &&
            Game.CurrentMainPhase == MainPhase.Charity &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareBiddingPrescience && deal.EndPhase == Phase.BiddingReport && deal.To.Length == 1 && deal.To[0] == Faction.Yellow))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareBiddingPrescience,
                EndPhase = Phase.BiddingReport,
                Price = 3,
                Text = "share prescience (phase)",
                To = new[] { Faction.Yellow }
            };

        return null;
    }

    protected virtual DealOffered DetermineDealOffered_BiddingPrescienceEntirePhase()
    {
        if (Faction == Faction.Green &&
            Game.CurrentMainPhase <= MainPhase.Bidding &&
            Game.HasBiddingPrescience(this) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareBiddingPrescience && deal.EndPhase == Phase.BiddingReport && !(deal.To.Length == 1 && deal.To[0] == Faction.Yellow)))
        {
            var nrOfCards = Game.Players.Count(p => p.TreacheryCards.Count() < p.MaximumNumberOfCards);
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareBiddingPrescience,
                EndPhase = Phase.BiddingReport,
                Price = (int)((Ally == Faction.None ? 1 : 1.5f) * nrOfCards) + (int)Math.Floor(0.1 * ResourcesIncludingAllyContribution),
                Text = "share prescience (phase)"
            };
        }

        return null;
    }

    protected virtual DealOffered DetermineDealOffered_BiddingPrescienceOneCard()
    {
        if (Faction == Faction.Green &&
            Game.CurrentMainPhase == MainPhase.Bidding &&
            Game.CurrentAuctionType == AuctionType.Normal &&
            Game.HasBiddingPrescience(this) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareBiddingPrescience && deal.EndPhase == Phase.Bidding))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareBiddingPrescience,
                EndPhase = Phase.Bidding,
                Price = 1 + (int)Math.Floor(0.05 * ResourcesIncludingAllyContribution) + (Ally == Faction.None ? 0 : 1),
                Text = "share prescience (1 card)"
            };

        return null;
    }
}