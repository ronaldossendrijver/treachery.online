/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class Donated : GameEvent
    {
        #region Construction

        public Donated(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Donated()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

        public int Resources { get; set; }

        public bool FromBank { get; set; } = false;

        public int _cardId = -1;

        [JsonIgnore]
        public TreacheryCard Card
        {
            get => TreacheryCardManager.Lookup.Find(_cardId);
            set => _cardId = TreacheryCardManager.Lookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (FromBank) return null;

            if (!MayDonate(Game, Player)) return Message.Express("You currently have an outstanding bid");
            if (Card == null && Resources <= 0) return Message.Express("Invalid amount");
            if (Player.Resources < Resources) return Message.Express("You can't give that much");
            if (Initiator != Faction.Red && Target == Player.Ally) return Message.Express("You can't bribe your ally");
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

        public static bool MayDonate(Game g, Player p)
        {
            if (g.HasBidToPay(p)) return false;

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

        public static int MaxAmount(Player p) => p.Resources;

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var target = GetPlayer(Target);

            if (!FromBank)
            {
                Game.ExchangeResourcesInBribe(Player, target, Resources);

                if (Card != null)
                {
                    Player.TreacheryCards.Remove(Card);
                    Game.RegisterKnown(Player, Card);
                    target.TreacheryCards.Add(Card);

                    foreach (var p in Game.Players.Where(p => !p.Is(Initiator) && p != target))
                    {
                        Game.UnregisterKnown(p, Player.TreacheryCards);
                        Game.UnregisterKnown(p, target.TreacheryCards);
                    }
                }

                Log();
                Game.Stone(Milestone.Bribe);
            }
            else
            {
                if (Resources < 0)
                {
                    int resourcesToTake = Math.Min(Math.Abs(Resources), target.Resources);
                    Log("Host puts ", Payment.Of(resourcesToTake), " from ", Target, " into the ", Concept.Resource, " Bank");
                    target.Resources -= resourcesToTake;
                }
                else
                {
                    Log("Host gives ", Target, Payment.Of(Resources), " from the ", Concept.Resource, " Bank");
                    target.Resources += Resources;
                }
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " give ", Payment.Of(Resources), MessagePart.ExpressIf(Card != null, " and a card"), " to ", Target);
        }

        #endregion Execution
    }
}
