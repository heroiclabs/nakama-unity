
// Copyright 2023 The Nakama Authors
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Satori.Snippets
{
    public class SatoriExample : MonoBehaviour
    {
        private const string ApiKey = "bb4b2da1-71ba-429e-b5f3-36556abbf4c9";

        private IClient _testClient;

        private async void Awake()
        {
            _testClient = new Client("http", "localhost", 7450, ApiKey, UnityWebRequestAdapter.Instance);
            Debug.Log("authenticating satori");
            try
            {
                var session = await _testClient.AuthenticateAsync($"{Guid.NewGuid()}");
                await _testClient.GetExperimentsAsync(session, Array.Empty<string>());
                var experiments = await _testClient.GetAllExperimentsAsync(session);
                Debug.Log("num experiments is " + experiments.Experiments.Count());
                await _testClient.AuthenticateLogoutAsync(session);
                Debug.Log("logged out of satori");

            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

}