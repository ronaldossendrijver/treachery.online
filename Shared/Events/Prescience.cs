/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    using System.Collections.Generic;

    public class Prescience : GameEvent
    {
        #region Construction

        public Prescience(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Prescience()
        {
        }

        #endregion Construction

        #region Properties

        public PrescienceAspect Aspect { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static IEnumerable<PrescienceAspect> ValidAspects(Game g, Player p)
        {
            var opponent = g.CurrentBattle.OpponentOf(p);
            if (opponent.HasNoFieldIn(g.CurrentBattle.Territory))
            {
                return new PrescienceAspect[] { PrescienceAspect.Leader, PrescienceAspect.Weapon, PrescienceAspect.Defense };
            }
            else
            {
                return new PrescienceAspect[] { PrescienceAspect.Dial, PrescienceAspect.Leader, PrescienceAspect.Weapon, PrescienceAspect.Defense };
            }
        }

        public static bool MayUsePrescience(Game g, Player p)
        {
            if (g.CurrentBattle != null && g.CurrentPrescience == null && !g.Prevented(FactionAdvantage.GreenBattlePlanPrescience))
            {
                if (p.Faction == Faction.Green)
                {
                    return g.CurrentBattle.IsInvolved(p);
                }
                else if (p.Ally == Faction.Green && g.GreenSharesPrescience)
                {
                    return g.CurrentBattle.IsAggressorOrDefender(p);
                }
            }

            return false;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.CurrentPrescience = this;
            Log();
            Game.Stone(Milestone.Prescience);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use Prescience to see the enemy ", Aspect);
        }

        #endregion Execution
    }
}
