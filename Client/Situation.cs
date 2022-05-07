using Treachery.Shared;

namespace Treachery.Client
{
    public class Situation
    {
        private Skin _skin;
        private Game _game;
        public int _eventCount;

        public bool RequiresUpdate(Game game)
        {
            var latestEvent = game.LatestEvent();

            bool result = (_skin == null || Skin.Current == null || _skin != Skin.Current || _game == null || game == null || _game != game);

            if (!result)
            {
                result = (_eventCount != game.EventCount);

                if (result)
                {
                    result = !(latestEvent is AllyPermission);
                }
            }

            _skin = Skin.Current;
            _game = game;
            _eventCount = game.EventCount;

            return result;
        }
    }
}
