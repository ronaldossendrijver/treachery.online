/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Linq;

namespace Treachery.Shared
{
    public class PortableAntidoteUsed : GameEvent
    {
        #region Construction

        public PortableAntidoteUsed(Game game) : base(game)
        {
        }

        public PortableAntidoteUsed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            var card = p.Card(TreacheryCardType.PortableAntidote);
            if (card != null)
            {
                var plan = g.CurrentBattle?.PlanOf(p);
                if (plan != null && plan.Defense == null && g.CurrentPortableAntidoteUsed == null && Battle.ValidDefenses(g, p, plan.Weapon, g.CurrentBattle?.Territory, false).Contains(card))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.CurrentPortableAntidoteUsed = this;
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " equip a ", TreacheryCardType.PortableAntidote);
        }

        #endregion Execution
    }
}
