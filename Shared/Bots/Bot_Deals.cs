/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Player
    {
        protected virtual DealAccepted DetermineDealAccepted()
        {
            if (!DealAccepted.MayAcceptDeals(Game, this, 1) || Game.Applicable(Rule.DisableResourceTransfers)) return null;

            DealAccepted result = null;

            if (Game.CurrentPhase == Phase.Bidding)
            {
                if (!Game.HasBiddingPrescience(this) && !(Ally != Faction.None && Game.HasBiddingPrescience(AlliedPlayer)))
                {
                    var biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, this).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
                    LogInfo("biddingPrescienceOfferEntirePhaseYellow: {0}", biddingPrescienceDeal);
                    if (biddingPrescienceDeal != null && Faction == Faction.Yellow && Game.CurrentTurn == 1 && Game.Applicable(Rule.GreyAndPurpleExpansionTechTokens) && Game.CurrentPhase <= Phase.ClaimingCharity)
                    {
                        return biddingPrescienceDeal.Acceptance(Faction);
                    }

                    biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, this).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.BiddingReport && d.Price <= Resources);
                    LogInfo("biddingPrescienceOfferEntirePhase: {0}", biddingPrescienceDeal);
                    if (biddingPrescienceDeal != null && Game.CurrentMainPhase == MainPhase.Bidding && Game.CardNumber == 1 && biddingPrescienceDeal.Price < 1.5f * Game.CardsOnAuction.Items.Count() && ResourcesIncludingAllyContribution - biddingPrescienceDeal.Price > 14)
                    {
                        return biddingPrescienceDeal.Acceptance(Faction);
                    }

                    biddingPrescienceDeal = DealAccepted.AcceptableDeals(Game, this).FirstOrDefault(d => d.Type == DealType.ShareBiddingPrescience && d.EndPhase == Phase.Bidding && d.Price <= Resources);
                    LogInfo("biddingPrescienceOfferOneCard: {0}", biddingPrescienceDeal);
                    int maxPriceToPay = 0;
                    if (Faction == Faction.Red && HasRoomForCards && ResourcesIncludingAllyContribution > 22)
                    {
                        maxPriceToPay = 2;
                    }
                    else if ((Faction == Faction.Red || ResourcesIncludingAllyContribution > 14) && HasRoomForCards)
                    {
                        maxPriceToPay = 1;
                    }

                    if (biddingPrescienceDeal != null && Game.CurrentMainPhase == MainPhase.Bidding && biddingPrescienceDeal.Price <= maxPriceToPay)
                    {
                        return biddingPrescienceDeal.Acceptance(Faction);
                    }
                }
            }

            return result;
        }

        protected virtual DealOffered DetermineDealOffered()
        {
            DealOffered result = null;

            if (Game.CurrentMainPhase > MainPhase.Setup && !Game.Applicable(Rule.DisableResourceTransfers))
            {
                result = DetermineOutdatedDealOffers();
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
            DealOffered outdated = FindOutdatedDealOffer(MainPhase.Bidding, DealType.ShareBiddingPrescience);
            if (outdated == null) outdated = FindOutdatedDealOffer(MainPhase.Battle, DealType.ShareResourceDeckPrescience);
            if (outdated == null) outdated = FindOutdatedDealOffer(MainPhase.Battle, DealType.ShareStormPrescience);

            if (outdated != null)
            {
                return outdated.Cancellation();
            }

            return null;
        }

        protected DealOffered FindOutdatedDealOffer(MainPhase after, DealType type)
        {
            if (Game.CurrentMainPhase > after)
            {
                return Game.DealOffers.FirstOrDefault(offer => offer.Initiator == Faction && offer.Type == type);
            }

            return null;
        }

        protected virtual DealOffered DetermineDealOffered_TellDiscardedTraitors()
        {
            if (Faction != Faction.Black && Faction != Faction.Purple &&
                !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.TellDiscardedTraitors))
            {
                return new DealOffered(Game)
                {
                    Initiator = Faction,
                    Cancel = false,
                    Type = DealType.TellDiscardedTraitors,
                    EndPhase = Phase.TurnConcluded,
                    Price = D(3, 3),
                    Text = "tell which traitors I discarded"
                };
            }

            return null;
        }

        protected virtual DealOffered DetermineDealOffered_ResourceDeckPrescience()
        {
            if (Faction == Faction.Green &&
                Game.CurrentMainPhase >= MainPhase.ShipmentAndMove &&
                Game.CurrentMainPhase <= MainPhase.Battle &&
                Game.HasResourceDeckPrescience(this) &&
                !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareResourceDeckPrescience && deal.EndPhase == Phase.TurnConcluded))
            {
                return new DealOffered(Game)
                {
                    Initiator = Faction,
                    Cancel = false,
                    Type = DealType.ShareResourceDeckPrescience,
                    EndPhase = Phase.TurnConcluded,
                    Price = D(1, 2),
                    Text = "share next spice card prescience this turn"
                };
            }

            return null;
        }

        protected virtual DealOffered DetermineDealOffered_StormPrescience()
        {
            if (Faction == Faction.Yellow &&
                Game.CurrentMainPhase > MainPhase.Storm &&
                Game.CurrentMainPhase <= MainPhase.Battle &&
                Game.HasStormPrescience(this) &&
                !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareStormPrescience && deal.EndPhase == Phase.TurnConcluded))
            {
                return new DealOffered(Game)
                {
                    Initiator = Faction,
                    Cancel = false,
                    Type = DealType.ShareStormPrescience,
                    EndPhase = Phase.TurnConcluded,
                    Price = D(1, 2),
                    Text = "share storm prescience this turn"
                };
            }

            return null;
        }

        protected virtual DealOffered DetermineDealOffered_BiddingPrescienceEntirePhaseYellowTurn1()
        {
            if (Faction == Faction.Green &&
                Game.IsPlaying(Faction.Yellow) &&
                Game.CurrentTurn == 1 &&
                Game.CurrentMainPhase <= MainPhase.Bidding &&
                Game.HasBiddingPrescience(this) &&
                !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareBiddingPrescience && deal.EndPhase == Phase.BiddingReport && deal.To.Length == 1 && deal.To[0] == Faction.Yellow))
            {
                return new DealOffered(Game)
                {
                    Initiator = Faction,
                    Cancel = false,
                    Type = DealType.ShareBiddingPrescience,
                    EndPhase = Phase.BiddingReport,
                    Price = 3,
                    Text = "share bidding prescience (entire phase)",
                    To = new Faction[] { Faction.Yellow }
                };
            }

            return null;
        }

        protected virtual DealOffered DetermineDealOffered_BiddingPrescienceEntirePhase()
        {
            if (Faction == Faction.Green &&
                Game.CurrentMainPhase <= MainPhase.Bidding &&
                Game.HasBiddingPrescience(this) &&
                !Game.DealOffers.Any(deal => deal.Initiator == Faction && deal.Type == DealType.ShareBiddingPrescience && deal.EndPhase == Phase.BiddingReport && !(deal.To.Length == 1 && deal.To[0] == Faction.Yellow)))
            {
                int nrOfCards = Game.Players.Count(p => p.TreacheryCards.Count() < p.MaximumNumberOfCards);
                return new DealOffered(Game)
                {
                    Initiator = Faction,
                    Cancel = false,
                    Type = DealType.ShareBiddingPrescience,
                    EndPhase = Phase.BiddingReport,
                    Price = (int)((Ally == Faction.None ? 1 : 1.5f) * nrOfCards) + (int)Math.Floor(0.1 * ResourcesIncludingAllyContribution),
                    Text = "share bidding prescience (entire phase)"
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
            {
                return new DealOffered(Game)
                {
                    Initiator = Faction,
                    Cancel = false,
                    Type = DealType.ShareBiddingPrescience,
                    EndPhase = Phase.Bidding,
                    Price = 1 + (int)Math.Floor(0.05 * ResourcesIncludingAllyContribution) + (Ally == Faction.None ? 0 : 1),
                    Text = "share bidding prescience (one card)"
                };
            }

            return null;
        }
    }
}
