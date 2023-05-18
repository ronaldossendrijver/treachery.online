/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class AuditCancelled : GameEvent
    {
        #region Construction

        public AuditCancelled(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public AuditCancelled()
        {
        }

        #endregion Construction

        #region Properties

        public bool Cancelled { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Cancelled && Player.Resources < Cost(Game)) return Message.Express("You can't pay enough to prevent the audit");

            return null;
        }

        public int Cost()
        {
            return Cost(Game);
        }

        public static int Cost(Game g)
        {
            return GetNumberOfCardsThatMayBeAudited(g);
        }

        public static int GetNumberOfCardsThatMayBeAudited(Game g)
        {
            var auditor = LeaderManager.GetLeaders(Faction.Brown).First(l => l.HeroType == HeroType.Auditor);
            return Math.Min(g.AuditorSurvivedBattle ? 2 : 1, GetCardsThatMayBeAudited(g).Count());
        }

        public static IEnumerable<TreacheryCard> GetCardsThatMayBeAudited(Game g)
        {
            var auditee = g.Auditee;
            var recentBattlePlan = g.CurrentBattle.PlanOf(auditee);
            if (recentBattlePlan != null)
            {
                return auditee.TreacheryCards.Where(c => c != recentBattlePlan.Weapon && c != recentBattlePlan.Defense && c != recentBattlePlan.Hero);
            }
            else
            {
                return Array.Empty<TreacheryCard>();
            }
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (Cancelled)
            {
                Log(Initiator, " pay ", Faction.Brown, Payment.Of(Cost(Game)), " to cancel an audit of ", GetNumberOfCardsThatMayBeAudited(Game), " cards");
            }
            else
            {
                Log(Initiator, " don't cancel an audit targeting ", GetNumberOfCardsThatMayBeAudited(Game), " of their cards");
            }

            if (Cancelled)
            {
                Player.Resources -= Cost();
                GetPlayer(Faction.Brown).Resources += Cost();
            }

            if (!Cancelled)
            {
                Game.Enter(Phase.Auditing);
                LogTo(Initiator, Faction.Brown, " see: ", Game.AuditedCards);
            }
            else
            {
                Game.Enter(Game.BattleWinner == Faction.None, Game.FinishBattle, Game.BlackMustDecideToCapture, Phase.CaptureDecision, Phase.BattleConclusion);
            }
        }

        public override Message GetMessage()
        {
            if (Cancelled)
            {
                return Message.Express(Initiator, " pay to cancel an audit");
            }
            else
            {
                return Message.Express(Initiator, " don't pay to cancel an audit");
            }
        }

        #endregion Execution
    }
}
