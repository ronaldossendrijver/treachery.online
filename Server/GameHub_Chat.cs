using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> SendChatMessage(string playerToken, string gameToken, GameChatMessage e)
    {
        if (!AreValid(playerToken, gameToken, out var playerId, out var game, out var error))
            return error;
        
        await Clients.Group(gameToken).HandleChatMessage(e);
        return Success();
    }

    public async Task<VoidResult> SendGlobalChatMessage(string playerToken, GlobalChatMessage message)
    {
        if (!usersByUserToken.TryGetValue(playerToken, out _))
            return Error("Player not found");
        
        await Clients.All.HandleGlobalChatMessage(message);
        return Success();
    }
        
}

