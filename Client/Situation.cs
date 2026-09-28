namespace Treachery.Client;

public class Situation
{
    private Skin? _skin;
    private Game? _game;
    private int _eventCount;

    public bool RequiresUpdate(IGameService service)
    {
        if (service.Game == null) return false;
        
        var latestEvent = service.Game.LatestEvent();

        var result = _skin == null || 
                     _skin != service.CurrentSkin || 
                     _game == null || 
                     _game != service.Game;

        if (!result)
        {
            result = _eventCount != service.Game.EventCount;

            if (result) result = latestEvent is not AllyPermission;
        }

        _skin = service.CurrentSkin;
        _game = service.Game;
        _eventCount = service.Game.EventCount;

        return result;
    }
}