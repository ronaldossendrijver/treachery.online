using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public Result<ServerSettings> Connect()
    {
        //Do some maintenance work here, cleaning up old tokens
        
        var result = new ServerSettings
        {
            ScheduledMaintenance = MaintenanceDate,
            AdminName = configuration["GameAdminUsername"]
        };

        return Success(result);
    }
    
    public async Task<VoidResult> RequestRegisterHeartbeat(string userToken)
    {
        if (!UserTokenInfo.TryGetValue(userToken, out var tokenInfo))
            return Error("User not found");

        tokenInfo.Refreshed = DateTime.Now;
        return await Task.FromResult(Success());
    }

    public async Task AdminUpdateMaintenance(string hashedPassword, DateTime maintenanceDate)
    {
        if (await AuthenticateAdmin(hashedPassword))
        {
            MaintenanceDate = maintenanceDate;
        }
    }

    public async Task AdminPersistState(string hashedPassword)
    {
        //TODO
    }

    public async Task AdminRestoreState(string hashedPassword)
    {
        //TODO
    }

    public async Task AdminCloseGame(string hashedPassword, string gameId)
    {
        //TODO
    }
}

