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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Nakama
{
    /// <summary>
    ///  A client for Nakama server which will dispatch all actions on the Unity
    ///  main thread.
    /// </summary>
    /// <example>
    /// Wrap a socket client <see cref="NClient"/> so all actions can be dispatched
    /// on the Unity main thread.
    /// <code>
    /// public class NakamaSessionManager : MonoBehaviour {
    ///     private INClient _client;
    ///
    ///     private void Start() {
    ///         var client = NClient.Default("somesecret");
    ///         _client = new NManagedClient(client);
    ///     }
    ///
    ///     private void Update() {
    ///         (_client as NManagedClient).ExecuteActions();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class NManagedClient : INClient
    {
        public uint ConnectTimeout { get { return _client.ConnectTimeout; } }

        public string Host { get { return _client.Host; } }

        public string Lang { get { return _client.Lang; } }

        public INLogger Logger { get { return _client.Logger; } }

        public Action<INDisconnectEvent> OnDisconnect {
            get {
                return _client.OnDisconnect;
            }
            set {
                _client.OnDisconnect = (INDisconnectEvent evt) => {
                    Enqueue(() => value(evt));
                };
            }
        }

        public Action<INError> OnError {
            get {
                return _client.OnError;
            }
            set {
                _client.OnError = (INError error) => {
                    Enqueue(() => value(error));
                };
            }
        }

        public Action<INMatchmakeMatched> OnMatchmakeMatched {
            get {
                return _client.OnMatchmakeMatched;
            }
            set {
                _client.OnMatchmakeMatched = (INMatchmakeMatched matched) => {
                    Enqueue(() => value(matched));
                };
            }
        }

        public Action<INMatchData> OnMatchData {
            get {
                return _client.OnMatchData;
            }
            set {
                _client.OnMatchData = (INMatchData match) => {
                    Enqueue(() => value(match));
                };
            }
        }

        public Action<INMatchPresence> OnMatchPresence {
            get {
                return _client.OnMatchPresence;
            }
            set {
                _client.OnMatchPresence = (INMatchPresence presence) => {
                    Enqueue(() => value(presence));
                };
            }
        }

        public Action<INTopicMessage> OnTopicMessage {
            get {
                return _client.OnTopicMessage;
            }
            set {
                _client.OnTopicMessage = (INTopicMessage message) => {
                    Enqueue(() => value(message));
                };
            }
        }

        public Action<INTopicPresence> OnTopicPresence {
            get {
                return _client.OnTopicPresence;
            }
            set {
                _client.OnTopicPresence = (INTopicPresence presence) => {
                    Enqueue(() => value(presence));
                };
            }
        }

        public Action<INNotification> OnNotification {
            get {
                return _client.OnNotification;
            }
            set {
                _client.OnNotification = (INNotification notification) => {
                    Enqueue(() => value(notification));
                };
            }
        }

        public uint Port { get { return _client.Port; } }

        public string ServerKey { get { return _client.ServerKey; } }

        public long ServerTime { get { return _client.ServerTime; } }

        public bool SSL { get { return _client.SSL; } }

        public uint Timeout { get { return _client.Timeout; } }

        public bool Trace { get { return _client.Trace; } }

        private NClient _client;

        private Queue<Action> _executionQueue;

        public NManagedClient(NClient client) : this(client, 1024)
        {
        }

        public NManagedClient(NClient client, int initialSize)
        {
            _executionQueue = new Queue<Action>(initialSize);
            _client = client;
        }

        public void Connect(INSession session)
        {
            _client.Connect(session);
        }

        public void Connect(INSession session, Action<bool> callback)
        {
            _client.Connect(session, (bool done) => {
                Enqueue(() => callback(done));
            });
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Disconnect(Action callback)
        {
            _client.Disconnect(() => {
                Enqueue(() => callback());
            });
        }

        private void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);

                // NOTE if client can't keep up we disconnect to prevent memory leaks.
                if (_executionQueue.Count > 1024)
                {
#if UNITY_5
                    var message = "Queued actions were not executed fast enough so forced client disconnect. Did you add '.ExecuteActions()' inside '.Update()'?";
                    UnityEngine.Debug.LogError(message);
#endif
                    _client.Disconnect();
                }
            }
        }

        public void ExecuteActions()
        {
            lock (_executionQueue)
            {
                for (int i = 0, l = _executionQueue.Count; i < l; i++)
                {
                    _executionQueue.Dequeue()();
                }
            }
        }

        public void Login(INAuthenticateMessage message, Action<INSession> callback, Action<INError> errback)
        {
            _client.Login(message, (INSession session) => {
                Enqueue(() => callback(session));
            }, (INError error) => {
                Enqueue(() => errback(error));
            });
        }

        public void Logout()
        {
            _client.Logout();
        }

        public void Logout(Action<bool> callback)
        {
            _client.Logout((bool done) => {
                Enqueue(() => callback(done));
            });
        }

        public void Register(INAuthenticateMessage message, Action<INSession> callback, Action<INError> errback)
        {
            _client.Register(message, (INSession session) => {
                Enqueue(() => callback(session));
            }, (INError error) => {
                Enqueue(() => errback(error));
            });
        }

        public void Send<T>(INCollatedMessage<T> message, Action<T> callback, Action<INError> errback)
        {
            _client.Send<T>(message, (T result) => {
                Enqueue(() => callback(result));
            }, (INError error) => {
                Enqueue(() => errback(error));
            });
        }

        public void Send(INUncollatedMessage message, bool reliable, Action<bool> callback, Action<INError> errback)
        {
            _client.Send(message, reliable, (bool done) =>
            {
                Enqueue(() => callback(done));
            }, (INError error) =>
            {
                Enqueue(() => errback(error));
            });
        }

        public void Send(INUncollatedMessage message, Action<bool> callback, Action<INError> errback)
        {
            Send(message, true, callback, errback);
        }

        private static IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }
    }
}
