/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Newtonsoft.Json;

namespace Treachery.Shared;

public class BlackMarketBid : PassableGameEvent, IBid
{
    #region Construction

    public BlackMarketBid(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public BlackMarketBid()
    {
    }

    #endregion Construction

    #region Properties

    public int Amount { get; set; }

    public int AllyContributionAmount { get; set; }

    public int RedContributionAmount { get; set; }

    [JsonIgnore]
    public bool UsesRedSecretAlly => false;

    [JsonIgnore]
    public int TotalAmount => Amount + AllyContributionAmount + RedContributionAmount;

    [JsonIgnore]
    public bool UsingKarmaToRemoveBidLimit => false;

    [JsonIgnore]
    public TreacheryCard KarmaCard => null;

    #endregion Properties

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.BidSequence.NextPlayer();
        Game.SkipPlayersThatCantBid(Game.BidSequence);
        Game.Stone(Milestone.Bid);
        Game.Bids.Remove(Initiator);
        Game.Bids.Add(Initiator, this);

        if (!Passed) Game.CurrentBid = this;

        switch (Game.CurrentAuctionType)
        {
            case AuctionType.BlackMarketNormal:
                HandleBlackMarketNormalBid();
                break;

            case AuctionType.BlackMarketOnceAround:
            case AuctionType.BlackMarketSilent:
                HandleBlackMarketSpecialBid();
                break;
        }
    }

    private void HandleBlackMarketNormalBid()
    {
        if (Passed)
        {
            var winningBid = Game.CurrentBid;

            if (winningBid != null && Game.BidSequence.CurrentFaction == winningBid.Initiator)
            {
                var card = Game.WinByHighestBid(
                    winningBid.Player,
                    winningBid,
                    winningBid.Amount,
                    winningBid.AllyContributionAmount,
                    winningBid.RedContributionAmount,
                    winningBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                    Game.CardsOnAuction, false);

                FinishBlackMarketBid(winningBid.Player, card);
            }
            else if (winningBid == null && Game.Bids.Count >= Game.PlayersThatCanBid.Count())
            {
                GetPlayer(Faction.White).TreacheryCards.Add(Game.CardsOnAuction.Draw());
                Log(Faction.White, " keep their card as no faction bid on it");
                Game.EnterWhiteBidding();
            }
        }
    }

    private void HandleBlackMarketSpecialBid()
    {
        var isLastBid = Game.Version < 140 ? Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count :
            (Game.CurrentAuctionType == AuctionType.BlackMarketSilent && Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count) ||
            (Game.CurrentAuctionType == AuctionType.BlackMarketOnceAround && Initiator == Faction.White) || (!GetPlayer(Faction.White).HasRoomForCards && Game.BidSequence.HasPassedWhite);

        if (isLastBid)
        {
            if (Game.CurrentAuctionType == AuctionType.BlackMarketSilent) Log(Game.Bids.Select(b => MessagePart.Express(b.Key, Payment.Of(b.Value.TotalAmount), " ")).ToList());

            var highestBid = Game.DetermineHighestBid(Game.Bids);
            if (highestBid != null && highestBid.TotalAmount > 0)
            {
                var card = Game.WinByHighestBid(
                    highestBid.Player,
                    highestBid,
                    highestBid.Amount,
                    highestBid.AllyContributionAmount,
                    highestBid.RedContributionAmount,
                    highestBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                    Game.CardsOnAuction, false);

                FinishBlackMarketBid(highestBid.Player, card);
            }
            else
            {
                GetPlayer(Faction.White).TreacheryCards.Add(Game.CardsOnAuction.Draw());
                Log(Faction.White, " keep their card as no faction bid on it");
                Game.EnterWhiteBidding();
            }
        }
    }

    private void FinishBlackMarketBid(Player winner, TreacheryCard card)
    {
        Game.CardJustWon = card;
        Game.WinningBid = Game.CurrentBid;
        Game.CardSoldOnBlackMarket = card;
        Game.FactionThatMayReplaceBoughtCard = Faction.None;

        var enterReplacingCardJustWon = winner != null && Game.Version > 150 && Game.Players.Any(p => p.Nexus != Faction.None);

        if (winner != null)
        {
            if (winner.Ally == Faction.Grey && Game.GreyAllowsReplacingCards)
            {
                if (!Game.Prevented(FactionAdvantage.GreyAllyDiscardingCard))
                {
                    Game.FactionThatMayReplaceBoughtCard = winner.Faction;
                    enterReplacingCardJustWon = true;
                }
                else
                {
                    if (!Game.Applicable(Rule.FullPhaseKarma)) Game.Allow(FactionAdvantage.GreyAllyDiscardingCard);

                    if (Game.NexusAllowsReplacingBoughtCards(winner))
                    {
                        Game.FactionThatMayReplaceBoughtCard = winner.Faction;
                        Game.ReplacingBoughtCardUsingNexus = true;
                        enterReplacingCardJustWon = true;
                    }
                }
            }
            else if (Game.NexusAllowsReplacingBoughtCards(winner))
            {
                Game.FactionThatMayReplaceBoughtCard = winner.Faction;
                enterReplacingCardJustWon = true;
                Game.ReplacingBoughtCardUsingNexus = true;
            }
        }

        Game.Enter(enterReplacingCardJustWon, Phase.ReplacingCardJustWon, Game.EnterWhiteBidding);

        if (Game.BiddingTriggeredBureaucracy != null)
        {
            Game.ApplyBureaucracy(Game.BiddingTriggeredBureaucracy.PaymentFrom, Game.BiddingTriggeredBureaucracy.PaymentTo);
            Game.BiddingTriggeredBureaucracy = null;
        }
    }

    public override Message GetMessage()
    {
        if (!Passed)
            return Message.Express(Initiator, " bid");
        return Message.Express(Initiator, " pass");
    }

    #endregion Execution

    #region Validation

    public override Message Validate()
    {
        if (Passed) return null;

        if (Game.CurrentAuctionType != AuctionType.BlackMarketSilent && TotalAmount < 1) return Message.Express("Bid must be higher than 0");
        if (Game.CurrentAuctionType != AuctionType.BlackMarketSilent && Game.CurrentBid != null && TotalAmount <= Game.CurrentBid.TotalAmount) return Message.Express("Bid not high enough");

        if (AllyContributionAmount > ValidMaxAllyAmount(Game, Player)) return Message.Express("your ally won't pay that much");

        var red = Game.GetPlayer(Faction.Red);
        if (RedContributionAmount > 0 && RedContributionAmount > red.Resources) return Message.Express(Faction.Red, " won't pay that much");

        if (Game.Version >= 155 && Game.CurrentAuctionType == AuctionType.BlackMarketSilent && TotalAmount > Player.Resources) return Message.Express("In a Silent auction, you can't bid more than you have");

        return null;
    }

    public static int ValidMaxAmount(Player p)
    {
        return p.Resources;
    }

    public static int ValidMaxAllyAmount(Game g, Player p)
    {
        return g.ResourcesYourAllyCanPay(p);
    }

    public static IEnumerable<SequenceElement> PlayersToBid(Game g)
    {
        return g.CurrentAuctionType switch
        {
            AuctionType.BlackMarketNormal or AuctionType.BlackMarketOnceAround => g.BidSequence.GetPlayersInSequence(),
            AuctionType.BlackMarketSilent => g.Players.Select(p => new SequenceElement { Player = p, HasTurn = p.HasRoomForCards && !g.Bids.ContainsKey(p.Faction) }),
            _ => Array.Empty<SequenceElement>()
        };
    }

    public static bool MayBePlayed(Game game, Player player)
    {
        return (game.CurrentAuctionType == AuctionType.BlackMarketSilent && !game.Bids.ContainsKey(player.Faction) && player.HasRoomForCards) ||
               (game.CurrentAuctionType != AuctionType.BlackMarketSilent && player == game.BidSequence.CurrentPlayer);
    }

    #endregion Validation
}