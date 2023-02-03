/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class TreacheryCalled : GameEvent
    {
        public TreacheryCalled(Game game) : base(game)
        {
        }

        public TreacheryCalled()
        {
        }

        public bool TraitorCalled { get; set; }

        public override Message Validate()
        {
            if (!TraitorCalled) return null;

            if (!MayCallTreachery(Game, Player)) return Message.Express("You cannot call TREACHERY");

            return null;
        }

        public bool TreacherySucceeded(Game g) => TraitorCalled && (Initiator != Faction.Black || !g.BlackTraitorWasCancelled);

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (TraitorCalled)
            {
                return Message.Express(Initiator, " call TREACHERY!");
            }
            else
            {
                return Message.Express(Initiator, " don't call treachery");
            }
        }

        public static bool MayCallTreachery(Game g, Player p)
        {
            if (g.AggressorBattleAction != null && g.DefenderBattleAction != null)
            {
                if (!g.DefenderBattleAction.Messiah)
                {
                    if (g.AggressorBattleAction.Initiator == p.Faction && HasTraitor(g, p, g.DefenderBattleAction.Hero) ||
                         p.Is(Faction.Black) && p.Ally == g.AggressorBattleAction.Initiator && !g.Prevented(FactionAdvantage.BlackCallTraitorForAlly) && HasTraitor(g, p, g.DefenderBattleAction.Hero))

                    {
                        return true;
                    }
                }

                if (!g.AggressorBattleAction.Messiah)
                {
                    if (g.DefenderBattleAction.Initiator == p.Faction && HasTraitor(g, p, g.AggressorBattleAction.Hero) ||
                         p.Is(Faction.Black) && p.Ally == g.DefenderBattleAction.Initiator && !g.Prevented(FactionAdvantage.BlackCallTraitorForAlly) && HasTraitor(g, p, g.AggressorBattleAction.Hero))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasTraitor(Game g, Player p, IHero h)
        {
            return p.Traitors.Any(t => t.IsTraitor(h)) || h != null && g.Applicable(Rule.CapturedLeadersAreTraitorsToOwnFaction) && h.Faction == p.Faction;
        }
    }
}
