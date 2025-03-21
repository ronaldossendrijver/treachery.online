// /*
//  * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

using System.ComponentModel.DataAnnotations;

namespace Treachery.Server;

public class PersistedScheduledGame
{
    public int Id { get; init; }

    [MaxLength(36)]
    public string GameId { get; init; } = string.Empty;
    
    public int CreatorUserId { get; init; }
    
    [MaxLength(128)]
    public string CreatorPlayerName { get; init; } = string.Empty;
    
    public DateTimeOffset DateTime { get; init; }
    
    public int? NumberOfPlayers { get; init; }
    
    public int? MaximumTurns { get; init; }

    public Ruleset? Ruleset { get; init; }
    
    public bool AsyncPlay { get; init; }

    public List<Faction> AllowedFactionsInPlay { get; init; } = [];

    public string SubscribedUsers { get; init; } = string.Empty;
}