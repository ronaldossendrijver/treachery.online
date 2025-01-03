// /*
//  * Copyright (C) 2020-2025 Ronald Ossendrijver (admin@treachery.online)
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
//  * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
//  * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
//  * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
// */

using System.Threading.Tasks;

namespace Treachery.Shared;

public interface IGameClient
{
    Task HandleGameEvent<TEvent>(TEvent evt, int newEventNumber) where TEvent : GameEvent;
    Task HandleChatMessage(GameChatMessage gameChatMessage);
    Task HandleGlobalChatMessage(GlobalChatMessage message);
    Task HandleSetTimer(int value);
    Task HandleSetSkin(string skin);
    Task HandleUndo(int untilEventNr);
    Task HandleJoinGame(int userId, string userName, int seat);
    Task HandleSetOrUnsetHost(int userId);
    Task HandleObserveGame(int userId, string userName);
    Task HandleOpenOrCloseSeat(int seat);
    Task HandleRemoveUser(int userId, bool kick);
    Task HandleBotStatus(bool botsArePaused);
    Task HandleLoadGame(GameInitInfo initInfo);
    Task HandleAssignSeats(Dictionary<int, int> assignment);
}