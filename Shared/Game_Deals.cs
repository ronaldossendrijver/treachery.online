/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public List<DealOffered> DealOffers { get; } = new();

        public List<Deal> Deals { get; } = new();

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
    }
}
