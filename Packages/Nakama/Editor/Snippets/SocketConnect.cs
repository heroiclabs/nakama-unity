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

using UnityEngine;

namespace Nakama.Snippets
{
    public class SocketConnect : MonoBehaviour
    {
        private readonly IClient _client = new Client("defaultkey");
        private ISocket _socket;

        private async void Awake()
        {
            _socket = _client.NewSocket();
            _socket.Closed += () => Debug.Log("Socket closed.");
            _socket.Connected += () => Debug.Log("Socket connected.");
            _socket.ReceivedError += e => Debug.LogErrorFormat("Socket error: {0}", e.Message);

            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var session = await _client.AuthenticateDeviceAsync(deviceId);
            await _socket.ConnectAsync(session);
            Debug.Log("After socket connected.");
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }
    }
}
