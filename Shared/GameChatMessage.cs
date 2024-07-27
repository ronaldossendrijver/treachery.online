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

    public override Message GetBodyIncludingPlayerInfo(int receivingUserId, Game game, bool contextIsGlobal)
    {
        if (SourceUserId == receivingUserId)
        {
            return TargetUserId < 0 ? 
                Message.Express("You: ", Body, " ⇒ ALL") : 
                Message.Express("You: ", Body, " ⇒ ", GetTargetFaction(game));
        }

        var sourceFaction = GetSourceFaction(game);

        if (TargetUserId < 0)
        {
            return sourceFaction != Faction.None ? 
                Message.Express(GetSourceFaction(game), " (to ALL) ", Body) : 
                Message.Express(SourceUserId, " (to ALL) ", Body);
        }

        return Message.Express(
            sourceFaction != Faction.None ? GetSourceFaction(game) : game.GetPlayerName(SourceUserId), 
            Body);
    }

    public Faction GetSourceFaction(Game game)
    {
        var player = game.GetPlayerByUserId(SourceUserId);
        return player?.Faction ?? Faction.None;
    }

    public Faction GetTargetFaction(Game game)
    {
        var player = game.GetPlayerByUserId(TargetUserId);
        return player?.Faction ?? Faction.None;
    }
}