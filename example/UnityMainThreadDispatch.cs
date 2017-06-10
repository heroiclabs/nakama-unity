/**
 * Copyright 2017 The Nakama Authors
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

using Nakama;
using System;

/// <summary>
///  This example demonstrates how to dispatch all action callbacks on the
///  Unity main thread. For an example on how to manually control how callbacks
///  are dispatched have a look at the Matchmake example scene.
/// </summary>
public class UnityMainThreadDispatch : MonoBehaviour
{
    private const string ServerHost = "127.0.0.1";
    private const int ServerPort = 7350;
    private const string ServerKey = "defaultkey";
    private const bool ServerSsl = false;

    // Nakama client.
    private readonly INClient _client;

    // Stores information on the current logged in user.
    private INSession _session;

    public UnityMainThreadDispatch()
    {
        // A new Nakama client which uses a socket thread.
        var client = new NClient.Builder(ServerKey)
                .Host(ServerHost)
                .Port(ServerPort)
                .SSL(ServerSsl)
                .Build();
        // Wrap the socket thread so we dispatch all callbacks on the main thread.
        _client = new NTDispatchClient(client);
    }

    private void LoginOrRegister()
    {
        // Lets see if we have a cached id in PlayerPrefs
        var id = PlayerPrefs.GetString("nkid");
        if (string.IsNullOrEmpty(id)) {
            // We'll use device ID for the user account. Have a look at the docs
            // for other options: https://heroiclabs.com/docs/development/authentication/
            id = SystemInfo.deviceUniqueIdentifier;
            // Store the identifier for next game start
            PlayerPrefs.SetString("nkid", id);
        }

        var message = NAuthenticateMessage.Device(id);
        client.Login(message, (INSession session) => {
            Debug.Log ("Login successful: '{0}'.", session.Token);
            client.Connect(session);
        }, (INError error) => {
            Debug.LogErrorFormat ("Could not login user: '{0}'.", error.Message);
        });
    }

    // In this example we'll re-use the same error handler
    private static void ErrorHandler(INError error)
    {
        Debug.LogErrorFormat ("Send error: code '{0}' - '{1}'.", error.Code, error.Message);
    }
}
