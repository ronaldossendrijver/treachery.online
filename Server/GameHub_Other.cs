using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public Result<ServerSettings> GetServerSettings()
    {
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
        if (!UsersByUserToken.TryGetValue(userToken, out var user))
            return Error("User not found");

        UserTokensLastSeen[userToken] = DateTime.Now;
        return await Task.FromResult(Success());
    }
}

