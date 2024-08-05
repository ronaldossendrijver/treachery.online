using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<VoidResult> SendChatMessage(string userToken, string gameToken, GameChatMessage e)
    {
        if (!AreValid(userToken, gameToken, out _, out _, out var error))
            return error;
        
        await Clients.Group(gameToken).HandleChatMessage(e);
        return Success();
    }

    public async Task<VoidResult> SendGlobalChatMessage(string userToken, GlobalChatMessage message)
    {
        if (!UsersByUserToken.ContainsKey(userToken))
            return Error("User not found");
        
        await Clients.All.HandleGlobalChatMessage(message);
        return Success();
    }
}

