/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class ResourcesAudited : GameEvent
    {
        #region Construction

        public ResourcesAudited(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public ResourcesAudited()
        {
        }

        #endregion Construction

        #region Properties

        public Faction Target { get; set; }

        #endregion Properties

        #region Validation
        public override Message Validate()
        {
            if (!ValidFactions(Game, Player).Contains(Target)) return Message.Express("Invalid faction");

            return null;
        }

        public static IEnumerable<Faction> ValidFactions(Game game, Player player) =>
            game.Players.Where(opp => opp != player &&
            !game.ResourceAuditedFactions.Contains(opp.Faction) &&
            (player.Homeworlds.Any(hw => opp.AnyForcesIn(hw) > 0) || opp.Homeworlds.Any(hw => player.AnyForcesIn(hw) > 0))
            ).Select(p => p.Faction);


        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.ResourceAuditedFactions.Add(Target);
            var target = GetPlayer(Target);

            Log();
            LogTo(Initiator, Target, " own ", Payment.Of(target.Resources), ", ", target.TreacheryCards.Count(tc => tc.IsWeapon), " weapons and ", target.TreacheryCards.Count(tc => tc.IsDefense), " defenses");
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " force ", Target, " to reveal amount of weapons, defenses and ", Concept.Resource);
        }

        #endregion Execution
    }
}
