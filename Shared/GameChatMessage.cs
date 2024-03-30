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
    public string TargetPlayerName { get; set; }

    public override Message GetBodyIncludingPlayerInfo(string receivingPlayerName, Game g, bool contextIsGlobal)
    {
        if (SourcePlayerName == receivingPlayerName)
        {
            if (TargetPlayerName == "")
                return Message.Express("You: ", Body, " ⇒ ALL");
            return Message.Express("You: ", Body, " ⇒ ", GetTargetFaction(g));
        }

        var sourceFaction = GetSourceFaction(g);

        if (TargetPlayerName == "")
        {
            if (sourceFaction != Faction.None)
                return Message.Express(GetSourceFaction(g), " (to ALL) ", Body);
            return Message.Express(SourcePlayerName, " (to ALL) ", Body);
        }

        if (sourceFaction != Faction.None)
            return Message.Express(GetSourceFaction(g), Body);
        return Message.Express(SourcePlayerName, Body);
    }

    public Faction GetSourceFaction(Game g)
    {
        var p = g.GetPlayer(SourcePlayerName);
        return p == null ? Faction.None : p.Faction;
    }

    public Faction GetTargetFaction(Game g)
    {
        var p = g.GetPlayer(TargetPlayerName);
        return p == null ? Faction.None : p.Faction;
    }
}