using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Treachery.Shared;

namespace Treachery.Server;

public partial class GameHub
{
    public async Task<Result<ServerInfo>> Connect()
    {
        await CleanupUserTokens();

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

    private async Task CleanupUserTokens()
    {
        var now = DateTimeOffset.Now;
        if (!(now.Subtract(LastCleanup).TotalMinutes >= 15)) 
            return;

        LastCleanup = now;
        
        foreach (var tokenAndInfo in UserTokenInfo.ToArray())
        {
            var age = now.Subtract(tokenAndInfo.Value.Issued).TotalMinutes;
            if (age >= 10080 ||
                age >= 15 && now.Subtract(tokenAndInfo.Value.Refreshed).TotalMinutes >= 15)
            {
                UsersByUserToken.Remove(tokenAndInfo.Key, out _);
                UserTokenInfo.Remove(tokenAndInfo.Key, out _);
            } 
        }

        foreach (var userIdAndConnectionInfo in ConnectionInfoByUserId)
        {
            foreach (var gameId in userIdAndConnectionInfo.Value.GetGameIdsWithOldConnections(10080).ToArray())
            {
                await RemoveFromGroup(gameId, userIdAndConnectionInfo.Key);
            }
        }
    }

    public async Task<VoidResult> RequestRegisterHeartbeat(string userToken)
    {
        if (!UserTokenInfo.TryGetValue(userToken, out var tokenInfo))
            return Error(ErrorType.UserNotFound);

        tokenInfo.Refreshed = DateTime.Now;
        return await Task.FromResult(Success());
    }

    public async Task<Result<string>> AdminUpdateMaintenance(string userToken, DateTimeOffset maintenanceDate)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

        MaintenanceDate = maintenanceDate;
        return await Task.FromResult(Success("Maintenance window updated"));
    }

    public async Task<Result<string>> AdminPersistState(string userToken)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);
        
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
                    LastAsyncPlayMessageSent = game.LastAsyncPlayMessageSent,
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
            return Error<string>(ErrorType.InvalidUserNameOrPassword);
        
        var amount = 0;
        await using (var context = GetDbContext())
        {
            GamesByGameId.Clear();
            
            foreach (var persistedGame in context.PersistedGames)
            {
                var id = persistedGame.GameId;
                var gameState = GameState.Load(persistedGame.GameState);
                var participation = JsonSerializer.Deserialize<GameParticipation>(persistedGame.GameParticipation);
                var loadMessage = Game.TryLoad(gameState, participation, false, true, out var game);
                if (loadMessage == null)
                {
                    var managedGame = new ManagedGame
                    {
                        CreationDate = persistedGame.CreationDate,
                        CreatorUserId = persistedGame.CreatorUserId,
                        GameId = persistedGame.GameId,
                        Game = game,
                        HashedPassword = persistedGame.HashedPassword,
                        BotsArePaused = persistedGame.BotsArePaused,
                        ObserversRequirePassword = persistedGame.ObserversRequirePassword,
                        StatisticsSent = persistedGame.StatisticsSent,
                        LastAsyncPlayMessageSent = persistedGame.LastAsyncPlayMessageSent,
                    };
                    
                    GamesByGameId.TryAdd(id, managedGame);
                    amount++;
                }
            }
        }
            
        return Success($"Number of games restored: {amount}");
    }

    public async Task<Result<string>> AdminCloseGame(string userToken, string gameId)
    {
        if (!UsersByUserToken.TryGetValue(userToken, out var user) || user.Name != configuration["GameAdminUsername"])
            return Error<string>(ErrorType.InvalidUserNameOrPassword);

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