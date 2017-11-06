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
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Google.Protobuf;
using NetcodeIO.NET;
using ReliableNetcode;

namespace Nakama
{
    internal class NTransportUdp : INTransport
    {
        public Action<SocketCloseEventArgs> OnClose;
        public Action<SocketErrorEventArgs> OnError;
        public Action<SocketMessageEventArgs> OnMessage;
        public Action OnOpen;

        public bool Trace { get; set; }
        public INLogger Logger { get; set; }

        private int tickrate = 30;
        
        private Client client;
        private ReliableEndpoint endpoint;
        private ManualResetEvent connectedEvt;
        private bool isConnected = false;

        public void Post(string uri,
            AuthenticateRequest payload,
            string authHeader,
            string langHeader,
            uint timeout,
            uint connectTimeout,
            Action<byte[]> successAction,
            Action<Exception> errorAction)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/octet-stream;";
            request.Accept = "application/octet-stream;";

            // Add Headers
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            request.UserAgent = String.Format("nakama-unitysdk/{0}", version);
            request.Headers.Add(HttpRequestHeader.Authorization, authHeader);
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, langHeader);

            // Optimise request
            request.Timeout = unchecked((int)connectTimeout);
            request.ReadWriteTimeout = unchecked((int)timeout);
            request.KeepAlive = true;
            request.Proxy = null;

            // FIXME(novabyte) Does HttpWebRequest ignore timeouts in async mode?
            dispatchRequestAsync(request, payload, (response) =>
            {
                if (Trace)
                {
                    Logger.TraceFormat("RawHttpResponse={0}", customToString(response));
                }
                var stream = response.GetResponseStream();
                var data = convertStream(stream);
                stream.Close();
                successAction(data);
                response.Close();
            }, errorAction);
        }

        private static void dispatchRequestAsync(WebRequest request,
            AuthenticateRequest payload,
            Action<HttpWebResponse> successAction,
            Action<Exception> errorAction)
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

        private static byte[] convertStream (Stream stream)
        {
            var buffer = new byte[4 * 1024];
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    var read = stream.Read (buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        return ms.ToArray();
                    }
                    ms.Write (buffer, 0, read);
                }
            }
        }

        private void createClient()
        {
            client = new Client();
            endpoint = new ReliableEndpoint();
            connectedEvt = new ManualResetEvent(false);
            client.Tickrate = tickrate;

            client.OnStateChanged += (ClientState state) =>
            {
                switch (state)
                {
                        case ClientState.ConnectTokenExpired:
                            if (OnError != null)
                            {
                                OnError(new SocketErrorEventArgs(new Exception("connect token expired")));
                            }
                            break;
                        case ClientState.InvalidConnectToken:
                            if (OnError != null)
                            {
                                OnError(new SocketErrorEventArgs(new Exception("invalid connect token")));
                            }
                            break;
                        case ClientState.ConnectionTimedOut:
                            isConnected = false;
                            if (OnError != null)
                            {
                                OnError(new SocketErrorEventArgs(new Exception("connection timed out")));
                            }
                            break;
                        case ClientState.ChallengeResponseTimedOut:
                            if (OnError != null)
                            {
                                OnError(new SocketErrorEventArgs(new Exception("connection response timed out")));
                            }
                            break;
                        case ClientState.ConnectionRequestTimedOut:
                            if (OnError != null)
                            {
                                OnError(new SocketErrorEventArgs(new Exception("connection request timed out")));
                            }
                            break;
                        case ClientState.ConnectionDenied:
                            if (OnError != null)
                            {
                                OnError(new SocketErrorEventArgs(new Exception("connection denied")));
                            }
                            break;
                        case ClientState.Disconnected:
                            isConnected = false;
                            if (OnClose != null)
                            {
                                OnClose(new SocketCloseEventArgs(1, "disconnected"));
                            }
                            break;
                        case ClientState.SendingConnectionRequest:
                            break;
                        case ClientState.SendingChallengeResponse:
                            break;
                        case ClientState.Connected:
                            isConnected = true;
                            connectedEvt.Set();
                            if (OnOpen != null)
                            {
                                OnOpen();
                            }
                            break;
                        default:
                            Logger.Warn(String.Format("Unknown client state in change: {0}", state));
                            break;
                }
            };

            client.OnMessageReceived += (payload, size) =>
            {
                endpoint.ReceivePacket(payload, size);
            };
            
            endpoint.TransmitCallback = (payload, size) =>
            {
                client.Send(payload, size);
            };
            endpoint.ReceiveCallback = (payload, size) =>
            {
                var payloadCopy = new byte[size];
                Array.Copy(payload, 0, payloadCopy, 0, size);
                if (OnMessage != null)
                {
                    OnMessage(new SocketMessageEventArgs(payloadCopy));
                }
            };
        }

        public void Connect(string uri, string token)
        {
            if (client == null)
            {
                createClient();
                client.Connect(System.Convert.FromBase64String(token));
            }
            connectedEvt.WaitOne(10000);
            if (client.State != ClientState.Connected)
            {
                if (OnError != null)
                {
                    OnError(new SocketErrorEventArgs(new Exception("client timed out while connecting")));
                }
                isConnected = false;
                client = null;
                endpoint = null;
                connectedEvt = null;
                return;
            }
            ThreadPool.QueueUserWorkItem(tick);
        }

        public void ConnectAsync(string uri, string token, Action<bool> callback)
        {
            if (client == null)
            {
                createClient();
                client.OnStateChanged += (ClientState state) =>
                {
                    if (state == ClientState.Connected)
                    {
                        callback(true);
                        ThreadPool.QueueUserWorkItem(tick);
                    }
                    else if (state != ClientState.SendingConnectionRequest &&
                             state != ClientState.SendingChallengeResponse)
                    {
                        if (OnError != null)
                        {
                            OnError(new SocketErrorEventArgs(new Exception("client timed out while connecting")));
                        }
                        isConnected = false;
                        client = null;
                        endpoint = null;
                        connectedEvt = null;
                    }
                };
                client.Connect(System.Convert.FromBase64String(token));
            }
        }

        public void Close()
        {
            if (client != null)
            {
                isConnected = false;
                client.Disconnect();
                client = null;
                endpoint = null;
                connectedEvt = null;
            }
        }

        public void CloseAsync(Action callback)
        {
            if (client != null)
            {
                isConnected = false;
                client.Disconnect();
                client = null;
                endpoint = null;
                connectedEvt = null;
                callback();
            }
        }

        public void Send(byte[] data, bool reliable)
        {
            if (endpoint != null)
            {
                endpoint.SendMessage(data, data.Length, reliable ? QosType.Reliable : QosType.Unreliable);
            }
            else
            {
                Logger.Warn("Send: Failed to send message. Client not connected.");
            }
        }

        public void SendAsync(byte[] data, bool reliable, Action<bool> completed)
        {
            if (endpoint != null)
            {
                endpoint.SendMessage(data, data.Length, reliable ? QosType.Reliable : QosType.Unreliable);
                completed(true);
            }
            else
            {
                Logger.Warn("SendAsync: Failed to send message. Client not connected.");
                completed(false);
            }
        }
        
        public void SetOnClose(Action<SocketCloseEventArgs> OnClose)
        {
            this.OnClose = OnClose;
        }

        public void SetOnError(Action<SocketErrorEventArgs> OnError)
        {
            this.OnError = OnError;
        }

        public void SetOnMessage(Action<SocketMessageEventArgs> OnMessage)
        {
            this.OnMessage = OnMessage;
        }

        public void SetOnOpen(Action OnOpen)
        {
            this.OnOpen = OnOpen;
        }

        private void tick(Object stateInfo)
        {
            while (isConnected)
            {
                endpoint.Update();

                // sleep until next tick
                double tickLength = 1.0 / tickrate;
                Thread.Sleep((int)(tickLength * 1000));
            }
        }
    }
}