/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Treachery.Shared
{
    public class PerformCyanSetup : GameEvent
    {
        #region Construction

        public PerformCyanSetup(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public PerformCyanSetup()
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

        public static IEnumerable<Location> ValidLocations(Game g) => g.Map.Locations(false).Where(l => l != g.Map.HiddenMobileStronghold && !g.AnyForcesIn(l.Territory));

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Player.ShipForces(Target, 6);
            Log();
            Game.Enter(Game.TreacheryCardsBeforeTraitors, Game.EnterStormPhase, Game.DealStartingTreacheryCards);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " have set up forces");
        }

        #endregion Execution
    }
}
