/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public List<DealOffered> DealOffers { get; private set; } = new List<DealOffered>();

        public List<Deal> Deals { get; internal set; } = new();

        internal void StartDeal(Deal deal)
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

        public bool IsGhola(IHero l) => l.Faction != Faction.Purple && IsPlaying(Faction.Purple) && GetPlayer(Faction.Purple).Leaders.Contains(l);

        internal RecruitsPlayed CurrentRecruitsPlayed { get; set; }


        public void HandleEvent(RecruitsPlayed e)
        {
            Log(e);
            CurrentRecruitsPlayed = e;
            Discard(e.Player, TreacheryCardType.Recruits);
        }

        
    }
}
