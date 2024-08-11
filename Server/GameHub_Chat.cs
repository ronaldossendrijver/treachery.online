using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> SendChatMessage(string userToken, string gameId, GameChatMessage e)
    {
        if (!AreValid(userToken, gameId, out _, out _, out var error))
            return error;
        
        await Clients.Group(gameId).HandleChatMessage(e);
        return Success();
    }

    public async Task<VoidResult> SendGlobalChatMessage(string userToken, GlobalChatMessage message)
    {
        if (!UsersByUserToken.ContainsKey(userToken))
            return Error(ErrorType.UserNotFound);
        
        await Clients.All.HandleGlobalChatMessage(message);
        return Success();
    }
}

