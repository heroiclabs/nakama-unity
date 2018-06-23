/**
 * Copyright 2018 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;

public class ChatRoom : MonoBehaviour
{
    private const string RoomName = "heroes";

    private IClient _client = new Client("defaultkey", "127.0.0.1", 7350, false);
    private ISocket _socket;

    private async void Start()
    {
        var deviceid = SystemInfo.deviceUniqueIdentifier;
        // NOTE should cache a user session.
        var session = await _client.AuthenticateDeviceAsync(deviceid);
        Debug.LogFormat("Session '{0}'", session);

        _socket = _client.CreateWebSocket();

        var connectedUsers = new List<IUserPresence>(0);
        _socket.OnChannelPresence += (sender, presenceChange) =>
        {
            connectedUsers.AddRange(presenceChange.Joins);
            foreach (var leave in presenceChange.Leaves)
            {
                connectedUsers.RemoveAll(item => item.SessionId.Equals(leave.SessionId));
            };

            // Print connected presences.
            var presences = string.Join(", ", connectedUsers);
            Debug.LogFormat("Presence List\n {0}", presences);
        };
        _socket.OnChannelMessage += (sender, message) =>
        {
            Debug.LogFormat("Received Message '{0}'", message);
        };
        _socket.OnConnect += (sender, evt) => Debug.Log("Socket connected.");
        _socket.OnDisconnect += (sender, evt) => Debug.Log("Socket disconnected.");

        await _socket.ConnectAsync(session);

        // Join chat channel.
        var channel = await _socket.JoinChatAsync(RoomName, ChannelType.Room);
        connectedUsers.AddRange(channel.Presences);

        // Send chat message.
        var content = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
        await _socket.WriteChatMessageAsync(channel, content);
    }

    private async void OnApplicationQuit()
    {
        if (_socket != null)
        {
            await _socket.DisconnectAsync(false);
        }
    }
}
