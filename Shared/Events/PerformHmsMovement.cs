/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformHmsMovement : PassableGameEvent
    {
        #region Construction

        public PerformHmsMovement(Game game) : base(game)
        {
        }

        public PerformHmsMovement()
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
            if (!Passed)
            {
                if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");
            }

            return null;
        }

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            return Map.FindNeighboursForHmsMovement(g.Map.HiddenMobileStronghold.AttachedToLocation, 1, false, g.SectorInStorm).Where(l => !l.IsStronghold);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            int collectionRate = Player.AnyForcesIn(Game.Map.HiddenMobileStronghold) * 2;
            Log();

            var currentLocation = Game.Map.HiddenMobileStronghold.AttachedToLocation;
            Game.CollectSpiceFrom(Initiator, currentLocation, collectionRate);

            if (!Passed)
            {
                Game.Map.HiddenMobileStronghold.PointAt(Target);
                Game.CollectSpiceFrom(Initiator, Target, collectionRate);
                Game.HmsMovesLeft--;
                Game.Stone(Milestone.HmsMovement);
            }

            if (Passed || Game.HmsMovesLeft == 0)
            {
                Game.DetermineStorm();
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " move the ", Game.Map.HiddenMobileStronghold, " to ", Target);
            }
            else
            {
                return Message.Express(Initiator, " pass (further) movement of the ", Game.Map.HiddenMobileStronghold);
            }
        }

        #endregion Execution
    }
}
