/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
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

        public override string Validate()
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
                return new Message(Initiator, "{0} adds 1 to force movement.", LeaderSkill.Planetologist);
            }
            else
            {
                return new Message(Initiator, "{0} allows movement from 2 different territories.", LeaderSkill.Planetologist);
            }
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            return (p == g.SkilledPassiveAs(LeaderSkill.Planetologist) && g.CurrentPlanetology == null);
        }
    }
}
