/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformBluePlacement : GameEvent
    {
        #region Construction

        public PerformBluePlacement(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PerformBluePlacement()
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
            if (!ValidLocations(Game).Contains(Target)) return Message.Express("Invalid location");

            return null;
        }

        public static bool BlueMayPlaceFirstForceInAnyTerritory(Game g) => g.Applicable(Rule.BlueFirstForceInAnyTerritory) || g.Version >= 144 && g.Applicable(Rule.BlueAdvisors);

        public static IEnumerable<Location> ValidLocations(Game g)
        {
            {
                if (BlueMayPlaceFirstForceInAnyTerritory(g))
                {
                    return g.Map.Locations(false).Where(l => l != g.Map.HiddenMobileStronghold);
                }
                else
                {
                    return new Location[] { g.Map.PolarSink };
                }
            }
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (Game.Version <= 154 && Game.IsOccupied(Target) || Game.Version > 154 && Game.IsOccupied(Target.Territory))
            {
                Player.ShipAdvisors(Target, 1);
            }
            else
            {
                Player.ShipForces(Target, 1);
            }

            Log();
            Game.Enter(IsPlaying(Faction.Cyan), Phase.CyanSettingUp, Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " position themselves in ", Target);
        }

        #endregion Execution
    }
}
