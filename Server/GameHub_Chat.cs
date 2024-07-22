using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> RequestChatMessage(string playerToken, string gameToken, GameChatMessage e)
    {
        if (!AreValid(playerToken, gameToken, out var playerId, out var game, out var error))
            return error;
        
        await Clients.Group(gameToken).HandleChatMessage(e);
        return Success();
    }

    public async Task SendGlobalChatMessage(GlobalChatMessage message) 
        => await Clients.All.HandleGlobalChatMessage(message);
}

