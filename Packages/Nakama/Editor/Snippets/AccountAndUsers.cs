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
    public class AccountAndUsers : MonoBehaviour
    {
        private readonly IClient _client = new Client("defaultkey");

        private async void Awake()
        {
            var deviceid = SystemInfo.deviceUniqueIdentifier;
            const string username = "myusername";
            var session = await _client.AuthenticateDeviceAsync(deviceid, username);

            var account = await _client.GetAccountAsync(session);
            // Account properties.
            Debug.LogFormat("Account devices: [{0}]", string.Join(",", account.Devices));
            Debug.LogFormat("Account custom id: '{0}'", account.CustomId);
            Debug.LogFormat("Account email: '{0}'", account.Email);
            Debug.LogFormat("Account verify time: '{0}'", account.VerifyTime);
            Debug.LogFormat("Account wallet: '{0}'", account.Wallet);

            // User properties.
            Debug.LogFormat("User id: '{0}'", account.User.Id);
            Debug.LogFormat("User metadata: '{0}'", account.User.Metadata);
            Debug.LogFormat("User username: '{0}'", account.User.Username);
            Debug.LogFormat("User online: {0}", account.User.Online);

            var result = await _client.GetUsersAsync(session, new[] {session.UserId});
            Debug.LogFormat("Users: [{0}]", string.Join(",\n", result.Users));
        }
    }
}
