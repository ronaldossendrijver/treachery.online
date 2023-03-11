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
        public ResourcesAudited(Game game) : base(game)
        {
        }

        public ResourcesAudited()
        {
        }

        public Faction Target { get; set; }

        public override Message Validate()
        {
            if (!ValidFactions(Game, Player).Contains(Target)) return Message.Express("Invalid faction");

            return null;
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " force ", Target, " to reveal amount of weapons, defenses and ", Concept.Resource);
        }

        public static IEnumerable<Faction> ValidFactions(Game game, Player player) =>
            game.Players.Where(opp => opp != player &&
            !game.ResourceAuditedFactions.Contains(opp.Faction) &&
            (player.Homeworlds.Any(hw => opp.AnyForcesIn(hw) > 0) || opp.Homeworlds.Any(hw => player.AnyForcesIn(hw) > 0))
            ).Select(p => p.Faction);

    }
}
