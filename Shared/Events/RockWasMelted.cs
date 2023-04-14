/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class RockWasMelted : GameEvent
    {
        #region Construction

        public RockWasMelted(Game game) : base(game)
        {
        }

        public RockWasMelted()
        {
        }

        #endregion Construction

        #region Properties

        public bool Kill { get; set; }

        #endregion Properties

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        public static bool CanBePlayed(Game g, Player p)
        {
            var plan = g.CurrentBattle.PlanOf(p);
            return plan != null && plan.Weapon != null && plan.Weapon.IsRockmelter;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log();
            if (Game.Version < 146) Game.Discard(Player, TreacheryCardType.Rockmelter);
            Game.CurrentRockWasMelted = this;
            Game.Enter(Phase.CallTraitorOrPass);
        }

        public override Message GetMessage()
        {
            if (Kill)
            {
                return Message.Express(Initiator, " use their ", TreacheryCardType.Rockmelter, " to kill both leaders");
            }
            else
            {
                return Message.Express(Initiator, " use their ", TreacheryCardType.Rockmelter, " to reduce both leaders to 0 strength");
            }
        }

        #endregion Execution
    }
}
