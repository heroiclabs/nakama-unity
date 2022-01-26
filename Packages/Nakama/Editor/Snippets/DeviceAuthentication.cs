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
using UnityEngine;

namespace Nakama.Snippets
{
    public class DeviceAuthentication : MonoBehaviour
    {
        private const string SessionTokenKey = "nksession";
        private const string UdidKey = "udid";

        private readonly IClient _client = new Client("defaultkey");

        private async void Awake()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;

            // Restore session from PlayerPrefs if possible.
            var sessionToken = PlayerPrefs.GetString(SessionTokenKey);
            var session = Session.Restore(sessionToken);
            // Add a day so we check whether the token is within a day of expiration to refresh it.
            var expiredDate = DateTime.UtcNow.AddDays(1);
            if (session == null || session.HasExpired(expiredDate))
            {
                session = await _client.AuthenticateDeviceAsync(deviceId);
                PlayerPrefs.SetString(UdidKey, deviceId);
                PlayerPrefs.SetString(SessionTokenKey, session.AuthToken);
            }

            Debug.LogFormat("Session user id: '{0}'", session.UserId);
            Debug.LogFormat("Session username: '{0}'", session.Username);
            Debug.LogFormat("Session expired: {0}", session.IsExpired);
            Debug.LogFormat("Session expires: '{0}'", session.ExpireTime); // in seconds.

            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            Debug.LogFormat("Session expires on: '{0}'", unixEpoch.AddSeconds(session.ExpireTime).ToLocalTime());
        }
    }
}
