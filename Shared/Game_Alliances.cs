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
        #region BrownCardTrading

        public CardTraded CurrentCardTradeOffer { get; internal set; }
        internal Phase PhaseBeforeCardTrade { get; set; }
        internal int LastTurnCardWasTraded { get; set; } = -1;

        #endregion

        #region Nexus

        internal List<AllianceOffered> CurrentAllianceOffers { get; } = new();
        internal bool NexusHasOccured { get; set; } = false;

        internal Dictionary<Faction, int> PermittedUseOfAllySpice { get; } = new();
        internal Dictionary<Faction, int> PermittedUseOfRedSpice { get; set; } = new();
        internal Dictionary<Faction, TreacheryCard> PermittedUseOfAllyKarma { get; } = new();

        public bool GreenSharesPrescience { get; internal set; } = false;
        public bool YellowWillProtectFromMonster { get; internal set; } = false;
        public bool YellowAllowsThreeFreeRevivals { get; internal set; } = false;
        public bool YellowSharesPrescience { get; internal set; } = false;
        public bool YellowRefundsBattleDial { get; internal set; } = false;
        public int RedWillPayForExtraRevival { get; internal set; } = 0;
        public bool OrangeAllowsShippingDiscount { get; internal set; } = false;
        public bool BlueAllowsUseOfVoice { get; internal set; } = false;
        public bool GreyAllowsReplacingCards { get; internal set; } = false;
        public bool PurpleAllowsRevivalDiscount { get; internal set; } = false;
        public bool WhiteAllowsUseOfNoField { get; internal set; } = false;
        public bool CyanAllowsKeepingCards { get; internal set; } = false;
        public bool PinkSharesAmbassadors { get; internal set; } = false;

        internal void MakeAlliance(Faction a, Faction b)
        {
            var playerA = GetPlayer(a);
            var playerB = GetPlayer(b);
            playerA.Ally = b;
            playerB.Ally = a;
            DiscardNexusCard(playerA);
            DiscardNexusCard(playerB);
            Log(a, " and ", b, " are now allies");

            if (Version > 150)
            {
                SetPermissions(a, true);
                SetPermissions(b, true);
            }
        }

        internal void BreakAlliance(Faction f)
        {
            var initiator = GetPlayer(f);
            var currentAlly = GetPlayer(initiator.Ally);

            if (Version <= 150)
            {
                if (f == Faction.Orange || initiator.Ally == Faction.Orange)
                {
                    OrangeAllowsShippingDiscount = false;
                }

                if (f == Faction.Red || initiator.Ally == Faction.Red)
                {
                    RedWillPayForExtraRevival = 0;
                }

                if (f == Faction.Yellow || initiator.Ally == Faction.Yellow)
                {
                    YellowWillProtectFromMonster = false;
                    YellowAllowsThreeFreeRevivals = false;
                }
            }
            else
            {
                SetPermissions(f, false);
                SetPermissions(initiator.Ally, false);
            }

            PermittedUseOfAllySpice.Remove(f);
            PermittedUseOfAllySpice.Remove(initiator.Ally);
            PermittedUseOfAllyKarma.Remove(f);
            PermittedUseOfAllyKarma.Remove(initiator.Ally);

            initiator.Ally = Faction.None;
            currentAlly.Ally = Faction.None;
        }

        internal void DecreasePermittedUseOfAllySpice(Faction f, int amount)
        {
            if (PermittedUseOfAllySpice.ContainsKey(f))
            {
                var newValue = PermittedUseOfAllySpice[f] - amount;
                PermittedUseOfAllySpice[f] = Math.Max(0, newValue);
            }
        }

        private void SetPermissions(Faction f, bool permission)
        {
            switch (f)
            {
                case Faction.Green: GreenSharesPrescience = permission; break;
                case Faction.Yellow:
                    YellowWillProtectFromMonster = permission;
                    YellowAllowsThreeFreeRevivals = permission;
                    YellowSharesPrescience = permission;
                    YellowRefundsBattleDial = permission;
                    break;
                case Faction.Red: RedWillPayForExtraRevival = permission ? 3 : 0; break;
                case Faction.Orange: OrangeAllowsShippingDiscount = permission; break;
                case Faction.Blue: BlueAllowsUseOfVoice = permission; break;
                case Faction.Grey: GreyAllowsReplacingCards = permission; break;
                case Faction.Purple: PurpleAllowsRevivalDiscount = permission; break;
                case Faction.White: WhiteAllowsUseOfNoField = permission; break;
                case Faction.Cyan: CyanAllowsKeepingCards = permission; break;
                case Faction.Pink: PinkSharesAmbassadors = permission; break;
            }
        }

        #endregion Nexus

        #region Information

        internal bool AreAllies(Faction a, Faction b) => GetPlayer(a)?.Ally == b;

        public int ResourcesYourAllyCanPay(Player p)
        {
            if (PermittedUseOfAllySpice.TryGetValue(p.Faction, out int value))
            {
                var ally = GetPlayer(p.Ally);
                return Math.Min(value, ally.Resources);
            }
            else
            {
                return 0;
            }
        }

        public int SpiceForBidsRedCanPay(Faction f)
        {
            if (PermittedUseOfRedSpice.TryGetValue(f, out int value))
            {
                var red = GetPlayer(Faction.Red);
                return Math.Min(value, red.Resources);
            }
            else
            {
                return 0;
            }
        }

        public TreacheryCard GetPermittedUseOfAllyKarma(Faction f)
        {
            var ally = Players.SingleOrDefault(p => p.Ally == f);

            if (PermittedUseOfAllyKarma.TryGetValue(f, out TreacheryCard value) && ally != null && ally.Has(value))
            {
                return PermittedUseOfAllyKarma[f];
            }
            else
            {
                return null;
            }
        }

        public int GetPermittedUseOfAllyResources(Faction f)
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

        #endregion Information
    }
}
