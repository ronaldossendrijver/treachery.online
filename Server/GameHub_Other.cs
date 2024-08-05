using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public Result<ServerSettings> Connect()
    {
        //Do some maintenance work here, cleaning up old tokens
        
        var maintenanceDateTime = configuration["GameMaintenanceDateTime"];

        var result = new ServerSettings
        {
            ScheduledMaintenance = maintenanceDateTime != null ? DateTime.Parse(maintenanceDateTime) : DateTime.MinValue,
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
}

