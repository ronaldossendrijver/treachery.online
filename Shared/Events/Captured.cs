/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class Captured : PassableGameEvent
    {
        #region Construction

        public Captured(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public Captured()
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
            Log();

            if (!Passed)
            {
                if (Game.Version > 125 && Game.Prevented(FactionAdvantage.BlackCaptureLeader))
                {
                    Game.LogPreventionByKarma(FactionAdvantage.BlackCaptureLeader);
                }
                else
                {
                    Game.CaptureLeader();
                }
            }

            Game.Enter(Phase.BattleConclusion);
        }

        public override Message GetMessage()
        {
            if (Passed)
            {
                return Message.Express(Initiator, " won't capture or kill a leader");
            }
            else
            {
                return Message.Express(Initiator, " will capture or kill a random leader...");
            }
        }

        #endregion Execution
    }
}
