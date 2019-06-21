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

using System;
using UnityEngine;

namespace Nakama.Snippets
{
    // ReSharper disable once InconsistentNaming
    public class WebGLExample : MonoBehaviour
    {
        private const string SessionTokenKey = "nksession";
        private const string UdidKey = "udid";

        private IClient _client;
        private ISocket _socket;

        private async void Awake()
        {
            _client = new Client("defaultkey", UnityWebRequestAdapter.Instance);
            _socket = _client.NewSocket();
            _socket.Closed += () => Debug.Log("Socket closed.");
            _socket.Connected += () => Debug.Log("Socket connected.");
            _socket.ReceivedError += e => Debug.LogErrorFormat("Socket error: {0}", e.Message);

            // Cant use SystemInfo.deviceUniqueIdentifier with WebGL builds.
            var udid = PlayerPrefs.GetString(UdidKey, Guid.NewGuid().ToString());
            Debug.LogFormat("Unique Device ID: {0}", udid);

            ISession session;
            var sessionToken = PlayerPrefs.GetString(SessionTokenKey);
            if (string.IsNullOrEmpty(sessionToken) || (session = Session.Restore(sessionToken)).IsExpired)
            {
                session = await _client.AuthenticateDeviceAsync(udid);
                PlayerPrefs.SetString(UdidKey, udid);
                PlayerPrefs.SetString(SessionTokenKey, session.AuthToken);
            }

            Debug.LogFormat("Session Token: {0}", session.AuthToken);
            await _socket.ConnectAsync(session, true);
            Debug.Log("After socket connected.");
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }
    }
}
