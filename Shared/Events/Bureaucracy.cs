/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Bureaucracy : PassableGameEvent
    {
        #region Construction

        public Bureaucracy(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Bureaucracy()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Log(GetDynamicMessage());
            if (!Passed)
            {
                Game.Stone(Milestone.Bureaucracy);
                Game.BureaucratWasUsedThisPhase = true;
                Game.GetPlayer(Game.TargetOfBureaucracy).Resources -= 2;
                Game.WasVictimOfBureaucracy = Game.TargetOfBureaucracy;
            }
            Game.Enter(Game.PhaseBeforeBureaucratWasActivated);
            Game.TargetOfBureaucracy = Faction.None;
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't apply Bureaucracy");
            }
            else
            {
                return Message.Express(Initiator, " apply Bureaucracy");
            }
        }

        public Message GetDynamicMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " don't apply Bureaucracy");
            }
            else
            {
                return Message.Express(Initiator, " apply Bureaucracy → ", Game.TargetOfBureaucracy, " lose ", Payment.Of(2));
            }
        }

        #endregion Execution
    }
}
