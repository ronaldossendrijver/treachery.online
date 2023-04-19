/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class FaceDancerRevealed : PassableGameEvent
    {
        #region Construction

        public FaceDancerRevealed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public FaceDancerRevealed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            if (Passed) return null;

            if (!FaceDanced.MayCallFaceDancer(Game, Player)) return Message.Express("You can't reveal a face dancer");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            if (!Passed)
            {
                var facedancer = Player.FaceDancers.FirstOrDefault(f => Game.WinnerHero.IsFaceDancer(f));
                Log(Initiator, " reveal ", facedancer, " as one of their Face Dancers!");

                Game.Stone(Milestone.FaceDanced);
                Game.Enter(Phase.Facedancing);
            }
            else
            {
                Log(Initiator, " don't reveal a Face Dancer");
                Game.FinishBattle();
            }
        }

        public override Message GetMessage()
        {
            if (!Passed)
            {
                return Message.Express(Initiator, " reveal a face dancer!");
            }
            else
            {
                return Message.Express(Initiator, " don't reveal a face dancer");
            }
        }

        #endregion Execution
    }
}
