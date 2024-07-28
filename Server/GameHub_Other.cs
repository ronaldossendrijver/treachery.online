﻿using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    /*public void ProcessHeartbeat(string playerToken)
    {
        if (usersByPlayerToken.ContainsKey(playerToken))
        {
            playerTokensLastSeen[playerToken] = DateTime.Now;
        }
    }*/

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
}
