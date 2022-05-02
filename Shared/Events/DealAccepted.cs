/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class DealAccepted : GameEvent
    {
        public Faction BoundFaction { get; set; }

        public DealType Type { get; set; }

        public string DealParameter1 { get; set; }

        public string DealParameter2 { get; set; }

        public string Text { get; set; }

        public Phase End { get; set; }

        public int Price;

        public int Benefit;

        public DealAccepted(Game game) : base(game)
        {
        }

        public DealAccepted()
        {
        }

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
                g.CurrentBid.Initiator != p.Faction && (g.CurrentBid.AllyContributionAmount == 0 || g.CurrentBid.Player.Ally != p.Faction);
        }

        public override Message GetMessage()
        {
            return Message.Express(
                Initiator,
                " accept ",
                BoundFaction,
                " offer for ",
                new Payment(Price),
                ": ",
                Deal.DealContentsDescription(Game, Type, Text, Benefit, End, DealParameter1));
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<DealOffered> AcceptableDeals(Game g, Player p)
        {
            if (g.EconomicsStatus != BrownEconomicsStatus.Double)
            {
                return g.DealOffers.Where(offer => offer.Initiator != p.Faction && offer.Initiator != p.Ally && (offer.To.Length == 0 || offer.To.Contains(p.Faction)));
            }
            else
            {
                return Array.Empty<DealOffered>();
            }
        }

        public static IEnumerable<DealOffered> CancellableDeals(Game g, Player p)
        {
            return g.DealOffers.Where(offer => offer.Initiator == p.Faction);
        }

        public static IEnumerable<Deal> CurrentDeals(Game g)
        {
            return g.Deals;
        }
    }
}
