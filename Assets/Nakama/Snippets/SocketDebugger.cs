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
using UnityEngine;

public class SocketDebugger : MonoBehaviour
{
    private IClient _client = new Client("defaultkey", "127.0.0.1", 7350, false);
    private ISocket _socket;

    async void Awake()
    {
        var deviceid = SystemInfo.deviceUniqueIdentifier;
        var session = await _client.AuthenticateDeviceAsync(deviceid);

        _socket = _client.CreateWebSocket();

        await _socket.ConnectAsync(session);
    }

    private float waitTime = 1f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > waitTime && _socket != null)
        {
           _socket.RpcAsync("somefunc", string.Empty);
        }
    }

    async void OnApplicationQuit()
    {
        if (_socket != null)
        {
            await _socket.DisconnectAsync(false);
        }
    }
}
