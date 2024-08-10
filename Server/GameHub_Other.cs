using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public Result<ServerInfo> Connect()
    {
        //Do some maintenance work here, cleaning up old tokens
        
        var result = new ServerInfo
        {
            AdminName = configuration["GameAdminUsername"],
            ScheduledMaintenance = MaintenanceDate,
            TotalUsers = GetDbContext().Users.Count(),
            UsersByUserTokenCount = UsersByUserToken.Count,
            ConnectionInfoByUserIdCount = ConnectionInfoByUserId.Count,
            GamesByGameIdCount = GamesByGameId.Count,
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

    public async Task<Result<string>> AdminUpdateMaintenance(string userToken, DateTimeOffset maintenanceDate)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != configuration["GameAdminUsername"])
            return Error<string>("Not authorized");

        MaintenanceDate = maintenanceDate;
        return await Task.FromResult(Success("Maintenance window updated"));
    }

    public async Task<Result<string>> AdminPersistState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != configuration["GameAdminUsername"])
            return Error<string>("Not authorized");
        
        var amount = 0;
        await using (var context = GetDbContext())
        {
            await context.PersistedGames.ExecuteDeleteAsync();
            
            foreach (var gameIdAndManagedGame in GamesByGameId)
            {
                var game = gameIdAndManagedGame.Value;

                var persisted = new PersistedGame
                {
                    CreationDate = game.CreationDate,
                    CreatorUserId = game.CreatorUserId,
                    GameId = game.GameId,
                    GameState = GameState.GetStateAsString(game.Game),
                    GameParticipation = JsonSerializer.Serialize(game.Game.Participation),
                    HashedPassword = game.HashedPassword,
                    BotsArePaused = game.BotsArePaused,
                    ObserversRequirePassword = game.ObserversRequirePassword,
                    StatisticsSent = game.StatisticsSent,
                    GameName = game.GameName,
                };

                context.PersistedGames.Add(persisted);
                await context.SaveChangesAsync();
                amount++;
            }
        }
            
        return Success($"Number of games persisted: {amount}");

    }

    public async Task<Result<string>> AdminRestoreState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != configuration["GameAdminUsername"])
            return Error<string>("Not authorized");
        
        var amount = 0;
        await using (var context = GetDbContext())
        {
            GamesByGameId.Clear();
            
            foreach (var persistedGame in context.PersistedGames)
            {
                var id = persistedGame.GameId;
                var gameState = GameState.Load(persistedGame.GameState);
                var participation = JsonSerializer.Deserialize<GameParticipation>(persistedGame.GameParticipation);
                if (Game.TryLoad(gameState, participation, false, true, out Game game) == null)
                {
                    var managedGame = new ManagedGame
                    {
                        CreatorUserId = persistedGame.CreatorUserId,
                        CreationDate = persistedGame.CreationDate,
                        GameId = persistedGame.GameId,
                        Game = game,
                        HashedPassword = persistedGame.HashedPassword,
                        BotsArePaused = persistedGame.BotsArePaused,
                        ObserversRequirePassword = persistedGame.ObserversRequirePassword,
                        StatisticsSent = persistedGame.StatisticsSent,
                        GameName = persistedGame.GameName,
                    };
                    GamesByGameId.TryAdd(id, managedGame);
                }
                amount++;
            }
        }
            
        return Success($"Number of games restored: {amount}");
    }

    public async Task<Result<string>> AdminCloseGame(string userToken, string gameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != configuration["GameAdminUsername"])
            return Error<string>("Not authorized");

        if (GamesByGameId.TryGetValue(gameId, out var game))
        {
            GamesByGameId.Remove(gameId, out _);

            foreach (var userId in game.Game.Participation.Users.Keys)
            {
                game.Game.RemoveUser(userId, true);
                await Clients.Group(gameId).HandleRemoveUser(userId, true);
                await RemoveFromGroup(gameId, userId);
            }
        }
        
        return Success("Game removed");
    }
}