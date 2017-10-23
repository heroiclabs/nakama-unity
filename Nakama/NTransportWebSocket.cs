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
using System.Net.WebSockets;
using System.Text;

namespace Nakama
{
    internal class NTransportWebSocket : INTransport
    {
        public event EventHandler<SocketCloseEventArgs> OnClose;
        public event EventHandler<SocketErrorEventArgs> OnError;
        public event EventHandler<SocketMessageEventArgs> OnMessage;
        public event EventHandler OnOpen;

        public bool Trace { get; set; }
        public INLogger Logger { get; set; }
        
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private ClientWebSocket client;

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
            client = new ClientWebSocket();
            client.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);
        }
        
        private async void listenClient()
        {
            var buffer = new byte[ReceiveChunkSize];
            try
            {
                while (client.State == WebSocketState.Open)
                {
                    byte[] msg = null;
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            if (OnClose != null)
                            {
                                OnClose(this, new SocketCloseEventArgs(1, "connection closed"));
                            }
                            return;
                        }
                        else
                        {
                            if (msg == null)
                            {
                                msg = new byte[result.Count];
                                Array.Copy(buffer, 0, msg, 0, result.Count);
                            }
                            else
                            {
                                byte[] newMsg = new byte[msg.Length + result.Count];
                                Array.Copy(msg, 0, newMsg, 0, msg.Length);
                                Array.Copy(buffer, 0, newMsg, msg.Length - 1, result.Count);
                                msg = newMsg;
                            }
                        }
                    } while (!result.EndOfMessage);
                    
                    if (OnMessage != null)
                    {
                        OnMessage(this, new SocketMessageEventArgs(msg));
                    }
                }
            }
            catch (Exception e)
            {
                if (OnClose != null)
                {
                    OnClose(this, new SocketCloseEventArgs(1, e.Message));                    
                }
                client = null;
            }
            finally
            {
                client.Dispose();
                client = null;
            }
        }

        public void Connect(string uri, byte[] token)
        {
            ConnectAsync(uri, token, b => {});
        }

        public async void ConnectAsync(string uri, byte[] token, Action<bool> callback)
        {
            if (client == null)
            {
                createClient();
            }
            await client.ConnectAsync(new Uri(uri), CancellationToken.None);
            // TODO enable NoDelay on underlying socket.
            listenClient();
            callback(true);
        }

        public void Close()
        {
            CloseAsync(() => {});
        }

        public async void CloseAsync(Action callback)
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            callback();
        }

        public void Send(byte[] data, bool reliable)
        {
            SendAsync(data, reliable, b => {});
        }

        public async void SendAsync(byte[] data, bool reliable, Action<bool> completed)
        {
            if (client == null || client.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            var messagesCount = (int)Math.Ceiling((double)data.Length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SendChunkSize * i);
                var count = SendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > data.Length)
                {
                    count = data.Length - offset;
                }

                await client.SendAsync(new ArraySegment<byte>(data, offset, count), WebSocketMessageType.Binary, lastMessage, CancellationToken.None);
            }
            completed(true);
        }
    }
}
