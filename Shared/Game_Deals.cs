/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;
using System;

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
                CurrentReport.Add(e.GetMessage());
                DealOffers.Add(e);
            }
        }

        public void HandleEvent(DealAccepted e)
        {
            CurrentReport.Add(e.GetMessage());
            var offer = DealOffers.FirstOrDefault(offer => offer.IsAcceptedBy(e));

            if (offer != null)
            {
                var newDeal = new Deal() { BoundFaction = offer.Initiator, ConsumingFaction = e.Initiator, DealParameter1 = e.DealParameter1, DealParameter2 = e.DealParameter2, End = e.End, Text = e.Text, Type = e.Type };
                StartDeal(newDeal);

                if (e.Price > 0)
                {
                    ExchangeResourcesInBribe(GetPlayer(e.Initiator), GetPlayer(offer.Initiator), e.Price);
                    RecentMilestones.Add(Milestone.Bribe);
                }
            }
        }

        public List<Deal> Deals = new List<Deal>();

        public void StartDeal(Deal deal)
        {
            Deals.Add(deal);
            CurrentReport.Add(new Message("Deal: {0}", deal.ToString(this)));
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
                    CurrentReport.Add(new Message("Deal ended: {0}", deal.ToString(this)));
                }
            }

            /*foreach (var deal in DealOffers.ToArray())
            {
                if (deal.End == phase)
                {
                    DealOffers.Remove(deal);
                    //CurrentReport.Add(new Message("Deal ended: {0}", deal.ToString(this)));
                }
            }*/
        }

        private delegate void EnterPhaseMethod();
    }
}
