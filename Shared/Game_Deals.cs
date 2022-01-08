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
        public List<DealOffered> DealOffers = new List<DealOffered>();

        public void HandleEvent(DealOffered e)
        {
            if (e.Cancel)
            {
                for (int i = 0; i < DealOffers.Count; i++)
                {
                    if (DealOffers[i].Same(e))
                    {
                        DealOffers.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                CurrentReport.Express(e);
                DealOffers.Add(e);
            }
        }

        public void HandleEvent(DealAccepted e)
        {
            CurrentReport.Express(e);
            var offer = DealOffers.FirstOrDefault(offer => offer.IsAcceptedBy(e));

            if (offer != null)
            {
                var newDeal = new Deal() { BoundFaction = offer.Initiator, ConsumingFaction = e.Initiator, DealParameter1 = e.DealParameter1, DealParameter2 = e.DealParameter2, End = e.End, Text = e.Text, Benefit = e.Benefit, Type = e.Type };
                StartDeal(newDeal);

                if (e.Price > 0)
                {
                    ExchangeResourcesInBribe(GetPlayer(e.Initiator), GetPlayer(offer.Initiator), e.Price);
                    RecentMilestones.Add(Milestone.Bribe);
                }

                if (e.Benefit > 0)
                {
                    ExchangeResourcesInBribe(GetPlayer(offer.Initiator), GetPlayer(e.Initiator), e.Benefit);
                    RecentMilestones.Add(Milestone.Bribe);
                }

                if (offer.Player.IsBot)
                {
                    HandleAcceptedBotDeal(offer, e);
                }
            }
        }

        private void HandleAcceptedBotDeal(DealOffered offer, DealAccepted accepted)
        {
            if (offer.Type == DealType.TellDiscardedTraitors)
            {
                CurrentReport.ExpressTo(accepted.Initiator, offer.Initiator, " discarded: ", offer.Player.DiscardedTraitors);
                CurrentReport.Express(offer.Initiator, " gave ", accepted.Initiator, " the agreed information");
            }
        }

        public List<Deal> Deals = new List<Deal>();


        private void StartDeal(Deal deal)
        {
            Deals.Add(deal);
        }

        public bool HasDeal(Faction f, DealType type)
        {
            return Deals.Any(Deal => Deal.ConsumingFaction == f && Deal.Type == type);
        }

        private void RemoveEndedDeals(Phase phase)
        {
            foreach (var deal in Deals.ToArray())
            {
                if (deal.End == phase)
                {
                    Deals.Remove(deal);
                }
            }
        }

        private delegate void EnterPhaseMethod();
    }
}
