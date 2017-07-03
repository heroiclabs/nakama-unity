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
    /// Wrap a socket client <see cref="NClient"/> so all actions are dispatched
    /// on the Unity main thread.
    /// <code>
    /// public class SomeObject : MonoBehaviour {
    ///     private NThreadedClient _client;
    ///
    ///     private void Start() {
    ///         var client = NClient.Default("somesecret");
    ///         _client = new NThreadedClient(client);
    ///     }
    ///
    ///     private void Update() {
    ///         _client.ExecuteActions();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class NThreadedClient
    {
        public uint ConnectTimeout { get { return _client.ConnectTimeout; } }

        public string Host { get { return _client.Host; } }

        public string Lang { get { return _client.Lang; } }

        public INLogger Logger { get { return _client.Logger; } }

        public Action OnDisconnect { get; set; }

        // NOTE This code causes an ICE with the Mono/Unity compiler.
        /*
        private IDictionary<Delegate, EventHandler> _onDisconnectMap;
        private EventHandler _onDisconnect;

        public event EventHandler OnDisconnect
        {
            add {
                EventHandler handler = delegate(object sender, EventArgs args) {
                    //MainThreadDispatcher.Enqueue(() => value(sender, args));
                    UnityEngine.Debug.Log("Here!");
                    value(sender, args);
                };
                lock (this)
                {
                    _onDisconnectMap.Add(value, handler);
                    _onDisconnect += handler;
                }
            }
            remove {
                EventHandler handler;
                _onDisconnectMap.TryGetValue(value, out handler);
                lock (this)
                {
                    _onDisconnect -= handler;
                }
            }
        }
        */

        public Action<NErrorEventArgs> OnError { get; set; }

        public Action<NMatchmakeMatchedEventArgs> OnMatchmakeMatched { get; set; }

        public Action<NMatchDataEventArgs> OnMatchData { get; set; }

        public Action<NMatchPresenceEventArgs> OnMatchPresence { get; set; }

        public Action<NTopicMessageEventArgs> OnTopicMessage { get; set; }

        public Action<NTopicPresenceEventArgs> OnTopicPresence { get; set; }

        public uint Port { get { return _client.Port; } }

        public string ServerKey { get { return _client.ServerKey; } }

        public long ServerTime { get { return _client.ServerTime; } }

        public bool SSL { get { return _client.SSL; } }

        public uint Timeout { get { return _client.Timeout; } }

        public bool Trace { get { return _client.Trace; } }

        private INClient _client;

        private Queue<Action> _executionQueue;

        public NThreadedClient(NClient client) : this(client, 1024)
        {
        }

        public NThreadedClient(NClient client, int initialSize)
        {
            _executionQueue = new Queue<Action>(initialSize);
            _client = client;
            _client.OnDisconnect += (object sender, EventArgs args) => {
                if (OnDisconnect != null)
                {
                    Enqueue(() => OnDisconnect());
                }
            };
            _client.OnError += (object sender, NErrorEventArgs args) => {
                if (OnError != null)
                {
                    Enqueue(() => OnError(args));
                }
            };
            _client.OnMatchmakeMatched += (object sender, NMatchmakeMatchedEventArgs args) => {
                if (OnMatchmakeMatched != null)
                {
                    Enqueue(() => OnMatchmakeMatched(args));
                }
            };
            _client.OnMatchData += (object sender, NMatchDataEventArgs args) => {
                if (OnMatchData != null)
                {
                    Enqueue(() => OnMatchData(args));
                }
            };
            _client.OnMatchPresence += (object sender, NMatchPresenceEventArgs args) => {
                if (OnMatchPresence != null)
                {
                    Enqueue(() => OnMatchPresence(args));
                }
            };
            _client.OnTopicMessage += (object sender, NTopicMessageEventArgs args) => {
                if (OnTopicMessage != null)
                {
                    Enqueue(() => OnTopicMessage(args));
                }
            };
            _client.OnTopicPresence += (object sender, NTopicPresenceEventArgs args) => {
                if (OnTopicPresence != null)
                {
                    Enqueue(() => OnTopicPresence(args));
                }
            };
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

        public void Send(INUncollatedMessage message, Action<bool> callback, Action<INError> errback)
        {
            _client.Send(message, (bool done) => {
                Enqueue(() => callback(done));
            }, (INError error) => {
                Enqueue(() => errback(error));
            });
        }

        private static IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }
    }
}
