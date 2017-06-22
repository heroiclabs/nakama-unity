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
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Google.Protobuf;
using WebSocketSharp;

namespace Nakama
{
    public class NClient : INClient
    {
        public uint ConnectTimeout { get; private set; }

        public string Host { get; private set; }

        public string Lang { get; private set; }

        public INLogger Logger { get; private set; }

        public event EventHandler OnDisconnect;

        public event EventHandler<NErrorEventArgs> OnError;

        public event EventHandler<NMatchmakeMatchedEventArgs> OnMatchmakeMatched;

        public event EventHandler<NMatchDataEventArgs> OnMatchData;

        public event EventHandler<NMatchPresenceEventArgs> OnMatchPresence;

        public event EventHandler<NTopicMessageEventArgs> OnTopicMessage;

        public event EventHandler<NTopicPresenceEventArgs> OnTopicPresence;

        public uint Port { get; private set; }

        public string ServerKey { get; private set; }

        public long ServerTime
        {
            get {
                if (serverTime < 1)
                {
                    // Time has not been set via socket yet.
                    TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    return Convert.ToInt64(span.TotalMilliseconds);
                }
                return serverTime;
            }
            private set {
                // Dont let server time go backwards.
                if ((value - serverTime) > 0)
                {
                    serverTime = value;
                }
            }
        }

        public bool SSL { get; private set; }

        public uint Timeout { get; private set; }

        public bool NoDelay { get; private set; }

        public bool Trace { get; private set; }

        private IDictionary<string, KeyValuePair<Action<object>, Action<INError>>> collationIds =
                new Dictionary<string, KeyValuePair<Action<object>, Action<INError>>>();

        private long serverTime = 0;

        private INTransport transport;

        private NClient(string serverKey)
        {
            // Don't send Expect: 100 Continue when sending HTTP requests
            ServicePointManager.Expect100Continue = false;
            // Fix SSL certificate handshake
            ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;

            ConnectTimeout = 3000;
            Host = "127.0.0.1";
            Port = 7350;
            ServerKey = serverKey;
            SSL = false;
            Timeout = 5000;
            NoDelay = true;
            Trace = false;
            Lang = "en";
#if UNITY
            // NOTE Not compiled by default; avoids dependency on UnityEngine
            Logger = new NUnityLogger();
#else
            Logger = new NConsoleLogger();
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
            transport = new NTransportJavascript();
#else
            transport = new NTransport();
#endif

            transport.Logger = Logger;
            transport.Trace = Trace;
            transport.OnClose += (sender, _) =>
            {
                collationIds.Clear();
                OnDisconnect.Emit(this, EventArgs.Empty);
            };
            transport.OnMessage += (sender, m) =>
            {
                var message = Envelope.Parser.ParseFrom(m.Data);
                Logger.TraceFormatIf(Trace, "SocketDecoded: {0}", message);
                onMessage(message);
            };
            transport.OnError += (sender, m) =>
            {
                if (OnError != null)
                {
                    var message = (m.Error != null) ? m.Error.Message : "A transport error occured.";
                    OnError(this, new NErrorEventArgs(new NError(message)));
                }
            };
        }

        public void Register(INAuthenticateMessage message,
            Action<INSession> callback,
            Action<INError> errback)
        {
            authenticate("/user/register", message.Payload, Lang, callback, errback);
        }

        public void Login(INAuthenticateMessage message,
            Action<INSession> callback,
            Action<INError> errback)
        {
            authenticate("/user/login", message.Payload, Lang, callback, errback);
        }

        public void Connect(INSession session)
        {
            transport.Connect(getWebsocketUri(session), NoDelay);
        }

        public void Connect(INSession session, Action<bool> callback)
        {
            transport.ConnectAsync(getWebsocketUri(session), NoDelay, callback);
        }

        public static NClient Default(string serverKey)
        {
            return new NClient.Builder(serverKey).Build();
        }

        public void Disconnect()
        {
            transport.Close();
        }

        public void Disconnect(Action callback)
        {
            transport.CloseAsync(callback);
        }

        public void Logout()
        {
            var payload = new Envelope {Logout = new Logout()};
            var stream = new MemoryStream();
            payload.WriteTo(stream);
            transport.Send(stream.ToArray());
        }

        public void Logout(Action<bool> callback)
        {
            var payload = new Envelope {Logout = new Logout()};
            var stream = new MemoryStream();
            payload.WriteTo(stream);
            transport.SendAsync(stream.ToArray(), (bool completed) =>
            {
                callback(completed);
            });
        }

        public void Send<T>(INCollatedMessage<T> message, Action<T> callback, Action<INError> errback)
        {
            // Set a collation ID to dispatch callbacks on receive
            string collationId = Guid.NewGuid().ToString();
            message.SetCollationId(collationId);

            // Track callbacks for message
            var pair = new KeyValuePair<Action<object>, Action<INError>>((data) =>
            {
                callback((T)data);
            }, errback);
            collationIds.Add(collationId, pair);

            var stream = new MemoryStream();
            message.Payload.WriteTo(stream);
            Logger.TraceFormatIf(Trace, "SocketWrite: {0}", message.Payload);
            transport.SendAsync(stream.ToArray(), (bool completed) =>
            {
                if (!completed)
                {
                    // The write may have failed; don't track it
                    collationIds.Remove(collationId);
                }
            });
        }

        public void Send(INUncollatedMessage message, Action<bool> callback, Action<INError> errback)
        {
            var stream = new MemoryStream();
            message.Payload.WriteTo(stream);
            Logger.TraceFormatIf(Trace, "SocketWrite: {0}", message.Payload);
            transport.SendAsync(stream.ToArray(), (bool completed) =>
            {
                if (completed)
                {
                    callback(true);
                }
                else
                {
                    errback(new NError("Message send error"));
                }
            });
        }

        public override string ToString()
        {
            var f = "NClient(ConnectTimeout={0},Host={1},Lang={2},Port={3},ServerKey={4},SSL={5},Timeout={6},NoDelay={7},Trace={8})";
            return String.Format(f, ConnectTimeout, Host, Lang, Port, ServerKey, SSL, Timeout, NoDelay, Trace);
        }

        private void authenticate(string path,
                                  AuthenticateRequest payload,
                                  string langHeader,
                                  Action<INSession> callback,
                                  Action<INError> errback)
        {
            // Add a collation ID for logs
            payload.CollationId = Guid.NewGuid().ToString();

            var scheme = (SSL) ? "https" : "http";
            var uri = new UriBuilder(scheme, Host, unchecked((int)Port), path).Uri;
            Logger.TraceFormatIf(Trace, "Url={0}, Payload={1}", uri, payload);

            byte[] buffer = Encoding.UTF8.GetBytes(ServerKey + ":");
            var authHeader = String.Concat("Basic ", Convert.ToBase64String(buffer));

            TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            transport.Post(uri.ToString(), payload, authHeader, langHeader, Timeout, ConnectTimeout, (data) =>
            {
                AuthenticateResponse authResponse = AuthenticateResponse.Parser.ParseFrom(data);
                Logger.TraceFormatIf(Trace, "DecodedResponse={0}", authResponse);

                switch (authResponse.IdCase)
                {
                    case AuthenticateResponse.IdOneofCase.Session:
                        callback(new NSession(authResponse.Session.Token, System.Convert.ToInt64(span.TotalMilliseconds)));
                        break;
                    case AuthenticateResponse.IdOneofCase.Error:
                        errback(new NError(authResponse.Error));
                        break;
                    case AuthenticateResponse.IdOneofCase.None:
                        Logger.Error("Received invalid response from server");
                        break;
                    default:
                        Logger.Error("Received invalid response from server");
                        break;
                }
            }, (e) =>
            {
                errback(new NError(e.Message));
            });
        }

        private string getWebsocketUri(INSession session)
        {
            // Init base WebSocket connection
            var scheme = (SSL) ? "wss" : "ws";
            var bUri = new UriBuilder(scheme, Host, unchecked((int)Port), "api");
            bUri.Query = String.Format("token={0}&lang={1}", session.Token, Lang);
            return bUri.Uri.ToString();
        }

        private void onMessage(Envelope message)
        {
            // Handle realtime messages
            switch (message.PayloadCase)
            {
                case Envelope.PayloadOneofCase.Heartbeat:
                    ServerTime = message.Heartbeat.Timestamp;
                    return;
                case Envelope.PayloadOneofCase.MatchmakeMatched:
                    if (OnMatchmakeMatched != null)
                    {
                        OnMatchmakeMatched(this, new NMatchmakeMatchedEventArgs(new NMatchmakeMatched(message.MatchmakeMatched)));
                    }
                    return;
                case Envelope.PayloadOneofCase.MatchData:
                    if (OnMatchData != null)
                    {
                        OnMatchData(this, new NMatchDataEventArgs(new NMatchData(message.MatchData)));
                    }
                    return;
                case Envelope.PayloadOneofCase.MatchPresence:
                    if (OnMatchPresence != null)
                    {
                        OnMatchPresence(this, new NMatchPresenceEventArgs(new NMatchPresence(message.MatchPresence)));
                    }
                    return;
                case Envelope.PayloadOneofCase.TopicMessage:
                    if (OnTopicMessage != null)
                    {
                        OnTopicMessage(this, new NTopicMessageEventArgs(new NTopicMessage(message.TopicMessage)));
                    }
                    return;
                case Envelope.PayloadOneofCase.TopicPresence:
                    if (OnTopicPresence != null)
                    {
                        OnTopicPresence(this, new NTopicPresenceEventArgs(new NTopicPresence(message.TopicPresence)));
                    }
                    return;
            }

            var collationId = message.CollationId;
            var pair = collationIds[collationId];
            collationIds.Remove(collationId);

            switch (message.PayloadCase)
            {
                case Envelope.PayloadOneofCase.None:
                    pair.Key(true);
                    break;
                case Envelope.PayloadOneofCase.Error:
                    var error = new NError(message.Error);
                    if (collationId != null)
                    {
                        pair.Value(error);
                    }
                    else
                    {
                        if (OnError != null)
                        {
                            OnError(this, new NErrorEventArgs(error));
                        }
                    }
                    break;
                case Envelope.PayloadOneofCase.Friends:
                    var friends = new List<INFriend>();
                    foreach (var friend in message.Friends.Friends)
                    {
                        friends.Add(new NFriend(friend));
                    }
                    pair.Key(new NResultSet<INFriend>(friends, null));
                    break;
                case Envelope.PayloadOneofCase.Group:
                    pair.Key(new NGroup(message.Group.Group));
                    break;
                case Envelope.PayloadOneofCase.GroupUsers:
                    var groupUsers = new List<INGroupUser>();
                    foreach (var groupUser in message.GroupUsers.Users)
                    {
                        groupUsers.Add(new NGroupUser(groupUser));
                    }
                    pair.Key(new NResultSet<INGroupUser>(groupUsers, null));
                    break;
                case Envelope.PayloadOneofCase.Groups:
                    var groups = new List<INGroup>();
                    foreach (var group in message.Groups.Groups)
                    {
                        groups.Add(new NGroup(group));
                    }
                    pair.Key(new NResultSet<INGroup>(groups, new NCursor(message.Groups.Cursor.ToByteArray())));
                    break;
                case Envelope.PayloadOneofCase.MatchmakeTicket:
                    pair.Key(new NMatchmakeTicket(message.MatchmakeTicket));
                    break;
                case Envelope.PayloadOneofCase.Match:
                    pair.Key(new NMatch(message.Match));
                    break;
                case Envelope.PayloadOneofCase.Matches:
                    var matches = new List<INMatch>();
                    foreach (var m in message.Matches.Matches)
                    {
                        matches.Add(new NMatch(m));
                    }
                    pair.Key(new NResultSet<INMatch>(matches, null));
                    break;
                case Envelope.PayloadOneofCase.Self:
                    pair.Key(new NSelf(message.Self.Self));
                    break;
                case Envelope.PayloadOneofCase.StorageKeys:
                    var keys = new List<INStorageKey>();
                    foreach (var key in message.StorageKeys.Keys)
                    {
                        keys.Add(new NStorageKey(key));
                    }
                    pair.Key(new NResultSet<INStorageKey>(keys, null));
                    break;
                case Envelope.PayloadOneofCase.StorageData:
                    var storageData = new List<INStorageData>();
                    foreach (var data in message.StorageData.Data)
                    {
                        storageData.Add(new NStorageData(data));
                    }
                    pair.Key(new NResultSet<INStorageData>(storageData, null));
                    break;
                case Envelope.PayloadOneofCase.Topics:
                    var topics = new List<INTopic>();
                    foreach (var topic in message.Topics.Topics)
                    {
                        topics.Add(new NTopic(topic));
                    }
                    pair.Key(new NResultSet<INTopic>(topics, null));
                    break;
                case Envelope.PayloadOneofCase.TopicMessageAck:
                    pair.Key(new NTopicMessageAck(message.TopicMessageAck));
                    break;
                case Envelope.PayloadOneofCase.TopicMessages:
                    var topicMessages = new List<INTopicMessage>();
                    foreach (var topicMessage in message.TopicMessages.Messages)
                    {
                        topicMessages.Add(new NTopicMessage(topicMessage));
                    }
                    pair.Key(new NResultSet<INTopicMessage>(topicMessages,
                        new NCursor(message.TopicMessages.Cursor.ToByteArray())));
                    break;
                case Envelope.PayloadOneofCase.Users:
                    var users = new List<INUser>();
                    foreach (var user in message.Users.Users)
                    {
                        users.Add(new NUser(user));
                    }
                    pair.Key(new NResultSet<INUser>(users, null));
                    break;
                case Envelope.PayloadOneofCase.Leaderboards:
                    var leaderboards = new List<INLeaderboard>();
                    foreach (var leaderboard in message.Leaderboards.Leaderboards)
                    {
                        leaderboards.Add(new NLeaderboard(leaderboard));
                    }
                    pair.Key(new NResultSet<INLeaderboard>(leaderboards, new NCursor(message.Leaderboards.Cursor.ToByteArray())));
                    break;
                case Envelope.PayloadOneofCase.LeaderboardRecords:
                    var leaderboardRecords = new List<INLeaderboardRecord>();
                    foreach (var leaderboardRecord in message.LeaderboardRecords.Records)
                    {
                        leaderboardRecords.Add(new NLeaderboardRecord(leaderboardRecord));
                    }
                    var cursor = message.LeaderboardRecords.Cursor == null ? null : new NCursor(message.LeaderboardRecords.Cursor.ToByteArray());
                    pair.Key(new NResultSet<INLeaderboardRecord>(leaderboardRecords, cursor));
                    break;
                case Envelope.PayloadOneofCase.Rpc:
                    pair.Key(new NRuntimeRpc(message.Rpc));
                    break;
                default:
                    Logger.TraceFormatIf(Trace, "Unrecognized protocol message: {0}", message);
                    break;
            }
        }

        public class Builder
        {
            private NClient client;

            public Builder(string serverKey)
            {
                client = new NClient(serverKey);
            }

            public Builder ConnectTimeout(uint connectTimeout)
            {
                client.ConnectTimeout = connectTimeout;
                return this;
            }

            public Builder Host(string host)
            {
                client.Host = host;
                return this;
            }

            public Builder Lang(string lang)
            {
                client.Lang = lang;
                return this;
            }

            public Builder Logger(INLogger logger)
            {
                client.Logger = logger;
                client.transport.Logger = logger;
                return this;
            }

            public Builder Port(uint port)
            {
                client.Port = port;
                return this;
            }

            public Builder SSL(bool enable)
            {
                client.SSL = enable;
                return this;
            }

            public Builder Timeout(uint timeout)
            {
                client.Timeout = timeout;
                return this;
            }

            public Builder NoDelay(bool enable)
            {
                client.NoDelay = enable;
                return this;
            }

            public Builder Trace(bool enable)
            {
                client.Trace = enable;
                client.transport.Trace = enable;
                return this;
            }

            public NClient Build()
            {
                // Clone object so builder now operates on new copy.
                var original = client;
                client = new NClient(original.ServerKey);
                client.ConnectTimeout = original.ConnectTimeout;
                client.Host = original.Host;
                client.Lang = original.Lang;
                client.Logger = original.Logger;
                client.Port = original.Port;
                client.SSL = original.SSL;
                client.Timeout = original.Timeout;
                client.NoDelay = original.NoDelay;
                client.Trace = original.Trace;
                return original;
            }
        }
    }
}
