/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    using System.Collections.Generic;

    public class Prescience : GameEvent
    {
        public PrescienceAspect Aspect { get; set; }

        public Prescience(Game game) : base(game)
        {
        }

        public Prescience()
        {
        }

        public override Message Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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

        public override Message GetMessage()
        {
            return Message.Express("By Prescience, ", Faction.Green, " see the enemy ", Aspect);
        }
    }

    public enum PrescienceAspect
    {
        None = 0,
        Dial = 10,
        Leader = 20,
        Weapon = 30,
        Defense = 40
    }
}
