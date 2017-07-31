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
using System.Collections;
using UnityEngine;

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

    // Store session from the current logged in user.
    private INSession _session;

    private string _uiLabelMessage;

    public UnityMainThreadDispatch()
    {
        // A new Nakama client which uses a socket thread.
        var client = new NClient.Builder(ServerKey)
                .Host(ServerHost)
                .Port(ServerPort)
                .SSL(ServerSsl)
                .Build();
        // Wrap the socket client so we dispatch all callbacks on the main thread.
        _client = new NManagedClient(client);
        _uiLabelMessage = "Initial state.";

        // Attach an error handler for received message errors.
        _client.OnError = (INError error) => {
            ErrorHandler(error);
        };
        // Log a message whenever we're disconnected from the server.
        _client.OnDisconnect = (INDisconnectEvent evt) => {
            Debug.Log("Disconnected from server.");
            // We'll set a UI label from the socket.
            _uiLabelMessage = "Disconnected.";
        };
    }

    private void Awake()
    {
        RestoreSessionAndConnect();
        if (_session == null)
        {
            // This will also connect a client.
            LoginOrRegister();
        }
    }

    private void OnApplicationQuit()
    {
        if (_session != null)
        {
            _client.Disconnect();
        }
    }

    private void OnGUI()
    {
        // Set initial UI.
        SetLabel(_uiLabelMessage);
    }

    private void Start()
    {
        // Wait before we force a disconnect.
        StartCoroutine(Wait(10, () => _client.Disconnect()));
    }

    private void Update()
    {
        // This ensures all callbacks are executed on the main thread.
        (_client as NManagedClient).ExecuteActions();
    }

    private void RestoreSessionAndConnect()
    {
        // Lets check if we can restore a cached session.
        var sessionString = PlayerPrefs.GetString("nk.session");
        if (string.IsNullOrEmpty(sessionString))
        {
            // We have no session to restore.
            return;
        }
        var session = NSession.Restore(sessionString);
        if (session.HasExpired(DateTime.UtcNow))
        {
            // We can't restore an expired session.
            return;
        }
        SessionHandler(session);
    }

    private void LoginOrRegister()
    {
        // See if we have a cached id in PlayerPrefs.
        var id = PlayerPrefs.GetString("nk.id");
        if (string.IsNullOrEmpty(id)) {
            // We'll use device ID for the user account. Have a look at the docs
            // for other options: https://heroiclabs.com/docs/development/authentication/
            id = SystemInfo.deviceUniqueIdentifier;
            // Store the identifier for next game start.
            PlayerPrefs.SetString("nk.id", id);
        }

        var message = NAuthenticateMessage.Device(id);
        _client.Login(message, SessionHandler, (INError error) => {
            if (error.Message.Equals("ID not found"))
            {
                _client.Register(message, SessionHandler, ErrorHandler);
                return;
            }
            Debug.LogErrorFormat("Login error: code '{0}' - '{1}'.", error.Code, error.Message);
        });
    }

    private void SessionHandler(INSession session)
    {
        Debug.LogFormat("Session: '{0}'.", session.Token);
        _session = session;
        _client.Connect(_session, (bool done) => {
            Debug.Log("Session connected.");
            // We'll set a UI label from the socket.
            _uiLabelMessage = "Connected.";
            // Store session for quick reconnects.
            PlayerPrefs.SetString("nk.session", session.Token);
        });
    }

    // In this example we'll re-use the same error handler.
    private static void ErrorHandler(INError error)
    {
        Debug.LogErrorFormat("Send error: code '{0}' - '{1}'.", error.Code, error.Message);
    }

    private static void SetLabel(string labelText)
    {
        // UI code should only be set on the Unity main thread.
        GUI.Label(new Rect(10, 10, 400, 20), labelText);
    }

    // Add a util function for delayed wait.
    private static IEnumerator Wait(int seconds, Action action)
    {
        yield return new WaitForSeconds((float)seconds);
        action();
    }
}
