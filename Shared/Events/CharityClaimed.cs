/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class CharityClaimed : GameEvent
    {
        #region Construction

        public CharityClaimed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public CharityClaimed()
        {
        }

        #endregion Construction

        #region Validation

        public override Message Validate()
        {
            var p = Player;
            if (p.Resources > 1) return Message.Express("You are not eligable for charity");

            return null;
        }

        #endregion Validation

        #region Execution

        protected override void ExecuteConcreteEvent()
        {
            Game.HasActedOrPassed.Add(Initiator);

            Game.GiveCharity(Player, (2 - Player.Resources) * Game.CurrentCharityMultiplier);

            if (!By(Faction.Blue))
            {
                Game.ResourceTechTokenIncome = true;
            }

            Game.Stone(Milestone.CharityClaimed);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " claim charity");
        }

        #endregion Execution
    }
}
