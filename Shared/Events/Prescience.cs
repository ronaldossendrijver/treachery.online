/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public static IEnumerable<PrescienceAspect> ValidAspects
        {
            get
            {
                return Enumerations.GetValuesExceptDefault(typeof(PrescienceAspect), PrescienceAspect.None);
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
                    if (g.Version < 78)
                    {
                        return g.CurrentBattle.IsInvolved(p);
                    }
                    else
                    {
                        return g.CurrentBattle.IsAggressorOrDefender(p);
                    }
                }
            }

            return false;
        }

        public override Message GetMessage()
        {
            return new Message(Faction.Green, "By Prescience, {0} see the enemy {1}.", Faction.Green, Aspect);
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
