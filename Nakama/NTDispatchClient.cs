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
    public class NTDispatchClient : INClient
    {
        // NOTE: Not compiled by default; avoids dependency on UnityEngine.

        public uint ConnectTimeout { get { return _client.ConnectTimeout; } }

        public string Host { get { return _client.Host; } }

        public string Lang { get { return _client.Lang; } }

        public INLogger Logger { get { return _client.Logger; } }

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

        private IDictionary<Delegate, EventHandler<NErrorEventArgs>> _onErrorMap;
        private EventHandler<NErrorEventArgs> _onError;

        public event EventHandler<NErrorEventArgs> OnError; /*
        {
            add {
                EventHandler<NErrorEventArgs> handler = delegate(object sender, NErrorEventArgs args) {
                    //value(sender, args);
                };
                lock (this)
                {
                    _onErrorMap.Add(value, handler);
                    _onError += handler;
                }
            }
            remove {
                EventHandler<NErrorEventArgs> handler;
                _onErrorMap.TryGetValue(value, out handler);
                lock (this)
                {
                    _onError -= handler;
                }
            }
        }*/

        //private EventHandler<NMatchmakeMatchedEventArgs> _onMatchmakeMatched;

        public event EventHandler<NMatchmakeMatchedEventArgs> OnMatchmakeMatched; /*{
            add {
                var handler = (object source, NMatchmakeMatchedEventArgs args) => {
                    Enqueue(() => value(source, args));
                };
                _onMatchmakeMatched = (EventHandler<NMatchmakeMatchedEventArgs>) Delegate.Combine(_onMatchmakeMatched, handler);
            }
            remove {
                // FIXME does it work?
                _onMatchmakeMatched = (EventHandler<NMatchmakeMatchedEventArgs>) Delegate.Remove(_onMatchmakeMatched, value);
            }
        }*/

        //private EventHandler<NMatchDataEventArgs> _onMatchData;

        public event EventHandler<NMatchDataEventArgs> OnMatchData; /*{
            add {
                var handler = (object source, NMatchDataEventArgs args) => {
                    Enqueue(() => value(source, args));
                };
                _onMatchData = (EventHandler<NMatchDataEventArgs>) Delegate.Combine(_onMatchData, handler);
            }
            remove {
                // FIXME does it work?
                _onMatchData = (EventHandler<NMatchDataEventArgs>) Delegate.Remove(_onMatchData, value);
            }
        }*/

        //private EventHandler<NMatchPresenceEventArgs> _onMatchPresence;

        public event EventHandler<NMatchPresenceEventArgs> OnMatchPresence; /*{
            add {
                var handler = (object source, NMatchPresenceEventArgs args) => {
                    Enqueue(() => value(source, args));
                };
                _onMatchPresence = (EventHandler<NMatchPresenceEventArgs>) Delegate.Combine(_onMatchPresence, handler);
            }
            remove {
                // FIXME does it work?
                _onMatchPresence = (EventHandler<NMatchPresenceEventArgs>) Delegate.Remove(_onMatchPresence, value);
            }
        }*/

        //private EventHandler<NTopicMessageEventArgs> _onTopicMessage;

        public event EventHandler<NTopicMessageEventArgs> OnTopicMessage; /*{
            add {
                var handler = (object source, NTopicMessageEventArgs args) => {
                    Enqueue(() => value(source, args));
                };
                _onTopicMessage = (EventHandler<NTopicMessageEventArgs>) Delegate.Combine(_onTopicMessage, handler);
            }
            remove {
                // FIXME does it work?
                _onTopicMessage = (EventHandler<NTopicMessageEventArgs>) Delegate.Remove(_onTopicMessage, value);
            }
        }*/

        //private EventHandler<NTopicPresenceEventArgs> _onTopicPresence;

        public event EventHandler<NTopicPresenceEventArgs> OnTopicPresence; /*{
            add {
                var handler = (object source, NTopicPresenceEventArgs args) => {
                    Enqueue(() => value(source, args));
                };
                _onTopicPresence = (EventHandler<NTopicPresenceEventArgs>) Delegate.Combine(_onTopicPresence, handler);
            }
            remove {
                // FIXME does it work?
                _onTopicPresence = (EventHandler<NTopicPresenceEventArgs>) Delegate.Remove(_onTopicPresence, value);
            }
        }*/

        public uint Port { get { return _client.Port; } }

        public string ServerKey { get { return _client.ServerKey; } }

        public long ServerTime { get { return _client.ServerTime; } }

        public bool SSL { get { return _client.SSL; } }

        public uint Timeout { get { return _client.Timeout; } }

        public bool Trace { get { return _client.Trace; } }

        private INClient _client;

        public NTDispatchClient(NClient client)
        {
            _client = client;
            _onDisconnectMap = new Dictionary<Delegate, EventHandler>();
            _onErrorMap = new Dictionary<Delegate, EventHandler<NErrorEventArgs>>();
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
