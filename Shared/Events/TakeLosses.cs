/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class TakeLosses : GameEvent
    {
        public TakeLosses(Game game) : base(game)
        {
        }

        public TakeLosses()
        {
        }

        public int ForceAmount { get; set; }

        public int SpecialForceAmount { get; set; }

        public bool UseUselessCard { get; set; }

        public override string Validate()
        {
            if (UseUselessCard && ValidUselessCardToPreventLosses(Game, Player) == null) return "You can't use a card to prevent force losses";
            if (UseUselessCard) return "";

            int valueToBeKilled = LossesToTake(Game).Amount;
            if (ForceAmount + 2 * SpecialForceAmount < valueToBeKilled) return string.Format("Select a total value of at least {0} to be killed.", valueToBeKilled);

            return "";
        }

        public static LossToTake LossesToTake(Game g)
        {
            return g.StormLossesToTake[0];
        }

        public static int ValidMaxForceAmount(Game g, Player p)
        {
            return p.ForcesIn(LossesToTake(g).Location);
        }

        public static int ValidMaxSpecialForceAmount(Game g, Player p)
        {
            return p.SpecialForcesIn(LossesToTake(g).Location);
        }

        public static TreacheryCard ValidUselessCardToPreventLosses(Game g, Player p)
        {
            if (p.Faction == Faction.Brown && !g.Prevented(FactionAdvantage.BrownDiscarding))
            {
                return p.TreacheryCards.FirstOrDefault(c => c.Id == TreacheryCardManager.CARD_JUBBACLOAK);
            }

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (SpecialForceAmount > 0)
            {
                var p = Player;
                return new Message(Initiator, "The storm kills {0} {1} forces and {2} {3}.", ForceAmount, Initiator, SpecialForceAmount, p.SpecialForce);
            }
            else
            {
                return new Message(Initiator, "The storm kills {0} {1} forces.", ForceAmount, Initiator);
            }
        }
    }
}
