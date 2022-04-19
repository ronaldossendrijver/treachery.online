using System.Linq;
using Treachery.Shared;

namespace Treachery.Client
{
    public class Situation
    {
        public Game Game;
        public int EventCount;

        public bool RequiresUpdate(Game game) {

            var latestEvent = game.LatestEvent();

            bool result = (Game == null || game == null || Game != game);

            if (!result)
            {
                result = (EventCount != game.EventCount);

                if (result)
                {
                    result = !(latestEvent is AllyPermission || (latestEvent is IBid && Game.BiddingIsInProgress));
                    //Support.Log("Event requires update: " + result + ", milestones: " + Skin.Current.Join(game.RecentMilestones));
                }
            }

            Game = game;
            EventCount = game.EventCount;

            return result;
        }
    }
}
