/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Donated : GameEvent
    {
        public Donated(Game game) : base(game)
        {
        }

        public Donated()
        {
        }

        public Faction Target { get; set; }

        public int Resources { get; set; }

        public int _cardId = -1;

        public bool FromBank { get; set; } = false;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get
            {
                return TreacheryCardManager.Lookup.Find(_cardId);
            }
            set
            {
                _cardId = TreacheryCardManager.Lookup.GetId(value);
            }
        }

        public override Message Validate()
        {
            if (FromBank) return null;

            var p = Player;
            if (!MayDonate(Game, Player)) return Message.Express("You currently have an outstanding bid");
            if (Card == null && Resources <= 0) return Message.Express("Invalid amount");
            if (p.Resources < Resources) return Message.Express("You can't give that much");
            if (Initiator != Faction.Red && Target == p.Ally) return Message.Express("You can't bribe your ally");
            if (Initiator != Faction.Red && Game.Applicable(Rule.DisableResourceTransfers)) return Message.Express(Concept.Resource, " transfers are disabled by house rule");
            if (!FromBank && (Game.EconomicsStatus == BrownEconomicsStatus.Double || Game.EconomicsStatus == BrownEconomicsStatus.DoubleFlipped)) Message.Express("No bribes can be made during Double Inflation");

            var targetPlayer = Game.GetPlayer(Target);
            if (Card != null && !targetPlayer.HasRoomForCards) return Message.Express("Target faction's hand is full");

            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.Players.Where(x => x.Faction != p.Faction && (p.Is(Faction.Red) || x.Faction != p.Ally)).Select(x => x.Faction);
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " give ", new Payment(Resources), MessagePart.ExpressIf(Card != null, " and a card"), " to ", Target);
        }

        public static bool MayDonate(Game g, Player p)
        {
            if (g.CurrentPhase == Phase.Bidding && g.CurrentBid != null && g.CurrentBid.Initiator == p.Faction) return false;

            if (g.CurrentPhase == Phase.Bidding && g.CurrentBid != null && g.CurrentBid.AllyContributionAmount > 0 && g.CurrentBid.Player.Ally == p.Faction) return false;

            return true;
        }

        public static int MinAmount(Game g, Player p)
        {
            if (!g.Applicable(Rule.CardsCanBeTraded))
            {
                return Math.Min(p.Resources, 1);
            }
            else
            {
                return 0;
            }
        }

        public static int MaxAmount(Player p)
        {
            return p.Resources;
        }
    }
}
