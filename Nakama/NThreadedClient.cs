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
    public class NThreadedClient
    {
        // NOTE: Not compiled by default; avoids dependency on UnityEngine.

        public uint ConnectTimeout { get { return _client.ConnectTimeout; } }

        public string Host { get { return _client.Host; } }

        public string Lang { get { return _client.Lang; } }

        public INLogger Logger { get { return _client.Logger; } }

        private Action _onDisconnect;
        public Action OnDisconnect
        {
            get {
                return _onDisconnect;
            }
            set {
                Action action = delegate() {
                    MainThreadDispatcher.Enqueue(() => value());
                };
                _onDisconnect = action;
                //_client.OnDisconnect = null; // wipe all registered event handlers
                _client.OnDisconnect += (object sender, EventArgs args) => {
                    action();
                };
            }
        }

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

        private Action<NErrorEventArgs> _onError;
        public Action<NErrorEventArgs> OnError
        {
            get {
                return _onError;
            }
            set {
                Action<NErrorEventArgs> action = delegate(NErrorEventArgs args) {
                    MainThreadDispatcher.Enqueue(() => value(args));
                };
                _onError = action;
                //_client.OnError = null; // wipe all registered event handlers.
                _client.OnError += (object sender, NErrorEventArgs args) => {
                    action(args);
                };
            }
        }

        private Action<NMatchmakeMatchedEventArgs> _onMatchmakeMatched;
        public Action<NMatchmakeMatchedEventArgs> OnMatchmakeMatched
        {
            get {
                return _onMatchmakeMatched;
            }
            set {
                Action<NMatchmakeMatchedEventArgs> action = delegate(NMatchmakeMatchedEventArgs args) {
                    MainThreadDispatcher.Enqueue(() => value(args));
                };
                _onMatchmakeMatched = action;
                //_client.OnMatchmakeMatched = null; // wipe all registered event handlers.
                _client.OnMatchmakeMatched += (object sender, NMatchmakeMatchedEventArgs args) => {
                    action(args);
                };
            }
        }

        private Action<NMatchDataEventArgs> _onMatchData;
        public Action<NMatchDataEventArgs> OnMatchData
        {
            get {
                return _onMatchData;
            }
            set {
                Action<NMatchDataEventArgs> action = delegate(NMatchDataEventArgs args) {
                    MainThreadDispatcher.Enqueue(() => value(args));
                };
                _onMatchData = action;
                //_client.OnMatchData = null; // wipe all registered event handlers.
                _client.OnMatchData += (object sender, NMatchDataEventArgs args) => {
                    action(args);
                };
            }
        }

        private Action<NMatchPresenceEventArgs> _onMatchPresence;
        public Action<NMatchPresenceEventArgs> OnMatchPresence
        {
            get {
                return _onMatchPresence;
            }
            set {
                Action<NMatchPresenceEventArgs> action = delegate(NMatchPresenceEventArgs args) {
                    MainThreadDispatcher.Enqueue(() => value(args));
                };
                _onMatchPresence = action;
                //_client.OnMatchPresence = null; // wipe all registered event handlers.
                _client.OnMatchPresence += (object sender, NMatchPresenceEventArgs args) => {
                    action(args);
                };
            }
        }

        private Action<NTopicMessageEventArgs> _onTopicMessage;
        public Action<NTopicMessageEventArgs> OnTopicMessage
        {
            get {
                return _onTopicMessage;
            }
            set {
                Action<NTopicMessageEventArgs> action = delegate(NTopicMessageEventArgs args) {
                    MainThreadDispatcher.Enqueue(() => value(args));
                };
                _onTopicMessage = action;
                //_client.OnTopicMessage = null; // wipe all registered event handlers.
                _client.OnTopicMessage += (object sender, NTopicMessageEventArgs args) => {
                    action(args);
                };
            }
        }

        private Action<NTopicPresenceEventArgs> _onTopicPresence;
        public Action<NTopicPresenceEventArgs> OnTopicPresence
        {
            get {
                return _onTopicPresence;
            }
            set {
                Action<NTopicPresenceEventArgs> action = delegate(NTopicPresenceEventArgs args) {
                    MainThreadDispatcher.Enqueue(() => value(args));
                };
                _onTopicPresence = action;
                _client.OnTopicPresence = null; // wipe all registered event handlers.
                _client.OnTopicPresence += (object sender, NTopicPresenceEventArgs args) => {
                    action(args);
                };
            }
        }

        public uint Port { get { return _client.Port; } }

        public string ServerKey { get { return _client.ServerKey; } }

        public long ServerTime { get { return _client.ServerTime; } }

        public bool SSL { get { return _client.SSL; } }

        public uint Timeout { get { return _client.Timeout; } }

        public bool Trace { get { return _client.Trace; } }

        private INClient _client;

        public NThreadedClient(NClient client)
        {
            _client = client;
        }

        public void Connect(INSession session)
        {
            _client.Connect(session);
        }

        public void Connect(INSession session, Action<bool> callback)
        {
            _client.Connect(session, (bool done) => {
                MainThreadDispatcher.Enqueue(() => callback(done));
            });
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Disconnect(Action callback)
        {
            _client.Disconnect(() => {
                MainThreadDispatcher.Enqueue(() => callback());
            });
        }

        public void Login(INAuthenticateMessage message, Action<INSession> callback, Action<INError> errback)
        {
            _client.Login(message, (INSession session) => {
                MainThreadDispatcher.Enqueue(() => callback(session));
            }, (INError error) => {
                MainThreadDispatcher.Enqueue(() => errback(error));
            });
        }

        public void Logout()
        {
            _client.Logout();
        }

        public void Logout(Action<bool> callback)
        {
            _client.Logout((bool done) => {
                MainThreadDispatcher.Enqueue(() => callback(done));
            });
        }

        public void Register(INAuthenticateMessage message, Action<INSession> callback, Action<INError> errback)
        {
            _client.Register(message, (INSession session) => {
                MainThreadDispatcher.Enqueue(() => callback(session));
            }, (INError error) => {
                MainThreadDispatcher.Enqueue(() => errback(error));
            });
        }

        public void Send<T>(INCollatedMessage<T> message, Action<T> callback, Action<INError> errback)
        {
            _client.Send<T>(message, (T result) => {
                MainThreadDispatcher.Enqueue(() => callback(result));
            }, (INError error) => {
                MainThreadDispatcher.Enqueue(() => errback(error));
            });
        }

        public void Send(INUncollatedMessage message, Action<bool> callback, Action<INError> errback)
        {
            _client.Send(message, (bool done) => {
                MainThreadDispatcher.Enqueue(() => callback(done));
            }, (INError error) => {
                MainThreadDispatcher.Enqueue(() => errback(error));
            });
        }
    }
}
