// Copyright 2019 The Nakama Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using Nakama.TinyJson;
using UnityEngine;

namespace Nakama.Snippets
{
    public class RealtimeChatRoom : MonoBehaviour
    {
        private const string RoomName = "heroes";

        private IClient _client;
        private ISocket _socket;

        private async void Start()
        {
            _client =  new Client("defaultkey", UnityWebRequestAdapter.Instance);

            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var session = await _client.AuthenticateDeviceAsync(deviceId);
            Debug.LogFormat("Session user id: '{0}'", session.UserId);

            _socket = _client.NewSocket();
            _socket.Connected += () => Debug.Log("Socket connected.");
            _socket.Closed += (reason) => Debug.LogFormat("Socket closed: {0}", reason);
            _socket.ReceivedError += Debug.LogError;

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

            var messageList = new List<IApiChannelMessage>(100);
            _socket.ReceivedChannelMessage += message =>
            {
                Debug.LogFormat("Received message: '{0}'", message);
                AddListSorted(messageList, message);
                Debug.LogFormat("Message list: {0}", string.Join(",\n  ", messageList));
            };
            await _socket.ConnectAsync(session);
            Debug.Log("After socket connected.");

            // Join chat channel.
            var channel = await _socket.JoinChatAsync(RoomName, ChannelType.Room);
            roomUsers.AddRange(channel.Presences);
            Debug.LogFormat("Joined chat channel: {0}", channel);

            // Send many chat messages.
            var content = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
            _ = _socket.WriteChatMessageAsync(channel, content);
            _ = _socket.WriteChatMessageAsync(channel, content);
            _ = _socket.WriteChatMessageAsync(channel, content);
            _ = _socket.WriteChatMessageAsync(channel, content);
            _ = _socket.WriteChatMessageAsync(channel, content);
            _ = _socket.WriteChatMessageAsync(channel, content);
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }

        private static void AddListSorted(List<IApiChannelMessage> messageList, IApiChannelMessage message)
        {
            messageList.Add(message);
            messageList.Sort((a, b) =>
            {
                var ordinal = string.CompareOrdinal(a.CreateTime, b.CreateTime);
                return ordinal == 0 ? string.CompareOrdinal(a.MessageId, b.MessageId) : ordinal;
            });
        }
    }
}
