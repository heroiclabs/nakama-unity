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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama.Snippets
{
    public class MatchmakerWithRelayedMultiplayer : MonoBehaviour
    {
        private readonly IClient _client = new Client("defaultkey");
        private ISocket _socket;

        private async void Start()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var session = await _client.AuthenticateDeviceAsync(deviceId);
            Debug.Log(session);

            _socket = _client.NewSocket();
            _socket.Connected += () => Debug.Log("Socket connected.");
            _socket.Closed += (reason) => Debug.LogFormat("Socket closed: {0}", reason);
            _socket.ReceivedError += Debug.LogError;

            IUserPresence self = null;
            var connectedOpponents = new List<IUserPresence>(2);
            _socket.ReceivedMatchmakerMatched += async matched =>
            {
                Debug.LogFormat("Matched result: {0}", matched);
                var match = await _socket.JoinMatchAsync(matched);

                self = match.Self;
                Debug.LogFormat("Self: {0}", self);
                connectedOpponents.AddRange(match.Presences);
            };
            _socket.ReceivedMatchPresence += presenceEvent =>
            {
                foreach (var presence in presenceEvent.Leaves)
                {
                    connectedOpponents.Remove(presence);
                }
                connectedOpponents.AddRange(presenceEvent.Joins);
                // Remove yourself from connected opponents.
                connectedOpponents.Remove(self);
                Debug.LogFormat("Connected opponents: [{0}]", string.Join(",\n  ", connectedOpponents));
            };
            await _socket.ConnectAsync(session);
            Debug.Log("After socket connected.");
            await _socket.AddMatchmakerAsync("*", 2, 2);


            // NOTE As an example create a second user and socket to matchmake against.
            var deviceId2 = Guid.NewGuid().ToString();
            var session2 = await _client.AuthenticateDeviceAsync(deviceId2);
            var socket2 = _client.NewSocket();
            socket2.ReceivedMatchmakerMatched += async matched => await socket2.JoinMatchAsync(matched);
            await socket2.ConnectAsync(session2);
            await socket2.AddMatchmakerAsync("*", 2, 2);
            await Task.Delay(TimeSpan.FromSeconds(10)); // disconnect after 10 seconds.
            Debug.Log("After delay socket2 closed.");
            await socket2.CloseAsync();
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }
    }
}
