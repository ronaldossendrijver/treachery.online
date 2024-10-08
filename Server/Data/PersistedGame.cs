// /*
//  * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

using System.ComponentModel.DataAnnotations;

namespace Treachery.Server;

public class PersistedGame
{
    public int Id { get; init; }
    
    [MaxLength(128)]
    public string GameName { get; set; }
    
    [MaxLength(36)]
    public string GameId { get; init; }
    
    public DateTimeOffset CreationDate { get; set; }
    
    public int CreatorUserId { get; init; }
    
    public string GameState { get; init; }

    public string GameParticipation { get; init; }
    
    [MaxLength(4000)]
    public string HashedPassword { get; init; }
    
    public bool BotsArePaused { get; init; }
    
    public bool ObserversRequirePassword { get; init; }
    
    public bool StatisticsSent { get; init; }
    
    public DateTimeOffset LastAsyncPlayMessageSent { get; set; }
}