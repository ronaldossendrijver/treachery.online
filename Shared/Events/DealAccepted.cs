/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

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

        public override string Validate()
        {
            if (!MayAcceptDeals(Game, Player, Price)) return "You currently have an outstanding bid";
            if (Price > Player.Resources) return "You can't pay that much";

            var boundPlayer = Game.GetPlayer(BoundFaction);
            if (Benefit > boundPlayer.Resources) return "Offer is not valid (anymore)";

            if (Price > 0 && Game.Applicable(Rule.DisableResourceTransfers)) return Skin.Current.Format("{0} transfers are disabled by house rule", Concept.Resource);
            if (!Game.DealOffers.Any(offer => offer.IsAcceptedBy(this))) return "Offer is not valid (anymore)";

            return "";
        }

        public static bool MayAcceptDeals(Game g, Player p, int price)
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
                Deal.DealContentsDescription(Game, Type, Text, Benefit, End, DealParameter1, DealParameter2));
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<DealOffered> AcceptableDeals(Game g, Player p)
        {
            return g.DealOffers.Where(offer => offer.Initiator != p.Faction && offer.Initiator != p.Ally && (offer.To.Length == 0 || offer.To.Contains(p.Faction)));
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
