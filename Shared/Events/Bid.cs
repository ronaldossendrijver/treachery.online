/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class Bid : PassableGameEvent, IBid
{
    #region Construction

    public Bid(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public Bid()
    {
    }

    #endregion

    #region Properties

    public int Amount { get; set; }

    public int AllyContributionAmount { get; set; }

    public int RedContributionAmount { get; set; }

    public bool UsesRedSecretAlly { get; set; }

    public int _karmaCardId = -1;
    
    [JsonIgnore]
    public TreacheryCard? KarmaCard
    {
        get => TreacheryCardManager.Lookup.Find(_karmaCardId);
        set => _karmaCardId = TreacheryCardManager.GetId(value);
    }

    [JsonIgnore]
    public int TotalAmount => Amount + AllyContributionAmount + RedContributionAmount;

    /// <summary>
    /// This indicates Karma was used to remove the bid amount limit
    /// </summary>
    [MemberNotNullWhen(true, nameof(KarmaCard))]
    [JsonIgnore]
    public bool UsingKarmaToRemoveBidLimit => KarmaCard != null && !KarmaBid;

    /// <summary>
    /// This indicates the card is won immediately
    /// </summary>
    public bool KarmaBid { get; set; }

    #endregion

    #region Validation

    public override Message? Validate()
    {
        if ((Game.CurrentAuctionType == AuctionType.BlackMarketSilent || Game.CurrentAuctionType == AuctionType.WhiteSilent) && Passed) return Message.Express("You cannot pass a silent bid");

        if (Passed) return null;

        var isSpecialAuction = Game.CurrentAuctionType == AuctionType.WhiteOnceAround || Game.CurrentAuctionType == AuctionType.WhiteSilent;
        if (KarmaBid && isSpecialAuction) return Message.Express("You can't use ", TreacheryCardType.Karma, " in Once Around or Silent bidding");

        if (KarmaBid && !CanKarma(Game, Player)) return Message.Express("You can't use ", TreacheryCardType.Karma, " for this bid");

        if (KarmaBid) return null;

        var p = Game.GetPlayer(Initiator);
        if (TotalAmount < 1 && Game.CurrentAuctionType != AuctionType.WhiteSilent) return Message.Express("Bid must be higher than 0");
        if (Game.CurrentBid != null && TotalAmount <= Game.CurrentBid.TotalAmount && Game.CurrentAuctionType != AuctionType.WhiteSilent) return Message.Express("Bid not high enough");

        if (AllyContributionAmount > ValidMaxAllyAmount(Game, Player)) return Message.Express("your ally won't pay that much");

        var red = Game.GetPlayer(Faction.Red);
        if (RedContributionAmount > 0 && RedContributionAmount > (red?.Resources ?? 0)) return Message.Express(Faction.Red, " won't pay that much");

        if (!UsingKarmaToRemoveBidLimit && Amount > Player.Resources) return Message.Express("You can't pay ", Payment.Of(Amount));
        if (KarmaCard != null && (p == null || !Karma.ValidKarmaCards(Game, p).Contains(KarmaCard))) return Message.Express("Invalid ", TreacheryCardType.Karma, " card");

        if (UsesRedSecretAlly && !MayUseRedSecretAlly(Game, Player)) return Message.Express("You can't use ", Faction.Red, " cunning");

        if (Game.Version >= 155 && Game.CurrentAuctionType == AuctionType.WhiteSilent && TotalAmount > Player.Resources) return Message.Express("In a Silent auction, you can't bid more than you have");

        return null;
    }

    public static int ValidMaxAmount(Player p, bool usingKarma)
    {
        if (usingKarma)
            return 100;
        return p.Resources;
    }

    public static int ValidMaxAllyAmount(Game g, Player? p)
    {
        if (p == null) return 0;
        return g.ResourcesYourAllyCanPay(p);
    }

    public static IEnumerable<SequenceElement> PlayersToBid(Game g)
    {
        var sequencePlayers = g.BidSequence?.GetPlayersInSequence() ?? Array.Empty<SequenceElement>();
        return g.CurrentAuctionType switch
        {
            AuctionType.Normal or AuctionType.WhiteOnceAround => sequencePlayers,
            AuctionType.WhiteSilent => g.Players.Select(p => new SequenceElement { Player = p, HasTurn = p.HasRoomForCards && !g.Bids.Keys.Contains(p.Faction) }),
            _ => Array.Empty<SequenceElement>()
        };
    }

    public static IEnumerable<TreacheryCard> ValidKarmaCards(Game g, Player? p)
    {
        if (p == null) return Array.Empty<TreacheryCard>();
        if (g.CurrentAuctionType == AuctionType.Normal)
            return Karma.ValidKarmaCards(g, p);
        return Array.Empty<TreacheryCard>();
    }

    public static bool CanKarma(Game g, Player? p)
    {
        return ValidKarmaCards(g, p).Any();
    }

    public static bool MayBePlayed(Game game, Player? player)
    {
        if (player == null) return false;
        return (game.CurrentAuctionType == AuctionType.WhiteSilent && !game.Bids.ContainsKey(player.Faction) && player.HasRoomForCards) ||
               (game.CurrentAuctionType != AuctionType.WhiteSilent && player == game.BidSequence?.CurrentPlayer);
    }

    public static bool MayUseRedSecretAlly(Game game, Player player)
    {
        return game.CurrentAuctionType == AuctionType.Normal && player.Nexus == Faction.Red &&
               NexusPlayed.CanUseSecretAlly(game, player);
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Game.ExecuteBid(this);

        var playerToBidWithPassNormalBid = Game.Players.FirstOrDefault(p => MayBePlayed(Game, p) && Game.IsAutoPassedBid(p.Faction));
        while (playerToBidWithPassNormalBid != null)
        {
            Game.ExecuteBid(new Bid(Game, playerToBidWithPassNormalBid.Faction) { Passed = true });
            playerToBidWithPassNormalBid = Game.Players.FirstOrDefault(p => MayBePlayed(Game, p) && Game.IsAutoPassedBid(p.Faction));
        }
    }

    public void HandleNormalBid()
    {
        if (Passed || KarmaBid)
        {
            if (KarmaBid)
            {
                //Immediate Karma
                var card = WinWithKarma(this);
                Game.FinishBid(Player, this, card, true);
            }
            else if (Game.CurrentBid is Bid winningBid && Game.BidSequence?.CurrentFaction == Game.CurrentBid.Initiator)
            {
                if (winningBid.UsingKarmaToRemoveBidLimit)
                {
                    //Karma was used to bid any amount
                    Game.ReturnKarmaCardUsedForBid();
                    var card = WinWithKarma(winningBid);
                    Game.FinishBid(winningBid.Player, winningBid, card, true);
                }
                else
                {
                    if (Game.CardsOnAuction == null) return;

                    var receiver = Faction.Red;
                    var card = Game.WinByHighestBid(
                        winningBid.Player,
                        winningBid,
                        winningBid.Amount,
                        winningBid.AllyContributionAmount,
                        winningBid.RedContributionAmount,
                        receiver,
                        Game.CardsOnAuction,
                        winningBid.UsesRedSecretAlly);

                    Game.FinishBid(winningBid.Player, winningBid, card, true);
                }
            }
            else if (Game.CurrentBid == null && Game.Bids.Count >= (Game.PlayersThatCanBid.Count()))
            {
                EveryonePassedBid();
            }
        }
        else if (Game.BidSequence?.CurrentFaction == Initiator)
        {
            if (Game.CardsOnAuction == null || Game.CurrentBid == null) return;

            var card = BidWonByOnlyPlayer(this, Faction.Red, Game.CardsOnAuction);
            Game.FinishBid(Game.CurrentBid.Player, this, card, true);
        }
    }

    private TreacheryCard WinWithKarma(Bid bid)
    {
        var winner = GetPlayer(bid.Initiator);
        if (winner == null || bid.KarmaCard == null || Game.CardsOnAuction == null)
            throw new InvalidOperationException("Cannot resolve karma bid winner or auction deck");
        var karmaCard = bid.KarmaCard;

        Game.Discard(karmaCard);

        if (karmaCard.Type == TreacheryCardType.Karma)
            Log(bid.Initiator, " get card ", Game.CardNumber, " using ", TreacheryCardType.Karma);
        else
            Log(bid.Initiator, " get card ", Game.CardNumber, " using ", karmaCard, " for ", TreacheryCardType.Karma);

        Game.Stone(Milestone.AuctionWon);
        Game.Stone(Milestone.Karma);

        var card = Game.CardsOnAuction.Draw();
        winner.TreacheryCards.Add(card);
        Game.RegisterWonCardAsKnown(card);
        LogTo(winner.Faction, "You won: ", card);
        Game.GivePlayerExtraCardIfApplicable(winner);
        return card;
    }

    private void EveryonePassedBid()
    {
        Log("Bid is passed by everyone; bidding ends and remaining cards are returned to the Treachery Deck");
        Game.Stone(Milestone.AuctionWon);

        if (Game.CardsOnAuction == null) return;

        while (!Game.CardsOnAuction.IsEmpty)
        {
            if (Game.Version >= 131) Game.CardsOnAuction.Shuffle();

            var card = Game.CardsOnAuction.Draw();
            Game.TreacheryDeck!.PutOnTop(card);
        }

        Game.EndBiddingPhase();
    }

    private TreacheryCard BidWonByOnlyPlayer(Bid bid, Faction paymentReceiver, Deck<TreacheryCard> toDrawFrom)
    {
        Game.CurrentBid = bid;
        var winner = GetPlayer(Game.CurrentBid.Initiator);
        if (winner == null) throw new InvalidOperationException("Cannot resolve winner for single-player bid");
        var receiverIncomeMessage = MessagePart.Express();

        if (!bid.UsesRedSecretAlly)
        {
            Game.PayForCard(winner, bid, Game.CurrentBid.Amount, Game.CurrentBid.AllyContributionAmount, Game.CurrentBid.RedContributionAmount, paymentReceiver, ref receiverIncomeMessage);
            Game.LogBid(winner, Game.CurrentBid.Amount, Game.CurrentBid.AllyContributionAmount, Game.CurrentBid.RedContributionAmount, receiverIncomeMessage);
        }
        else
        {
            Game.PlayNexusCard(winner, "Secret Ally", "get this card for free");
            Game.LogBid(winner, 0, 0, 0, receiverIncomeMessage);
        }

        Game.Stone(Milestone.AuctionWon);
        var card = toDrawFrom.Draw();
        Game.RegisterWonCardAsKnown(card);
        winner.TreacheryCards.Add(card);
        LogTo(winner.Faction, "You won: ", card);
        Game.GivePlayerExtraCardIfApplicable(winner);
        return card;
    }

    public void HandleWhiteBid()
    {
        var bidSequenceHasPassedWhite = Game.BidSequence?.HasPassedWhite == true;
        var whiteHasNoRoom = GetPlayer(Faction.White)?.HasRoomForCards == false;

        var isLastBid = Game.Version < 140 ? Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count :
            (Game.CurrentAuctionType == AuctionType.WhiteSilent && Game.Players.Count(p => p.HasRoomForCards) == Game.Bids.Count) ||
            (Game.Version < 151 && ((Game.CurrentAuctionType == AuctionType.WhiteOnceAround && Initiator == Faction.White) || (whiteHasNoRoom && bidSequenceHasPassedWhite))) ||
            (Game.Version >= 151 && Game.CurrentAuctionType == AuctionType.WhiteOnceAround && (Initiator == Faction.White || (whiteHasNoRoom && bidSequenceHasPassedWhite)));

        if (isLastBid)
        {
            if (Game.CurrentAuctionType == AuctionType.WhiteSilent) Log("Bids: ", Game.Bids.Select(b => MessagePart.Express(b.Key, Payment.Of(b.Value.TotalAmount), " ")).ToList());

            var highestBid = Game.DetermineHighestBid(Game.Bids);
            if (highestBid is { TotalAmount: > 0 } winningBid)
            {
                var cardsOnAuction = Game.CardsOnAuction;
                if (cardsOnAuction == null) return;

                var card = Game.WinByHighestBid(
                    winningBid.Player,
                    winningBid,
                    winningBid.Amount,
                    winningBid.AllyContributionAmount,
                    winningBid.RedContributionAmount,
                    winningBid.Initiator != Faction.White ? Faction.White : Faction.Red,
                    cardsOnAuction, false);

                Game.FinishBid(winningBid.Player, winningBid, card, true);
            }
            else
            {
                Log("Card not sold as no faction bid on it");
                var white = GetPlayer(Faction.White);
                if (white?.HasRoomForCards == true)
                {
                    Game.Enter(Phase.WhiteKeepingUnsoldCard);
                }
                else
                {
                    var cardsOnAuction = Game.CardsOnAuction;
                    if (cardsOnAuction == null) return;
                    var card = cardsOnAuction.Draw();
                    Game.RemovedTreacheryCards.Add(card);
                    Game.RegisterWonCardAsKnown(card);
                    Log(card, " was removed from the game");
                    Game.FinishBid(null, null, card, false);
                }
            }
        }
    }

    public override Message GetMessage()
    {
        if (Passed) return Message.Express(Initiator, " pass");
        
        return KarmaBid 
            ? Message.Express(Initiator, " win the bid using ", TreacheryCardType.Karma) 
            : Message.Express(Initiator, " bid");
    }

    #endregion

}