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

using System;
using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public class AuthenticateWithDevice : MonoBehaviour
{
    private readonly IClient _client = new Client("defaultkey", "127.0.0.1", 7350, false);

    private async void Awake()
    {
        var deviceid = SystemInfo.deviceUniqueIdentifier;
        var session = await _client.AuthenticateDeviceAsync(deviceid);

        Debug.LogFormat("User id '{0}'", session.UserId);
        Debug.LogFormat("User username '{0}'", session.Username);
        Debug.LogFormat("Session has expired: {0}", session.IsExpired);
        Debug.LogFormat("Session expires at: {0}", session.ExpireTime); // in seconds.

        var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        date = date.AddSeconds(session.ExpireTime).ToLocalTime();
        Debug.LogFormat("Session expires on: '{0}'", date);
    }
}
