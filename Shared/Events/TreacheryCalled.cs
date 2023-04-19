/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Linq;

namespace Treachery.Shared
{
    public class TreacheryCalled : GameEvent
    {
        #region Construction

        public TreacheryCalled(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public TreacheryCalled()
        {
        }

        #endregion Construction

        #region Properties

        public bool TraitorCalled { get; set; }

        [JsonIgnore]
        public bool Succeeded => TraitorCalled && (Initiator != Faction.Black || !Game.BlackTraitorWasCancelled);

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!TraitorCalled) return null;

            if (!MayCallTreachery(Game, Player)) return Message.Express("You cannot call TREACHERY");

            return null;
        }

        public static bool MayCallTreachery(Game g, Player p)
        {
            if (g.AggressorBattleAction != null && g.DefenderBattleAction != null)
            {
                if (!g.CurrentBattle.Territory.IsHomeworld || p.IsNative(g.CurrentBattle.Territory))
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
            }

            return false;
        }

        private static bool HasTraitor(Game g, Player p, IHero h)
        {
            return p.Traitors.Any(t => t.IsTraitor(h)) || h != null && g.Applicable(Rule.CapturedLeadersAreTraitorsToOwnFaction) && h.Faction == p.Faction;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (Game.AggressorBattleAction.By(Initiator) || By(Faction.Black) && Game.AreAllies(Game.AggressorBattleAction.Initiator, Faction.Black))
            {
                Game.AggressorTraitorAction = this;
                if (TraitorCalled)
                {
                    Log();
                    Game.Stone(Milestone.TreacheryCalled);
                    Player.RevealedTraitors.Add(Game.DefenderBattleAction.Hero);
                }
            }

            if (Game.DefenderBattleAction.By(Initiator) || By(Faction.Black) && Game.AreAllies(Game.DefenderBattleAction.Initiator, Faction.Black))
            {
                Game.DefenderTraitorAction = this;
                if (TraitorCalled)
                {
                    Log();
                    Game.Stone(Milestone.TreacheryCalled);
                    Player.RevealedTraitors.Add(Game.AggressorBattleAction.Hero);
                }
            }

            if (Game.AggressorTraitorAction != null && Game.DefenderTraitorAction != null)
            {
                var treachery = Game.CurrentBattle.TreacheryOf(Faction.Black);

                if (Game.Applicable(Rule.NexusCards) && treachery != null && treachery.Initiator == Faction.Black && treachery.TraitorCalled)
                {
                    Game.Enter(Phase.CancellingTraitor);
                }
                else
                {
                    Game.HandleRevealedBattlePlans();
                }
            }
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

        #endregion Execution
    }
}
