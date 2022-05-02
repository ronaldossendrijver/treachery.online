/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class AuditCancelled : GameEvent
    {
        public AuditCancelled(Game game) : base(game)
        {
        }

        public AuditCancelled()
        {
        }

        public bool Cancelled { get; set; }

        public override Message Validate()
        {
            if (Cancelled && Player.Resources < Cost(Game)) return Message.Express("You can't pay enough to prevent the audit");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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

        public Message GetDynamicMessage()
        {
            if (Cancelled)
            {
                return Message.Express(Initiator, " pay ", Faction.Brown, new Payment(Cost(Game)), " to cancel an audit of ", GetNumberOfCardsThatMayBeAudited(Game), " cards");
            }
            else
            {
                return Message.Express(Initiator, " don't cancel an audit targeting ", GetNumberOfCardsThatMayBeAudited(Game), " of their cards");
            }
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
            return Math.Min(g.IsAlive(auditor) ? 2 : 1, GetCardsThatMayBeAudited(g).Count());
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
    }
}
