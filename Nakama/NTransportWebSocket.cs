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
using System.Net.Sockets;
using System.Reflection;
using Google.Protobuf;
using WebSocketSharp;

namespace Nakama
{
    internal class NTransportWebSocket : INTransport
    {
        public Action<SocketCloseEventArgs> OnClose;
        public Action<SocketErrorEventArgs> OnError;
        public Action<SocketMessageEventArgs> OnMessage;
        public Action OnOpen;

        public bool Trace { get; set; }
        public INLogger Logger { get; set; }

        private WebSocket socket;

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

        private void createWebSocket(string uri)
        {
            socket = new WebSocket(uri);
            if (Trace)
            {
                socket.Log.Level = LogLevel.Debug;
            }

            socket.OnClose += (sender, evt) =>
            {
                // Release socket handle
                socket = null;
                Logger.TraceIf(Trace, String.Format("Socket Closed. Code={0}, Reason={1}", evt.Code, evt.Reason));
                if (OnClose != null)
                {
                    OnClose(new SocketCloseEventArgs(evt.Code, evt.Reason));
                }
            };
            socket.OnMessage += (sender, evt) =>
            {
                if (evt.IsPing)
                {
                    Logger.TraceIf(Trace, "SocketReceive: WebSocket ping.");
                    return;
                }

                if (evt.IsText)
                {
                    Logger.TraceIf(Trace, "SocketReceive: Invalid content (text/plain).");
                    return;
                }

                if (OnMessage != null)
                {
                    OnMessage(new SocketMessageEventArgs(evt.RawData));
                }
            };
            socket.OnError += (sender, evt) =>
            {
                if (OnError != null)
                {
                    OnError(new SocketErrorEventArgs(evt.Exception));
                }
            };
            socket.OnOpen += (sender, evt) =>
            {
                if (OnOpen != null)
                {
                    OnOpen();
                }
            };

        }

        public void Connect(string uri, string token)
        {
            if (socket == null)
            {
                createWebSocket(uri);
            }
            socket.Connect();

            // Experimental. Get a reference to the underlying socket and enable TCP_NODELAY.
            Logger.TraceIf(Trace, "Connect: Enabling NoDelay on socket.");
            socket.TcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            Logger.TraceIf(Trace, "Connect: Enabled NoDelay on socket.");
        }

        public void ConnectAsync(string uri, string token, Action<bool> callback)
        {
            if (socket == null)
            {
                createWebSocket(uri);
                socket.OnOpen += (sender, _) =>
                {
                    // Experimental. Get a reference to the underlying socket and enable TCP_NODELAY.
                    Logger.TraceIf(Trace, "ConnectAsync: Enabling NoDelay on socket.");
                    socket.TcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                    Logger.TraceIf(Trace, "ConnectAsync: Enabled NoDelay on socket.");

                    callback(true);
                };
            }

            socket.ConnectAsync();
        }

        public void Close()
        {
            if (socket != null)
            {
                socket.Close(CloseStatusCode.Normal);
            }
        }

        public void CloseAsync(Action callback)
        {
            if (socket != null)
            {
                socket.CloseAsync(CloseStatusCode.Normal);
            }
            callback();
        }

        public void Send(byte[] data, bool reliable)
        {
            if (socket != null)
            {
                socket.Send(data);
            }
            else
            {
                Logger.Warn("Send: Failed to send message. Client not connected.");
            }
        }

        public void SendAsync(byte[] data, bool reliable, Action<bool> completed)
        {
            if (socket != null)
            {
                socket.SendAsync(data, completed);
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
    }
}
