/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class FactionSelected : GameEvent
    {
        #region Construction

        public FactionSelected(Game game, string playername) : base(game, playername)
        {
        }

        public FactionSelected()
        {
        }

        #endregion Construction

        #region Properties

        public string InitiatorPlayerName { get; set; }

        public Faction Faction { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Faction != Faction.None && !ValidFactions(Game).Contains(Faction)) return Message.Express("Faction not available");

            return null;
        }

        public static IEnumerable<Faction> ValidFactions(Game g)
        {
            return g.FactionsInPlay.Where(f => !g.Players.Any(p => p.Faction == f));
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var initiator = Game.Players.FirstOrDefault(p => p.Name == InitiatorPlayerName);
            if (initiator != null && Game.FactionsInPlay.Contains(Faction))
            {
                initiator.Faction = Faction;
                Game.FactionsInPlay.Remove(Faction);
                Log();
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(InitiatorPlayerName, " plays ", Faction);
        }

        #endregion Execution
    }
}
