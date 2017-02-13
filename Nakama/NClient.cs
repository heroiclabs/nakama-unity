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

        public bool Trace { get; private set; }

        private IDictionary<string, KeyValuePair<Action<object>, Action<INError>>> collationIds =
                new Dictionary<string, KeyValuePair<Action<object>, Action<INError>>>();

        private long serverTime = 0;

        private WebSocket socket;

        private NClient(string serverKey)
        {
            ConnectTimeout = 3000;
            Host = "127.0.0.1";
            Lang = "en";
#if UNITY
            // NOTE Not compiled by default; avoids dependency on UnityEngine
            Logger = new NUnityLogger();
#else
            Logger = new NConsoleLogger();
#endif
            Port = 7350;
            ServerKey = serverKey;
            SSL = false;
            Timeout = 5000;
            Trace = false;
        }

        public void Connect(INSession session)
        {
            if (socket == null)
            {
                socket = createSocket(session);
            }
            socket.Connect();
        }

        public void Connect(INSession session, Action<bool> callback)
        {
            if (socket == null)
            {
                socket = createSocket(session);
                socket.OnOpen += (sender, _) =>
                {
                    callback(true);
                };
            }
            socket.ConnectAsync();
        }

        public static NClient Default(string serverKey)
        {
            return new NClient.Builder(serverKey).Build();
        }

        public void Disconnect()
        {
            if (socket != null)
            {
                socket.Close(CloseStatusCode.Normal);
            }
        }

        public void Disconnect(Action callback, Action<INError> errback)
        {
            if (socket != null)
            {
                socket.CloseAsync(CloseStatusCode.Normal);
            }
            callback();
        }

        public void Login(INAuthenticateMessage message,
                          Action<INSession> callback,
                          Action<INError> errback)
        {
            authenticate("/user/login", message.Payload, Lang, callback, errback);
        }

        public void Logout()
        {
            if (socket != null)
            {
                var payload = new Envelope {Logout = new Logout()};
                var stream = new MemoryStream();
                payload.WriteTo(stream);
                socket.Send(stream.ToArray());
            }
        }

        public void Logout(Action<bool> callback)
        {
            if (socket != null)
            {
                var payload = new Envelope {Logout = new Logout()};
                var stream = new MemoryStream();
                payload.WriteTo(stream);
                socket.SendAsync(stream.ToArray(), (bool completed) =>
                {
                    callback(completed);
                });
            }
        }

        public void Register(INAuthenticateMessage message,
                             Action<INSession> callback,
                             Action<INError> errback)
        {
            authenticate("/user/register", message.Payload, Lang, callback, errback);
        }

        public void Send<T>(INMessage<T> message, Action<T> callback, Action<INError> errback)
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
            socket.SendAsync(stream.ToArray(), (bool completed) =>
            {
                if (!completed)
                {
                    // The write may have failed; don't track it
                    collationIds.Remove(collationId);
                }
            });
        }

        public override string ToString()
        {
            var f = "NClient(ConnectTimeout={0},Host={1},Lang={2},Port={3},ServerKey={4},SSL={5},Timeout={6},Trace={7})";
            return String.Format(f, ConnectTimeout, Host, Lang, Port, ServerKey, SSL, Timeout, Trace);
        }

        private void authenticate(string path,
                                  AuthenticateRequest payload,
                                  string lang,
                                  Action<INSession> callback,
                                  Action<INError> errback)
        {
            var scheme = (SSL) ? "https" : "http";
            var uri = new UriBuilder(scheme, Host, unchecked((int)Port), path).Uri;
            Logger.TraceFormatIf(Trace, "Url={0}, Payload={1}", uri, payload);

            // Add a collation ID for logs
            payload.CollationId = Guid.NewGuid().ToString();

            // Init base HTTP request
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/octet-stream;";
            request.Accept = "application/octet-stream;";

            // Add Headers
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            request.UserAgent = String.Format("nakama-unitysdk/{0}", version);
            byte[] buffer = Encoding.UTF8.GetBytes(ServerKey + ":");
            var header = String.Concat("Basic ", Convert.ToBase64String(buffer));
            request.Headers.Add(HttpRequestHeader.Authorization, header);
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, lang);

            // Optimise request
            request.Timeout = unchecked((int)ConnectTimeout);
            request.ReadWriteTimeout = unchecked((int)Timeout);
            request.KeepAlive = true;
            request.Proxy = null;

            TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // FIXME(novabyte) Does HttpWebRequest ignore timeouts in async mode?
            dispatchRequestAsync(request, payload, (response) =>
            {
                if (Trace)
                {
                    Logger.TraceFormat("RawHttpResponse={0}", customToString(response));
                }
                var stream = response.GetResponseStream();
                AuthenticateResponse authResponse = AuthenticateResponse.Parser.ParseFrom(stream);
                stream.Close();
                Logger.TraceFormatIf(Trace, "DecodedResponse={0}", authResponse);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    callback(new NSession(authResponse.Session.Token, System.Convert.ToInt64(span.TotalMilliseconds)));
                }
                else
                {
                    errback(new NError(authResponse.Error));
                }
                response.Close();
            }, (e) =>
            {
                errback(new NError(e.Message));
            });
        }

        private WebSocket createSocket(INSession session)
        {
            // Init base WebSocket connection
            var scheme = (SSL) ? "wss" : "ws";
            var bUri = new UriBuilder(scheme, Host, unchecked((int)Port), "api");
            bUri.Query = String.Format("serverkey={0}&token={1}&lang={2}", ServerKey, session.Token, Lang);
            WebSocket socket = new WebSocket(bUri.Uri.ToString());

            if (Trace)
            {
                // TODO(novabyte) Do we log too much information?
                // TODO(novabyte) How to redirect log output for Unity?
                socket.Log.Level = LogLevel.Debug;
            }

            socket.OnClose += (sender, _) =>
            {
                collationIds.Clear();
                // Release socket handle
                socket = null;
                Logger.TraceIf(Trace, "Socket Closed");
                OnDisconnect.Emit(this, EventArgs.Empty);
            };
            socket.OnMessage += (sender, evt) =>
            {
                if (evt.IsPing)
                {
                    Logger.TraceIf(Trace, "SocketReceive: WebSocket ping.");
                    return;
                }
                else if (evt.IsText)
                {
                    Logger.TraceIf(Trace, "SocketReceive: Invalid content (text/plain).");
                    return;
                }
                var message = Envelope.Parser.ParseFrom(evt.RawData);
                Logger.TraceFormatIf(Trace, "SocketDecoded: {0}", message);
                onMessage(message);
            };
            return socket;
        }

        private void onMessage(Envelope message)
        {
            // Handle realtime messages
            switch (message.PayloadCase)
            {
                case Envelope.PayloadOneofCase.Heartbeat:
                    ServerTime = message.Heartbeat.Timestamp;
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
                    var error = new NError(message.Error.Reason);
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
                case Envelope.PayloadOneofCase.Self:
                    pair.Key(new NSelf(message.Self.Self));
                    break;
                case Envelope.PayloadOneofCase.StorageKey:
                    var keys = new List<INStorageKey>();
                    foreach (var key in message.StorageKey.Keys)
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
                case Envelope.PayloadOneofCase.Topic:
                    pair.Key(new NTopic(message.Topic));
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
                    pair.Key(new NResultSet<INTopicMessage>(topicMessages, new NCursor(message.TopicMessages.Cursor.ToByteArray())));
                    break;
                case Envelope.PayloadOneofCase.Users:
                    var users = new List<INUser>();
                    foreach (var user in message.Users.Users)
                    {
                        users.Add(new NUser(user));
                    }
                    pair.Key(new NResultSet<INUser>(users, null));
                    break;
                default:
                    Logger.TraceFormatIf(Trace, "Unrecognized message: {0}", message);
                    break;
            }
        }

        private static void dispatchRequestAsync(WebRequest request,
                                                 AuthenticateRequest payload,
                                                 Action<HttpWebResponse> successAction,
                                                 Action<WebException> errorAction)
        {
            // Wrap HttpWebRequest dispatch to avoid sync connection setup
            Action dispatchAction = () =>
            {
                try
                {
                    // Pack payload
                    var memStream = new MemoryStream();
                    payload.WriteTo(memStream);
                    var data = memStream.ToArray();
                    request.ContentLength = data.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(data, 0, data.Length);
                    dataStream.Close();
                }
                catch (WebException e)
                {
                    // Handle ConnectFailure socket errors
                    errorAction(e);
                    return;
                }

                request.BeginGetResponse((iar) =>
                {
                    try
                    {
                        var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
                        successAction(response);
                    }
                    catch (WebException e)
                    {
                        if (e.Response is HttpWebResponse)
                        {
                            successAction(e.Response as HttpWebResponse);
                            return;
                        }
                        errorAction(e);
                    }
                }, request);
            };
            dispatchAction.BeginInvoke((iar) =>
            {
                var action = (Action)iar.AsyncState;
                action.EndInvoke(iar);
            }, dispatchAction);
        }

        private static string customToString(HttpWebResponse response)
        {
            var f = "{{ \"uri\": \"{0}\", \"method\": \"{1}\", \"status\": {{ \"code\": {2}, \"description\": \"{3}\" }} }}";
            return String.Format(f, response.ResponseUri, response.Method, (int)response.StatusCode, response.StatusDescription);
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

            public Builder Trace(bool enable)
            {
                client.Trace = enable;
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
                client.Trace = original.Trace;
                return original;
            }
        }
    }
}
