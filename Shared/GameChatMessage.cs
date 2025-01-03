/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Treachery.Shared;

public class GameChatMessage : ChatMessage
{
    public int TargetUserId { get; init; }

    public override Message GetBodyIncludingPlayerInfo(int receivingUserId, Dictionary<int,LoggedInUserInfo> users, Game game, bool contextIsGlobal)
    {
        var sourcePlayerName = GetPlayerName(SourceUserId, users);
        
        if (SourceUserId == receivingUserId)
        {
            var targetFaction = GetFaction(TargetUserId, game);
            
            return TargetUserId < 0 ? 
                Message.Express("You: ", Body, " ⇒ ALL") : 
                Message.Express("You: ", Body, " ⇒ ", targetFaction != Faction.None ? targetFaction : GetPlayerName(TargetUserId, users));
        }

        var sourceFaction = GetFaction(SourceUserId, game);

        if (TargetUserId < 0)
        {
            return sourceFaction != Faction.None ? 
                Message.Express(sourceFaction, " (to ALL) ", Body) : 
                Message.Express(sourcePlayerName, " (to ALL) ", Body);
        }

        return Message.Express(sourceFaction != Faction.None ? sourceFaction : sourcePlayerName, " ", Body);
    }

    private string GetPlayerName(int userId, Dictionary<int, LoggedInUserInfo> users)
        => users.TryGetValue(userId, out var sourcePlayerInfo) ? sourcePlayerInfo.PlayerName : "Offline player";

    public Faction GetFaction(int userId, Game game) => game?.GetPlayerByUserId(userId)?.Faction ?? Faction.None;
}