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
using System.Collections.Generic;
using UnityEngine;

namespace Nakama
{
    /// <summary>
    ///  A client for Nakama server which will dispatch all actions on the Unity
    ///  main thread.
    /// </summary>
    public class NTDispatchClient : INClient, MonoBehaviour
    {
        // NOTE Not compiled by default; avoids dependency on UnityEngine

        public uint ConnectTimeout { get { return _client.ConnectTimeout; } }

        public string Host { get { return _client.Host; } }

        public string Lang { get { return _client.Lang; } }

        public INLogger Logger { get { return _client.Logger; } }

        private EventHandler _onDisconnect;

        public event EventHandler OnDisconnect {
            add {
                _onDisconnect = (EventHandler) Delegate.Combine(_onDisconnect, () => {
                    Enqueue(() => value());
                });
            }
            remove {
                // FIXME
            }
        }

        public event EventHandler<NErrorEventArgs> OnError;

        public event EventHandler<NMatchmakeMatchedEventArgs> OnMatchmakeMatched;

        public event EventHandler<NMatchDataEventArgs> OnMatchData;

        public event EventHandler<NMatchPresenceEventArgs> OnMatchPresence;

        public event EventHandler<NTopicMessageEventArgs> OnTopicMessage;

        public event EventHandler<NTopicPresenceEventArgs> OnTopicPresence;

        public uint Port { get { return _client.Port; } }

        public string ServerKey { get { return _client.ServerKey; } }

        public long ServerTime { get { return _client.ServerTime; } }

        public bool SSL { get { return _client.SSL; } }

        public uint Timeout { get { return _client.Timeout; } }

        public bool NoDelay { get { return _client.NoDelay; } }

        public bool Trace { get { return _client.Trace; } }

        private INClient _client;

        private Queue<IEnumerator> _executionQueue;

        public NTDispatchClient(NClient client)
            : this(client, 2048)
        {
        }

        public NTDispatchClient(NClient client, int initialQueueSize)
        {
            if (initialQueueSize < 1)
            {
                throw new ArgumentException("'initialQueueSize' cannot be less than 1.");
            }
            _client = client;
            _executionQueue = new Queue<IEnumerator>(initialQueueSize);
        }

        public void Update()
        {
            lock (_executionQueue)
            {
                for (int i = 0, l = _executionQueue.Count; i < l; i++)
                {
                    StartCoroutine(_executionQueue.Dequeue());
                }
            }
        }

        private void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(WrapAction(action));
            }
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

        private static IEnumerator WrapAction(Action action)
        {
            action();
            yield return null;
        }
    }
}
