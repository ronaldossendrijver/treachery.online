/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformHmsPlacement : GameEvent
    {
        #region Construction

        public PerformHmsPlacement(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PerformHmsPlacement()
        {
        }

        #endregion Construction

        #region Properties

        public int _targetId;

        [JsonIgnore]
        public Location Target
        {
            get => Game.Map.LocationLookup.Find(_targetId);
            set => _targetId = Game.Map.LocationLookup.GetId(value);
        }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            if (!ValidLocations(Game, Player).Contains(Target)) return Message.Express("Invalid location");

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g, Player p)
        {
            if (p.Faction != Faction.Grey)
            {
                return g.Map.Locations(false).Where(l => !l.Territory.IsStronghold);
            }
            else
            {
                return g.Map.Locations(false).Where(l => !l.Territory.IsStronghold && l.Sector != g.SectorInStorm);
            }
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.Map.HiddenMobileStronghold.PointAt(Target);
            Log();
            Game.EndStormPhase();
            Game.Stone(Milestone.HmsMovement);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " position the ", Game.Map.HiddenMobileStronghold, " above ", Target);
        }

        #endregion Execution
    }
}
