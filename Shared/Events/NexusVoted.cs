/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

using System.Linq;

namespace Treachery.Shared
{
    public class NexusVoted : PassableGameEvent
    {
        #region Construction

        public NexusVoted(Game game) : base(game)
        {
        }

        public NexusVoted()
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
            Game.NexusVotes.Add(this);
            if (Game.NexusVotes.Count == Game.Players.Count)
            {
                if (2 * Game.NexusVotes.Count(v => v.Passed) >= Game.Players.Count)
                {
                    Game.Enter(Game.CurrentPhase == Phase.VoteAllianceA && Game.Applicable(Rule.IncreasedResourceFlow), Game.EnterBlowB, Game.StartNexusCardPhase);
                }
                else
                {
                    Game.Enter(Game.CurrentPhase == Phase.VoteAllianceA, Phase.AllianceA, Phase.AllianceB);
                }
            }
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " vote ", Passed ? "No" : "Yes");
        }

        #endregion Execution
    }
}
