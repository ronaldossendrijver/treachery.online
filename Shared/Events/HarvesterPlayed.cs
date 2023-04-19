/*
 * Copyright 2020-2023 Ronald Ossendrijver. All rights reserved.
 */

namespace Treachery.Shared
{
    public class HarvesterPlayed : GameEvent
    {
        #region Construction

        public HarvesterPlayed(Game game, Faction initiator) : base(game, initiator)
        {
        }

        public HarvesterPlayed()
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
            Game.Discard(Player, TreacheryCardType.Harvester);
            var lastResourceCard = Game.CurrentPhase == Phase.HarvesterA ? Game.LatestSpiceCardA : Game.LatestSpiceCardB;
            if (Game.ResourcesOnPlanet.TryGetValue(lastResourceCard.Location, out int currentAmountOfSpice))
            {
                Game.ChangeResourcesOnPlanet(lastResourceCard.Location, currentAmountOfSpice);
            }

            Log();
            Game.MoveToNextPhaseAfterResourceBlow();
            Game.Stone(Milestone.Harvester);
        }

        public override Message GetMessage()
        {
            return Message.Express(Initiator, " use a ", TreacheryCardType.Harvester);
        }

        #endregion Execution
    }
}
