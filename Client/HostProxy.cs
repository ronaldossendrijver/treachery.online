/*
 * Copyright 2020-2022 Ronald Ossendrijver. All rights reserved.
 */

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Treachery.Shared;

namespace Treachery.Client
{
    public class HostProxy
    {
        public int HostID;
        private HubConnection _connection;

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

        public async Task Request(ChatMessage e)
        {
            if (e.Body != null && e.Body.Length > 0)
            {
                try
                {
                    await _connection.SendAsync("RequestChatMessage", HostID, e);
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
