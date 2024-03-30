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

namespace Treachery.Shared;

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

    public bool GreenSharesPrescience { get; internal set; }
    public bool YellowWillProtectFromMonster { get; internal set; }
    public bool YellowAllowsThreeFreeRevivals { get; internal set; }
    public bool YellowSharesPrescience { get; internal set; }
    public bool YellowRefundsBattleDial { get; internal set; }
    public int RedWillPayForExtraRevival { get; internal set; }
    public bool OrangeAllowsShippingDiscount { get; internal set; }
    public bool BlueAllowsUseOfVoice { get; internal set; }
    public bool GreyAllowsReplacingCards { get; internal set; }
    public bool PurpleAllowsRevivalDiscount { get; internal set; }
    public bool WhiteAllowsUseOfNoField { get; internal set; }
    public bool CyanAllowsKeepingCards { get; internal set; }
    public bool PinkSharesAmbassadors { get; internal set; }

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
            
        while (playerA.HasTooManyCards) Discard(playerA, playerA.TreacheryCards.RandomOrDefault(Random));

        while (playerB.HasTooManyCards) Discard(playerB, playerB.TreacheryCards.RandomOrDefault(Random));
    }

    internal void BreakAlliance(Faction f)
    {
        var initiator = GetPlayer(f);
        var currentAlly = GetPlayer(initiator.Ally);

        if (Version <= 150)
        {
            if (f == Faction.Orange || initiator.Ally == Faction.Orange) OrangeAllowsShippingDiscount = false;

            if (f == Faction.Red || initiator.Ally == Faction.Red) RedWillPayForExtraRevival = 0;

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

    internal bool AreAllies(Faction a, Faction b)
    {
        return GetPlayer(a)?.Ally == b;
    }

    public int ResourcesYourAllyCanPay(Player p)
    {
        if (PermittedUseOfAllySpice.TryGetValue(p.Faction, out var value))
        {
            var ally = GetPlayer(p.Ally);
            return Math.Min(value, ally.Resources);
        }

        return 0;
    }

    public int SpiceForBidsRedCanPay(Faction f)
    {
        if (PermittedUseOfRedSpice.TryGetValue(f, out var value))
        {
            var red = GetPlayer(Faction.Red);
            return Math.Min(value, red.Resources);
        }

        return 0;
    }

    public TreacheryCard GetPermittedUseOfAllyKarma(Faction f)
    {
        var ally = Players.SingleOrDefault(p => p.Ally == f);

        if (PermittedUseOfAllyKarma.TryGetValue(f, out var value) && ally != null && ally.Has(value))
            return PermittedUseOfAllyKarma[f];
        return null;
    }

    public int GetPermittedUseOfAllyResources(Faction f)
    {
        var ally = GetPlayer(f).AlliedPlayer;

        if (!PermittedUseOfAllySpice.ContainsKey(f) || ally == null)
            return 0;
        return Math.Min(PermittedUseOfAllySpice[f], ally.Resources);
    }

    #endregion Information
}