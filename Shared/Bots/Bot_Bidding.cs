/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared.Model;

public partial class Player
{
    protected virtual Bid DetermineBid()
    {
        LogInfo("DetermineBid()");

        var otherPlayers = Game.Players.Where(p => p != this && p.HasRoomForCards);
        var biddingPlayers = otherPlayers.Where(p => p.TreacheryCards.Count() < p.MaximumNumberOfCards);
        var maxBidOfOtherPlayer = !biddingPlayers.Any() ? 0 : biddingPlayers.Max(p => p.Resources);

        var currentBid = Game.CurrentBid == null ? 0 : Game.CurrentBid.TotalAmount;
        var currentBidIsFromAlly = Game.CurrentBid != null && Game.CurrentBid.Initiator == Ally;
        var isKnownCard = Game.HasBiddingPrescience(this) || (HasAlly && Game.HasBiddingPrescience(AlliedPlayer)) || Game.KnownCards(this).Contains(Game.CardsOnAuction.Top);

        var thisCardIsUseless = isKnownCard && !MayUseUselessAsKarma && Game.CardsOnAuction.Top.Type == TreacheryCardType.Useless;
        var thisCardIsCrappy = isKnownCard && !WannaHave(Game.CardsOnAuction.Top);
        var thisCardIsPerfect = isKnownCard && CardQuality(Game.CardsOnAuction.Top, this) == 5;

        var resourcesToKeep = thisCardIsPerfect ? Param.Bidding_ResourcesToKeepWhenCardIsPerfect : Param.Bidding_ResourcesToKeepWhenCardIsntPerfect;
        var resourcesAvailable = Math.Max(0, Resources - resourcesToKeep) + ResourcesFromAlly + ResourcesFromRed;
        var couldUseKarmaForBid = Game.CurrentAuctionType == AuctionType.Normal && !Game.KarmaPrevented(Faction) && Ally != Faction.Red && (SpecialKarmaPowerUsed || !Param.Karma_SaveCardToUseSpecialKarmaAbility || Game.CurrentTurn >= 5 || MaximumNumberOfCards - TreacheryCards.Count <= 1);
        var karmaCardToUseForBidding = couldUseKarmaForBid && ((MayUseUselessAsKarma && currentBid > 2) || currentBid > 4 || resourcesAvailable <= currentBid) ? TreacheryCards.FirstOrDefault(c => c.Type == TreacheryCardType.Karma || (MayUseUselessAsKarma && c.Type == TreacheryCardType.Useless)) : null;
        var karmaWorth = karmaCardToUseForBidding == null ? 0 : 8;
        var maximumIWillSpend = D(1, Math.Min(resourcesAvailable, 12) + karmaWorth);
        var amountToBidInSilentOrOnceAround = D(1, Math.Min(resourcesAvailable, 8));

        LogInfo("currentBidIsFromAlly: {0}, thisCardIsUseless: {1}, thisCardIsCrappy: {2}, resourcesAvailable:, {3}, karmaWorth: {4}, maximumIWillSpend: {5}, karmaCardToUseForBidding: {6}.",
            currentBidIsFromAlly, thisCardIsUseless, thisCardIsCrappy, resourcesAvailable, karmaWorth, maximumIWillSpend, karmaCardToUseForBidding);

        if ((Faction == Faction.White || Ally == Faction.White) &&
            (Game.CurrentAuctionType == AuctionType.BlackMarketSilent || Game.CurrentAuctionType == AuctionType.BlackMarketNormal || Game.CurrentAuctionType == AuctionType.BlackMarketOnceAround))
        {
            LogInfo("this is my own card");
            if (Game.CurrentAuctionType == AuctionType.BlackMarketSilent)
                return CreateBidUsingAllyAndRedSpice(0, 0, null);
            return PassedBid();
        }

        if (Game.CurrentAuctionType == AuctionType.BlackMarketSilent || Game.CurrentAuctionType == AuctionType.WhiteSilent)
        {
            if (thisCardIsUseless || thisCardIsCrappy)
            {
                var toBid = thisCardIsUseless || Math.Max(0, Resources - resourcesToKeep) < 2 ? 0 : D(1, 2);
                return CreateBidUsingAllyAndRedSpice(toBid, 0, null);
            }

            return CreateBidUsingAllyAndRedSpice(Math.Min(Math.Max(0, Resources - resourcesToKeep), amountToBidInSilentOrOnceAround), 0, null);
        }

        if (currentBidIsFromAlly || (thisCardIsUseless && currentBid > 0) || (thisCardIsCrappy && D(1, 1 + 2 * currentBid) > 1))
        {
            LogInfo("currentBidIsFromAlly or thisCardIsUseless or thisCardIsCrappy");
            return PassedBid();
        }

        if (Game.CurrentAuctionType == AuctionType.BlackMarketOnceAround || Game.CurrentAuctionType == AuctionType.WhiteOnceAround)
        {
            if (amountToBidInSilentOrOnceAround > currentBid)
                return CreateBidUsingAllyAndRedSpice(amountToBidInSilentOrOnceAround, 0, null);
            return PassedBid();
        }

        if (currentBid == 0 && ResourcesIncludingAllyAndRedContribution > 0)
        {
            LogInfo("always bid at least 1 if possible");
            return CreateBidUsingAllyAndRedSpice(1, 0, null);
        }

        if (Ally != Faction.Red && currentBid > Param.Bidding_PassingTreshold + maximumIWillSpend)
        {
            LogInfo("Ally != Faction.Red && currentBid ({0}) > Param.Bidding_PassingTreshold ({1}) + maximumIWillSpend (D(1,{2}+{3}) => {4})",
                currentBid,
                Param.Bidding_PassingTreshold,
                resourcesAvailable,
                karmaWorth,
                maximumIWillSpend);

            return PassedBid();
        }

        if (currentBid + 2 == maxBidOfOtherPlayer && (karmaCardToUseForBidding != null || currentBid + 2 <= resourcesAvailable))
        {
            LogInfo("currentBid ({0}) + 2 == maxBidOfOtherPlayer ({1}) && (karmaCardToUseForBidding ({2}) != null || currentBid + 2 <= resourcesAvailable ({3}))",
                currentBid,
                maxBidOfOtherPlayer,
                karmaCardToUseForBidding,
                resourcesAvailable
            );

            return CreateBidUsingAllyAndRedSpice(maxBidOfOtherPlayer, resourcesToKeep, karmaCardToUseForBidding);
        }

        if (currentBid + 1 <= resourcesAvailable || karmaCardToUseForBidding != null)
        {
            LogInfo("currentBid ({0}) + 1 <= resourcesAvailable ({1}) || karmaCardToUseForBidding ({2}) != null",
                currentBid,
                resourcesAvailable,
                karmaCardToUseForBidding);

            return CreateBidUsingAllyAndRedSpice(currentBid + 1, resourcesToKeep, karmaCardToUseForBidding);
        }

        LogInfo("Not enough spice available for bid");
        return PassedBid();
    }

    private Bid PassedBid()
    {
        return new Bid(Game, Faction) { Passed = true };
    }

    protected virtual Bid CreateBidUsingAllyAndRedSpice(int amount, int spiceToKeep, TreacheryCard karmaCard)
    {
        if (karmaCard == null)
        {
            var useRedSecretAlly = amount > 6 && ForcesKilled < 12 && Bid.MayUseRedSecretAlly(Game, this);

            var spiceLeftToPay = amount;
            var redContribution = Math.Min(spiceLeftToPay, Game.SpiceForBidsRedCanPay(Faction));
            spiceLeftToPay -= redContribution;

            var allyContribution = Math.Min(spiceLeftToPay, Game.ResourcesYourAllyCanPay(this));
            spiceLeftToPay -= allyContribution;

            return new Bid(Game, Faction) { Amount = spiceLeftToPay, AllyContributionAmount = allyContribution, RedContributionAmount = redContribution, KarmaBid = false, KarmaCard = null, Passed = false, UsesRedSecretAlly = useRedSecretAlly };
        }

        return new Bid(Game, Faction) { Amount = amount, KarmaBid = false, KarmaCard = karmaCard, Passed = false };
    }

    protected bool WannaHave(TreacheryCard c)
    {
        return
            c.IsLaser ||
            c.Type == TreacheryCardType.ProjectileAndPoison ||
            c.Type == TreacheryCardType.ShieldAndAntidote ||
            c.Type == TreacheryCardType.Chemistry ||
            c.Type == TreacheryCardType.WeirdingWay ||
            c.Type == TreacheryCardType.ArtilleryStrike ||
            c.Type == TreacheryCardType.PoisonTooth ||
            c.Type == TreacheryCardType.Amal ||
            (c.IsProjectileWeapon && !TreacheryCards.Any(c => c.IsProjectileWeapon)) ||
            (c.IsPoisonWeapon && !TreacheryCards.Any(c => c.IsPoisonWeapon && c.Type != TreacheryCardType.Chemistry)) ||
            (c.IsProjectileDefense && !TreacheryCards.Any(c => c.IsProjectileDefense && c.Type != TreacheryCardType.WeirdingWay)) ||
            (c.IsPoisonDefense && !TreacheryCards.Any(c => c.IsPoisonDefense)) ||
            c.Type == TreacheryCardType.Karma ||
            (c.Type == TreacheryCardType.Mercenary && Leaders.Count(l => Game.IsAlive(l)) <= 1) ||
            (c.Type == TreacheryCardType.RaiseDead && ForcesKilled > 7) ||
            c.Type == TreacheryCardType.SearchDiscarded ||
            c.Type == TreacheryCardType.TakeDiscarded ||
            c.Type == TreacheryCardType.PortableAntidote ||
            c.Type == TreacheryCardType.Rockmelter;
    }

    protected virtual BlackMarketBid DetermineBlackMarketBid()
    {
        var bid = DetermineBid();
        return new BlackMarketBid(Game, Faction) { Amount = bid.Amount, AllyContributionAmount = bid.AllyContributionAmount, RedContributionAmount = bid.RedContributionAmount, Passed = bid.Passed };
    }
}