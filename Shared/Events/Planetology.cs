/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Newtonsoft.Json;

namespace Treachery.Shared
{
    public class Planetology : GameEvent
    {
        public Planetology(Game game) : base(game)
        {
        }

        public Planetology()
        {
        }

        public bool AddOneToMovement { get; set; }

        [JsonIgnore]
        public bool MoveFromTwoTerritories => !AddOneToMovement;

        public override Message Validate()
        {
            return "";
        }

        protected override void ExecuteConcreteEvent()
        {
            Game.HandleEvent(this);
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

        public static bool CanBePlayed(Game g, Player p)
        {
            return (g.SkilledAs(p, LeaderSkill.Planetologist) && g.CurrentPlanetology == null);
        }
    }
}
