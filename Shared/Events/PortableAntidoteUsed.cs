/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class PortableAntidoteUsed : GameEvent
    {
        public PortableAntidoteUsed(Game game) : base(game)
        {
        }

        public PortableAntidoteUsed()
        {
        }

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return new Message(Initiator, "{0} use a {1}.", Initiator, TreacheryCardType.PortableAntidote);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            var card = p.Card(TreacheryCardType.PortableAntidote);
            if (card != null)
            {
                var plan = g.CurrentBattle?.PlanOf(p);
                if (plan != null && plan.Defense == null && Battle.ValidDefenses(g, p, plan.Weapon, false).Contains(card))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
