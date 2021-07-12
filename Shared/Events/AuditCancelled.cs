/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Linq;
using System.Collections.Generic;

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

        public override string Validate()
        {
            if (Cancelled && Player.Resources < Cost(Game)) return "You can't pay enough to cancel the audit";

            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            if (Cancelled)
            {
                return new Message(Initiator, "{0} paid a bribe to cancel an audit.", Initiator);
            }
            else
            {
                return new Message(Initiator, "{0} don't pay a bribe to cancel an audit", Initiator);
            }
        }

        public Message GetDynamicMessage()
        {
            if (Cancelled)
            {
                return new Message(Initiator, "{0} bribe {1} for {2} to cancel the audit.", Initiator, Faction.Brown, Cost(Game));
            }
            else
            {
                return new Message(Initiator, "{0} don't cancel an audit targeting {1} of their cards.", Initiator, GetNumberOfCardsThatMayBeAudited(Game));
            }
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
            return auditee.TreacheryCards.Where(c => c != recentBattlePlan.Weapon && c != recentBattlePlan.Defense && c != recentBattlePlan.Hero);
        }
    }
}
