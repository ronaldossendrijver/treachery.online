/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            if (FromBank) return "";

            var p = Player;
            if (!MayDonate(Game, Player)) return "You currently have an outstanding bid";
            if (Card == null && Resources <= 0) return "Invalid amount";
            if (p.Resources < Resources) return "You can't give that much";
            if (Initiator != Faction.Red && Target == p.Ally) return "You can't bribe your ally";

            var targetPlayer = Game.GetPlayer(Target);
            if (Card != null && !targetPlayer.HasRoomForCards) return "Target faction's hand is full";

            return "";
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
            return new Message(Initiator, "{0} give {1}{3} to {2}.", Initiator, Resources, Target, CardMessage);
        }

        public static bool MayDonate(Game g, Player p)
        {
            if (g.CurrentPhase == Phase.Bidding && g.CurrentBid != null && g.CurrentBid.Initiator == p.Faction) return false;

            if (g.Version >= 100 && g.CurrentPhase == Phase.Bidding && g.CurrentBid != null && g.CurrentBid.AllyContributionAmount > 0 && g.CurrentBid.Player.Ally == p.Faction) return false;

            return true;
        }

        private string CardMessage
        {
            get
            {
                return Card == null ? "" : " and a card ";
            }
        }

        public static IEnumerable<int> ValidAmounts(Game g, Player p)
        {
            if (!g.Applicable(Rule.CardsCanBeTraded))
            {
                return Enumerable.Range(1, p.Resources);
            }
            else
            {
                return Enumerable.Range(0, p.Resources + 1);
            }
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

        public static int MaxAmount(Game g, Player p)
        {
            return p.Resources;
        }
    }
}
