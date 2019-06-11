/**
 * Copyright 2019 The Nakama Authors
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

using System.Collections.Generic;
using Nakama.TinyJson;
using UnityEngine;

namespace Nakama.Snippets
{
    public class RealtimeChatRoom : MonoBehaviour
    {
        private const string RoomName = "heroes";

        private readonly IClient _client = new Client("defaultkey");
        private ISocket _socket;

        private async void Start()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var session = await _client.AuthenticateDeviceAsync(deviceId);
            Debug.LogFormat("Session user id: '{0}'", session.UserId);

            _socket = _client.NewSocket();
            _socket.Connected += () => Debug.Log("Socket connected.");
            _socket.Closed += () => Debug.Log("Socket closed.");

            var roomUsers = new List<IUserPresence>(10);
            _socket.ReceivedChannelPresence += presenceEvent =>
            {
                foreach (var presence in presenceEvent.Leaves)
                {
                    roomUsers.Remove(presence);
                }

                roomUsers.AddRange(presenceEvent.Joins);
                Debug.LogFormat("Room users: [{0}]", string.Join(",\n  ", roomUsers));
            };
            _socket.ReceivedChannelMessage += message => Debug.LogFormat("Received message: '{0}'", message);
            await _socket.ConnectAsync(session);

            // Join chat channel.
            var channel = await _socket.JoinChatAsync(RoomName, ChannelType.Room);
            roomUsers.AddRange(channel.Presences);

            // Send chat message.
            var content = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
            await _socket.WriteChatMessageAsync(channel, content);
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }
    }
}
