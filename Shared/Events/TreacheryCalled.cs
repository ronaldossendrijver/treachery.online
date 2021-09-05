/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
        {
            if (!TraitorCalled) return "";

            var p = Player;
            if (!MayCallTreachery(Game, p)) return "You cannot call TREACHERY";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (TraitorCalled)
            {
                return new Message(Initiator, "{0} call TREACHERY!", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} don't call treachery.", Initiator);
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
