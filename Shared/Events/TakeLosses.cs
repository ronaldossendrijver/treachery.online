/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

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

            if (Game.Version >= 120)
            {
                if (ForceAmount > ValidMaxForceAmount(Game, Player)) return "Invalid amount of forces";
                if (SpecialForceAmount > ValidMaxSpecialForceAmount(Game, Player)) return "Invalid amount of forces";
            }

            return "";
        }

        public static LossToTake LossesToTake(Game g)
        {
            if (g.StormLossesToTake.Count > 0)
            {
                return g.StormLossesToTake[0];
            }
            else
            {
                return new LossToTake();
            }
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
            var p = Player;
            return Message.Express(
                "The storm kills ",
                MessagePart.ExpressIf(ForceAmount > 0, ForceAmount, p.Force),
                MessagePart.ExpressIf(ForceAmount > 0 && SpecialForceAmount > 0, " and "),
                MessagePart.ExpressIf(SpecialForceAmount > 0, SpecialForceAmount, p.SpecialForce));
        }
    }
}
