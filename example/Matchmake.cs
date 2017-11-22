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
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Matchmake : MonoBehaviour {

    private const string ServerHost = "127.0.0.1";
    private const int ServerPort = 7350;
    private const string ServerKey = "defaultkey";
    private const bool ServerSsl = false;

    // number of players participating in the match.
    private const int MatchPlayerSize = 2;
    // A custom Opcode to distinguish different message types
    private const int MatchOpCode = 16;

    // Nakama client
    private readonly INClient _client;
    // The current player's login ID.
    private readonly string _customUserId;

    // Matchmaking matchmakeTicket - used to cancel matchmaking request.
    private INMatchmakeTicket _matchmakeTicket;
    // Ongoing match - used to send data
    private INMatch _match;

    // Send Match Data button
    private Button _matchSendDataButton;
    // State of the send match data button
    private bool _matchSendDataButtonEnable;

    // Store actions which will need to be dispatched on the Unity main thread.
    private Queue<System.Action> actionQueue;

    [Serializable]
    public class Text {
        public string timestamp;
        public string message;
    }

    public Matchmake() {
        // Create a new User ID for demo purposes. In a real game you should use
        // a device ID or social ID.
        // For more information have a look at: https://heroiclabs.com/docs/development/user/
        _customUserId = String.Format("nakama_demo_{0}", DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        // Create new client
        _client = new NClient.Builder(ServerKey)
                .Host(ServerHost)
                .Port(ServerPort)
                .SSL(ServerSsl)
                .Build();

        actionQueue = new Queue<System.Action>(1024);
    }

    void Start() {
        _matchSendDataButton = GameObject.Find("MatchSendDataButton").GetComponent<Button>();

        RegisterAndConnect();
        SetupMatchmakeListener();
        SetupMatchPresenceListener();
        SetupMatchDataListener();
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0, l = actionQueue.Count; i < l; i++)
        {
            actionQueue.Dequeue()();
        }
        _matchSendDataButton.interactable = _matchSendDataButtonEnable;
        // You must not do any network calls on the Update loop.
    }

    void OnApplicationQuit() {
        // If no matchmaking active just disconnect.
        if (_matchmakeTicket == null) {
            // Lets gracefully disconnect from the server.
            _client.Disconnect();
            return;
        }

        // If matchmaking active stop matchmaking then disconnect.
        var message = NMatchmakeRemoveMessage.Default(_matchmakeTicket);
        _client.Send(message, (bool done) => {
            // The user is now removed from the matchmaker pool.
            Debug.Log("Matchmaking stopped.");

            // Lets gracefully disconnect from the server.
            _client.Disconnect();
        }, (INError error) => {
            Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);

            // Lets gracefully disconnect from the server.
            _client.Disconnect();
        });
    }

    private void RegisterAndConnect() {
        // Lets log a message whenever we're disconnected from the server.
        _client.OnDisconnect = (INDisconnectEvent evt) => {
            Debug.Log("Disconnected from the server.");
        };

        // This can be DeviceID, Facebook, Google, GameCenter or a custom ID.
        // For more information have a look at: https://heroiclabs.com/docs/development/user/
        var message = NAuthenticateMessage.Custom(_customUserId);
        _client.Register(message, (INSession session) => {
            Debug.LogFormat("Registered user.");
            // We suggest that you cache the Session object on device storage
            // so you don't have to login each time the game starts.
            // For demo purposes - we'll ignore that.

            _client.Connect(session, (bool connected) => {
                Debug.Log("Socket connected.");
            });
        }, (INError error) => {
            Debug.LogErrorFormat("ID register '{0}' failed: {1}", _customUserId, error);
        });
    }

    private void SetupMatchmakeListener() {
        // Attach an event handler for when the user is matched with other users.
        // This only needs to be done once.
        // For more information have a look at: https://heroiclabs.com/docs/development/matchmaker/
        _client.OnMatchmakeMatched = (INMatchmakeMatched matched) => {
            // Set the local cache to null now that we've been offered a match.
            _matchmakeTicket = null;
            Debug.Log("Matchmaker found an opponent.");

            // The match token is used to join a multiplayer match.
            // Lets accept the match and join it.
            var message = NMatchJoinMessage.Default(matched.Token);
            _client.Send(message, (INResultSet<INMatch> matches) => {
                Debug.Log("Successfully joined matches.");

                _match = matches.Results[0];
                foreach (var presence in _match.Presence) {
                    Debug.LogFormat("Presence initial state: User handle '{0}' is in the match.", presence.Handle);
                }

                // Lets enable the button now that we've joined a match.
                _matchSendDataButtonEnable = true;
            }, (INError error) => {
                Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);
            });
        };
    }

    private void SetupMatchPresenceListener() {
        // For more information have a look at: https://heroiclabs.com/docs/development/realtime-multiplayer/
        _client.OnMatchPresence = (INMatchPresence presences) => {
            // `args.MatchPresence.Id` to get the `byte[]` match ID this update relates to.
            // `args.MatchPresence.Join` and `args.MatchPresence.Leave` for lists of
            // presences that have joined/left since the last update for this match.
            foreach (var presence in presences.Join) {
                Debug.LogFormat("Presence update: User handle '{0}' joined the match.", presence.Handle);
            }
            foreach (var presence in presences.Leave) {
                Debug.LogFormat("Presence update: User handle '{0}' left the match.", presence.Handle);
            }
        };
    }

    private void SetupMatchDataListener() {
        _client.OnMatchData = (INMatchData matchData) => {
            // We enqueue any logic which we want to execute on the Unity main thread.
            actionQueue.Enqueue(() => {
                // `args.MatchData.Id` to get the `byte[]` match ID this data relates to.
                // `args.MatchData.Presence` is the sender of this data.
                // `args.MatchData.OpCode` and `args.MatchData.Data` are the custom
                // fields set by the sender.

                var received = matchData;
                switch (received.OpCode) {
                case MatchOpCode:
                    var dataString = Encoding.UTF8.GetString(received.Data);
                    Debug.LogFormat("Received match data from {0}: {1}", received.Presence.Handle, dataString);
                    break;
                default:
                    Debug.Log("Received data but didn't match expected OpsCode.");
                    break;
                }
            });
        };
    }

    // This is invoked by clicking on the scene UI button.
    public void DoMatchmake() {
        // Add self to the matchmaker pool.
        var message = NMatchmakeAddMessage.Default(MatchPlayerSize);
        _client.Send(message, (INMatchmakeTicket ticket) => {
            // Store the matchmakeTicket so it can be used to cancel later.
            Debug.Log("Added to the pool by Matchmaker.");
            _matchmakeTicket = ticket;
        }, (INError error) => {
            Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);
        });
    }

    // This is invoked by clicking on the scene UI button.
    public void SendMatchData() {
        var text = new Text();
        text.timestamp = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
        text.message = "Hello world.";

        var json = JsonUtility.ToJson(text);
        var data = Encoding.UTF8.GetBytes(json);

        // `matchId` is the `byte[]` ID of the match to send to.
        // `opCode` is any desired integer.
        // `data` is any `byte[]` of data to send.
        var message = NMatchDataSendMessage.Default(_match.Id, MatchOpCode, data);
        _client.Send(message, (bool complete) => {
            Debug.Log("Successfully sent data to match.");
        }, error => {
            Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);
        });
    }
}
