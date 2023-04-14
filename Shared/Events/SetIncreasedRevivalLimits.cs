/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class SetIncreasedRevivalLimits : GameEvent
    {
        #region Construction

        public SetIncreasedRevivalLimits(Game game) : base(game)
        {
        }

        public SetIncreasedRevivalLimits()
        {
        }

        #endregion Construction

        #region Properties

        public Faction[] Factions { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Factions.Any(f => !ValidTargets(Game, Player).Contains(f))) return Message.Express("Invalid faction");

            return null;
        }

        public static IEnumerable<Faction> ValidTargets(Game g, Player p)
        {
            return g.Players.Where(x => x.Faction != p.Faction).Select(x => x.Faction);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.FactionsWithIncreasedRevivalLimits = Factions;
            Log();
        }

        public override Message GetMessage()
        {
            if (Factions.Any())
            {
                return Message.Express(Initiator, " grant a revival limit of ", 5, " to ", Factions);
            }
            else
            {
                return Message.Express(Initiator, " don't grant increased revival limits");
            }
        }

        #endregion Execution
    }
}
