/*
 * Copyright (C) 2020-2024 Ronald Ossendrijver (admin@treachery.online)
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This
 * program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have
 * received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Treachery.Client
{
    public class HostProxy
    {
        public int HostID;
        private readonly HubConnection _connection;

        public HostProxy(int hostID, HubConnection connection)
        {
            HostID = hostID;
            _connection = connection;
        }

        public async Task Request(GameEvent e)
        {
            try
            {
                await _connection.SendAsync("Request" + e.GetType().Name, HostID, e);
            }
            catch (Exception)
            {
                Support.Log("Disconnected...");
            }
        }

        public async Task Request(GameChatMessage message)
        {
            if (message.Body != null && message.Body.Length > 0)
            {
                try
                {
                    await _connection.SendAsync("RequestChatMessage", HostID, message);
                }
                catch (Exception)
                {
                    Support.Log("Disconnected...");
                }
            }
        }

        public async Task SendHeartbeat(string playerName)
        {
            try
            {
                await _connection.SendAsync("ProcessHeartbeat", HostID, playerName);
            }
            catch (Exception)
            {
                Support.Log("Disconnected...");
            }
        }

        public async Task SendVideo(int playerPosition, byte[] data)
        {
            try
            {
                await _connection.SendAsync("SendVideo", HostID, playerPosition, data);
            }
            catch (Exception)
            {
                Support.Log("Disconnected...");
            }
        }
    }
}
