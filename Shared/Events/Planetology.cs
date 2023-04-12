/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class Planetology : GameEvent
    {
        #region Construction

        public Planetology(Game game) : base(game)
        {
        }

        public Planetology()
        {
        }

        #endregion Construction

        #region Properties

        public bool AddOneToMovement { get; set; }

        [JsonIgnore]
        public bool MoveFromTwoTerritories => !AddOneToMovement;

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return (g.SkilledAs(p, LeaderSkill.Planetologist) && g.CurrentPlanetology == null);
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            Game.CurrentPlanetology = this;
        }

        public override Message GetMessage()
        {
            if (AddOneToMovement)
            {
                return Message.Express(LeaderSkill.Planetologist, " adds ", 1, " to force movement");
            }
            else
            {
                return Message.Express(LeaderSkill.Planetologist, " allows movement from ", 2, " different territories");
            }
        }

        #endregion Execution
    }
}
