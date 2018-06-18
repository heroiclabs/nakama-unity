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

public class SessionWithPlayerPrefs : MonoBehaviour
{
    private const string PrefKeyName = "nakama.session";

    private IClient _client = new Client("defaultkey", "127.0.0.1", 7350, false);
    private ISession _session;

    private async void Awake()
    {
        var authtoken = PlayerPrefs.GetString(PrefKeyName);
        if (!string.IsNullOrEmpty(authtoken))
        {
            var session = Session.Restore(authtoken);
            if (!session.IsExpired)
            {
                _session = session;
                Debug.Log(_session);
                return;
            }
        }
        var deviceid = SystemInfo.deviceUniqueIdentifier;
        _session = await _client.AuthenticateDeviceAsync(deviceid);
        PlayerPrefs.SetString(PrefKeyName, _session.AuthToken);
        Debug.Log(_session);
    }
}
