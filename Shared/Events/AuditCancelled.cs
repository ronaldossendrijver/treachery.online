/*
 * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace Treachery.Shared;

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
            return auditee.TreacheryCards.Where(c => c != recentBattlePlan.Weapon && c != recentBattlePlan.Defense && c != recentBattlePlan.Hero);
        return Array.Empty<TreacheryCard>();
    }

    #endregion Validation

    #region Execution

    protected override void ExecuteConcreteEvent()
    {
        if (Cancelled)
            Log(Initiator, " pay ", Faction.Brown, Payment.Of(Cost(Game)), " to cancel an audit of ", GetNumberOfCardsThatMayBeAudited(Game), " cards");
        else
            Log(Initiator, " don't cancel an audit targeting ", GetNumberOfCardsThatMayBeAudited(Game), " of their cards");

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
            return Message.Express(Initiator, " pay to cancel an audit");
        return Message.Express(Initiator, " don't pay to cancel an audit");
    }

    #endregion Execution
}