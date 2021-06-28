/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public partial class Game
    {
        public bool OrangeAllyMayShipAsGuild { get; private set; } = false;
        public bool PurpleAllyMayReviveAsPurple { get; private set; } = false;
        public bool GreyAllyMayReplaceCards { get; private set; } = false;
        public bool BlueAllyMayUseVoice { get; private set; } = false;
        public int RedWillPayForExtraRevival { get; private set; } = 0;
        public bool YellowWillProtectFromShaiHulud { get; private set; } = false;
        public bool YellowAllowsThreeFreeRevivals { get; private set; } = false;
        public bool YellowSharesPrescience { get; private set; } = false;
        public bool GreenSharesPrescience { get; private set; } = false;
        private Dictionary<Faction, int> PermittedUseOfAllySpice { get; } = new Dictionary<Faction, int>();
        private Dictionary<Faction, int> PermittedUseOfRedSpice { get; set; } = new Dictionary<Faction, int>();
        private Dictionary<Faction, TreacheryCard> PermittedUseOfAllyKarma { get; } = new Dictionary<Faction, TreacheryCard>();

        public void HandleEvent(AllyPermission e)
        {
            var ally = GetPlayer(e.Initiator).Ally;

            switch (e.Initiator)
            {
                case Faction.Orange:
                    OrangeAllyMayShipAsGuild = e.AllyMayShipAsOrange;
                    break;

                case Faction.Purple:
                    PurpleAllyMayReviveAsPurple = e.AllyMayReviveAsPurple;
                    break;

                case Faction.Grey:
                    GreyAllyMayReplaceCards = e.AllyMayReplaceCards;
                    break;

                case Faction.Red:
                    RedWillPayForExtraRevival = e.RedWillPayForExtraRevival;
                    break;

                case Faction.Yellow:
                    YellowWillProtectFromShaiHulud = e.YellowWillProtectFromMonster;
                    YellowAllowsThreeFreeRevivals = e.YellowAllowsThreeFreeRevivals;
                    YellowSharesPrescience = e.YellowSharesPrescience;
                    break;

                case Faction.Green:
                    GreenSharesPrescience = e.GreenSharesPrescience;
                    break;

                case Faction.Blue:
                    BlueAllyMayUseVoice = e.BlueAllowsUseOfVoice;
                    break;
            }

            Set(PermittedUseOfAllySpice, ally, e.PermittedResources);
            Set(PermittedUseOfAllyKarma, ally, e.PermittedKarmaCard);
        }

        private void Set<KeyType, ValueType>(IDictionary<KeyType, ValueType> dict, KeyType key, ValueType value)
        {
            if (dict.ContainsKey(key))
            {
                dict.Remove(key);
            }

            dict.Add(key, value);
        }

        public TreacheryCard GetPermittedUseOfAllyKarma(Faction f)
        {
            var ally = Players.SingleOrDefault(p => p.Ally == f);

            if (PermittedUseOfAllyKarma.ContainsKey(f) && ally != null && ally.TreacheryCards.Contains(PermittedUseOfAllyKarma[f]))
            {
                return PermittedUseOfAllyKarma[f];
            }
            else
            {
                return null;
            }
        }

        public int GetPermittedUseOfAllySpice(Faction f)
        {
            var ally = GetPlayer(f).AlliedPlayer;

            if (!PermittedUseOfAllySpice.ContainsKey(f) || ally == null)
            {
                return 0;
            }
            else
            {
                return Math.Min(PermittedUseOfAllySpice[f], ally.Resources);
            }
        }

        public void DecreasePermittedUseOfAllySpice(Faction f, int amount)
        {
            if (PermittedUseOfAllySpice.ContainsKey(f))
            {
                var newValue = PermittedUseOfAllySpice[f] - amount;
                PermittedUseOfAllySpice[f] = Math.Max(0, newValue);
            }
        }

        public int SpiceYourAllyCanPay(Player p)
        {
            int result = 0;
            if (PermittedUseOfAllySpice.ContainsKey(p.Faction))
            {
                var ally = GetPlayer(p.Ally);
                result = Math.Min(PermittedUseOfAllySpice[p.Faction], ally.Resources);
            }
            return result;
        }

        public int SpiceForBidsRedCanPay(Faction f)
        {
            int result = 0;
            if (PermittedUseOfRedSpice.ContainsKey(f))
            {
                var red = GetPlayer(Faction.Red);
                result = Math.Min(PermittedUseOfRedSpice[f], red.Resources);
            }
            return result;
        }

        private bool Allies(Faction a, Faction b)
        {
            var player = GetPlayer(a);
            if (player != null)
            {
                return player.Ally == b;
            }
            else
            {
                return false;
            }
        }
    }
}
