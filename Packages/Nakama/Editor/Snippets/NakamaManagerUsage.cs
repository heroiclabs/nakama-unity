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
    /// <seealso cref="NakamaManager"/>
    public class NakamaManagerUsage : MonoBehaviour
    {
        private async void Start()
        {
            var session = await NakamaManager.Instance.Session;
            Debug.LogFormat("Active Session: {0}", session);
            var account = await NakamaManager.Instance.Client.GetAccountAsync(session);
            Debug.LogFormat("Account id: {0}", account.User.Id);

            NakamaManager.Instance.Socket.Closed += () => Debug.Log("Socket closed.");
            NakamaManager.Instance.Socket.Connected += () => Debug.Log("Socket connected.");
            NakamaManager.Instance.Socket.ReceivedError += Debug.LogError;
            await NakamaManager.Instance.Socket.ConnectAsync(session);
        }
    }
}
