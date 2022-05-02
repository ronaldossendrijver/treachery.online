/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
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

        public override Message Validate()
        {
            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " equip a ", TreacheryCardType.PortableAntidote);
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            var card = p.Card(TreacheryCardType.PortableAntidote);
            if (card != null)
            {
                var plan = g.CurrentBattle?.PlanOf(p);
                if (plan != null && plan.Defense == null && g.CurrentPortableAntidoteUsed == null && Battle.ValidDefenses(g, p, plan.Weapon, false).Contains(card))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
