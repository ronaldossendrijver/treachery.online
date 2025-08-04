/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Bots;

public partial class ClassicBot
{
    private DealAccepted? DetermineDealAccepted()
    {
        if (!DealAccepted.MayDeal(Game, Player, 1) || Game.Applicable(Rule.DisableResourceTransfers)) return null;

        DealAccepted? result = null;

        if (Game.CurrentPhase == Phase.ClaimingCharity && Player.TechTokens.Any(tt => tt == TechToken.Resources) && Resources <= 3 && !Game.HasBiddingPrescience(Player) && !(Ally != Faction.None && Game.HasBiddingPrescience(AlliedPlayer)))
        {
            var greenPrescienceDeal = DealAccepted.AcceptableDeals(Game, Player).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
            if (greenPrescienceDeal != null) return greenPrescienceDeal.Acceptance(Faction);
        }

        if (Game.CurrentPhase == Phase.Bidding)
            if (!Game.HasBiddingPrescience(Player) && !(Ally != Faction.None && Game.HasBiddingPrescience(AlliedPlayer)))
            {
                var biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, Player).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
                LogInfo("biddingPrescienceOfferEntirePhaseYellow: {0}", biddingPrescienceDeal);
                if (biddingPrescienceDeal != null && Faction == Faction.Yellow && Game.CurrentTurn == 1 && Game.Applicable(Rule.TechTokens) && Game.CurrentPhase <= Phase.ClaimingCharity) return biddingPrescienceDeal.Acceptance(Faction);

                biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, Player).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
                LogInfo("biddingPrescienceOfferEntirePhase: {0}", biddingPrescienceDeal);
                if (biddingPrescienceDeal != null && Game is { CurrentMainPhase: MainPhase.Bidding, CardNumber: 1 } && biddingPrescienceDeal.Price < 1.4f * Game.CardsOnAuction.Items.Count() && ResourcesIncludingAllyContribution - biddingPrescienceDeal.Price > 16) return biddingPrescienceDeal.Acceptance(Faction);

                biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, Player).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.Bidding && d.Price <= Resources);
                LogInfo("biddingPrescienceOfferOneCard: {0}", biddingPrescienceDeal);
                var maxPriceToPay = 0;
                if (Faction == Faction.Red && Player.HasRoomForCards && ResourcesIncludingAllyContribution > 24)
                    maxPriceToPay = 1;
                else if (((Faction == Faction.Red && ResourcesIncludingAllyContribution > 6) || ResourcesIncludingAllyContribution > 16) && Player.HasRoomForCards) maxPriceToPay = 1;

                if (biddingPrescienceDeal != null && Game.CurrentMainPhase == MainPhase.Bidding && biddingPrescienceDeal.Price <= maxPriceToPay) return biddingPrescienceDeal.Acceptance(Faction);
            }

        return result;
    }

    private DealOffered? DetermineDealCancelled()
    {
        return DetermineOutdatedDealOffers();
    }

    private DealOffered? DetermineDealOffered()
    {
        if (LastTurn || Game.CurrentMainPhase <= MainPhase.Setup ||
            Game.Applicable(Rule.DisableResourceTransfers)) return null;

        return DetermineDealOffered_BiddingPrescienceEntirePhaseYellowTurn1()
               ?? DetermineDealOffered_BiddingPrescienceEntirePhase()
               ?? DetermineDealOffered_BiddingPrescienceOneCard()
               ?? DetermineDealOffered_StormPrescience()
               ?? DetermineDealOffered_ResourceDeckPrescience()
               ?? DetermineDealOffered_TellDiscardedTraitors();
    }

    private DealOffered? DetermineOutdatedDealOffers()
    {
        var outdated = FindInvalidDealOffers()
                       ?? FindOutdatedDealOffer(MainPhase.Bidding, DealType.ShareBiddingPrescience)
                       ?? FindOutdatedDealOffer(MainPhase.Battle, DealType.ShareResourceDeckPrescience)
                       ?? FindOutdatedDealOffer(MainPhase.Battle, DealType.ShareStormPrescience);
        
        if (outdated == null && LastTurn) 
            outdated = Game.DealOffers.FirstOrDefault();

        return outdated?.Cancellation();
    }

    private DealOffered? FindInvalidDealOffers()
    {
        var green = Game.GetPlayer(Faction.Green);
        if (green != null && Game.CurrentMainPhase == MainPhase.Bidding && !Game.HasBiddingPrescience(green)) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction.Green && offer.Type == DealType.ShareBiddingPrescience);

        if (green != null && Game.CurrentMainPhase == MainPhase.ShipmentAndMove && !Game.HasResourceDeckPrescience(green)) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction.Green && offer.Type == DealType.ShareResourceDeckPrescience);

        var yellow = Game.GetPlayer(Faction.Yellow);
        if (yellow != null && !Game.HasStormPrescience(yellow)) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction.Yellow && offer.Type == DealType.ShareStormPrescience);

        return null;
    }

    private DealOffered? FindOutdatedDealOffer(MainPhase after, DealType type)
    {
        if (Game.CurrentMainPhase > after) return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction && offer.Type == type);

        return null;
    }

    private DealOffered? DetermineDealOffered_TellDiscardedTraitors()
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

    private DealOffered? DetermineDealOffered_ResourceDeckPrescience()
    {
        if (Faction == Faction.Green &&
            Game.CurrentMainPhase is >= MainPhase.ShipmentAndMove and <= MainPhase.Battle &&
            Game.HasResourceDeckPrescience(Player) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal is { Type: DealType.ShareResourceDeckPrescience, EndPhase: Phase.TurnConcluded }))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareResourceDeckPrescience,
                EndPhase = Phase.TurnConcluded,
                Price = D(1, 2),
                Text = "share spice card prescience Player turn"
            };

        return null;
    }

    private DealOffered? DetermineDealOffered_StormPrescience()
    {
        if (Faction == Faction.Yellow &&
            Game.CurrentMainPhase is > MainPhase.Storm and <= MainPhase.Battle &&
            Game.HasStormPrescience(Player) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal is { Type: DealType.ShareStormPrescience, EndPhase: Phase.TurnConcluded }))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareStormPrescience,
                EndPhase = Phase.TurnConcluded,
                Price = D(1, 2),
                Text = "share storm prescience Player turn"
            };

        return null;
    }

    private DealOffered? DetermineDealOffered_BiddingPrescienceEntirePhaseYellowTurn1()
    {
        if (Faction == Faction.Green &&
            Game.IsPlaying(Faction.Yellow) &&
            Game is { CurrentTurn: 1, CurrentMainPhase: MainPhase.Charity } &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal is { Type: DealType.ShareBiddingPrescience, EndPhase: Phase.BiddingReport } && deal.To.Length == 1 && deal.To[0] == Faction.Yellow))
            return new DealOffered(Game, Faction)
            {
                Cancel = false,
                Type = DealType.ShareBiddingPrescience,
                EndPhase = Phase.BiddingReport,
                Price = 3,
                Text = "share prescience (phase)",
                To = [Faction.Yellow]
            };

        return null;
    }

    private DealOffered? DetermineDealOffered_BiddingPrescienceEntirePhase()
    {
        if (Faction == Faction.Green &&
            Game.CurrentMainPhase <= MainPhase.Bidding &&
            Game.HasBiddingPrescience(Player) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal is { Type: DealType.ShareBiddingPrescience, EndPhase: Phase.BiddingReport } && !(deal.To.Length == 1 && deal.To[0] == Faction.Yellow)))
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

    private DealOffered? DetermineDealOffered_BiddingPrescienceOneCard()
    {
        if (Faction == Faction.Green &&
            Game is { CurrentMainPhase: MainPhase.Bidding, CurrentAuctionType: AuctionType.Normal } &&
            Game.HasBiddingPrescience(Player) &&
            !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal is { Type: DealType.ShareBiddingPrescience, EndPhase: Phase.Bidding }))
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