/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared;

public class DealAccepted : GameEvent
{
    #region Construction

    public DealAccepted(Game game, Faction initiator) : base(game, initiator)
    {
    }

    public DealAccepted()
    {
    }

    #endregion Construction

    #region Properties

    public Faction BoundFaction { get; set; }

    public DealType Type { get; set; }

    public string DealParameter1 { get; set; }

    public string DealParameter2 { get; set; }

    public string Text { get; set; }

    public Phase End { get; set; }

    public int Price { get; set; }

    public int Benefit { get; set; }

    #endregion Properties

    #region Validation

    public override Message Validate()
    {
        if (!MayDeal(Game, Player, Price)) return Message.Express("You currently have an outstanding bid");
        if (Game.Version >= 142 && !MayDeal(Game, Game.GetPlayer(BoundFaction), Benefit)) return Message.Express(BoundFaction, " currently have an outstanding bid");
        if (Price > Player.Resources) return Message.Express("You can't pay that much");

        var boundPlayer = Game.GetPlayer(BoundFaction);
        if (Benefit > boundPlayer.Resources) return Message.Express("Offer is not valid (anymore)");

        if ((Price > 0 || (Game.Version >= 142 && Benefit > 0)) && Game.Applicable(Rule.DisableResourceTransfers)) return Message.Express(Concept.Resource, " transfers are disabled by house rule");
        if (!Game.DealOffers.Any(offer => offer.IsAcceptedBy(this))) return Message.Express("Offer is not valid (anymore)");

        return null;
    }

    public static bool MayDeal(Game g, Player p, int price)
    {
        return
            g.CurrentPhase != Phase.Bidding ||
            price <= 0 ||
            g.CurrentBid == null ||
            (g.CurrentBid.Initiator != p.Faction && (g.CurrentBid.AllyContributionAmount == 0 || g.CurrentBid.Player.Ally != p.Faction));
    }

    public static IEnumerable<DealOffered> AcceptableDeals(Game g, Player p)
    {
        return g.DealOffers.Where(offer =>
            offer.Initiator != p.Faction &&
            offer.Initiator != p.Ally &&
            (offer.To.Length == 0 || offer.To.Contains(p.Faction)) &&
            (g.EconomicsStatus != BrownEconomicsStatus.Double || (offer.Benefit == 0 && offer.Price == 0)));
    }

    public static IEnumerable<DealOffered> CancellableDeals(Game g, Player p)
    {
        return g.DealOffers.Where(offer => offer.Initiator == p.Faction);
    }

    public static IEnumerable<Deal> CurrentDeals(Game g)
    {
        return g.Deals;
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        Log();
        var offer = Game.DealOffers.FirstOrDefault(offer => offer.IsAcceptedBy(this));

        if (offer != null)
        {
            var newDeal = new Deal
            {
                BoundFaction = offer.Initiator,
                ConsumingFaction = Initiator,
                DealParameter1 = DealParameter1,
                DealParameter2 = DealParameter2,
                End = End,
                Text = Text,
                Benefit = Benefit,
                Type = Type
            };

            Game.StartDeal(newDeal);

            if (Price > 0)
            {
                Game.ExchangeResourcesInBribe(Player, GetPlayer(offer.Initiator), Price);
                Game.Stone(Milestone.Bribe);
            }

            if (Benefit > 0)
            {
                Game.ExchangeResourcesInBribe(GetPlayer(offer.Initiator), Player, Benefit);
                Game.Stone(Milestone.Bribe);
            }

            if (offer.Player.IsBot) HandleAcceptedBotDeal(offer);
        }
    }

    private void HandleAcceptedBotDeal(DealOffered offer)
    {
        if (offer.Type == DealType.TellDiscardedTraitors)
        {
            LogTo(Initiator, offer.Initiator, " discarded: ", offer.Player.DiscardedTraitors);
            Log(offer.Initiator, " gave ", Initiator, " the agreed information");
        }
    }

    public override Message GetMessage()
    {
        return Message.Express(
            Initiator,
            " accept ",
            BoundFaction,
            " offer for ",
            Payment.Of(Price),
            ": ",
            GetDealDescription());
    }

    public override Message GetShortMessage()
    {
        return Message.Express(Initiator, " accept a deal by ", BoundFaction);
    }

    public Message GetDealDescription()
    {
        return Deal.DealContentsDescription(Game, Type, Text, Benefit, End, DealParameter1);
    }

    public Message GetDealContents()
    {
        return Text != null && Text.Length > 0 ? Message.Express(Text) : Message.Express(Type);
    }

    #endregion Execution
}