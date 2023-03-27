/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class BlueBattleAnnouncement : GameEvent
    {
        #region Construction

        public BlueBattleAnnouncement(Game game) : base(game)
        {
        }

        public BlueBattleAnnouncement()
        {
        }

        #endregion Construction

        #region Properties

        public int _territoryId;

        [JsonIgnore]
        public Territory Territory
        {
            get => Game.Map.TerritoryLookup.Find(_territoryId);
            set => _territoryId = Game.Map.TerritoryLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (Initiator != Faction.Blue) return Message.Express("Your faction can't announce battle");
            var p = Player;
            if (p.SpecialForcesIn(Territory) == 0) return Message.Express("You don't have advisors there");
            var ally = Game.GetPlayer(p.Ally);
            if (ally != null && ally.AnyForcesIn(Territory) > 0) return Message.Express("Can't announce battle due to ally presence");

            return null;
        }

        public static IEnumerable<Territory> ValidTerritories(Game g, Player p)
        {
            return g.Map.Territories(g.Applicable(Rule.Homeworlds)).Where(t =>
                (!p.HasAlly || p.AlliedPlayer.AnyForcesIn(t) == 0) &&
                p.SpecialForcesIn(t) > 0 &&
                (!t.IsStronghold || g.NrOfOccupantsExcludingPlayer(t, p) <= 1) &&
                !t.Locations.Any(l => l.Sector == g.SectorInStorm && p.SpecialForcesIn(l) > 0));
        }

        #endregion Properties

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            var initiator = GetPlayer(Initiator);
            initiator.FlipForces(Territory, false);
            Log();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " flip to ", FactionForce.Blue, " in ", Territory);
        }

        #endregion
    }
}
