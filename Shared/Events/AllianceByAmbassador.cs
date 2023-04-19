/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class AllianceByAmbassador : PassableGameEvent
    {
        #region Construction

        public AllianceByAmbassador(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public AllianceByAmbassador()
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
            Game.Enter(Game.PausedAmbassadorPhase);

            if (!Passed)
            {
                Game.MakeAlliance(Initiator, Game.CurrentAmbassadorActivated.Initiator);

                if (Game.CurrentAmbassadorActivated.PinkGiveVidalToAlly)
                {
                    Game.TakeVidal(Player, VidalMoment.EndOfTurn);
                }
                else if (Game.CurrentAmbassadorActivated.PinkTakeVidal)
                {
                    Game.TakeVidal(Game.CurrentAmbassadorActivated.Player, VidalMoment.AfterUsedInBattle);
                }

                if (Game.HasActedOrPassed.Contains(Initiator) && Game.HasActedOrPassed.Contains(Game.CurrentAmbassadorActivated.Initiator))
                {
                    Game.CheckIfForcesShouldBeDestroyedByAllyPresence(Player);
                }

                Game.FlipBeneGesseritWhenAloneOrWithPinkAlly();
            }
            else
            {
                Log(Initiator, " don't ally with ", Game.CurrentAmbassadorActivated.Initiator);

                if (Game.CurrentAmbassadorActivated.PinkTakeVidal)
                {
                    Game.TakeVidal(Game.CurrentAmbassadorActivated.Player, VidalMoment.AfterUsedInBattle);
                }
            }

            Game.DetermineNextShipmentAndMoveSubPhase();
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, !Passed ? "" : " don't", " agree to ally");
        }

        #endregion Execution
    }
}
