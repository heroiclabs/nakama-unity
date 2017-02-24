using System;
using System.IO;
using System.Net;
using System.Reflection;
using Google.Protobuf;
using WebSocketSharp;

namespace Nakama
{
    internal class NTransport : INTransport
    {
        public event EventHandler<WebSocketCloseEventArgs> OnClose;
        public event EventHandler<WebSocketErrorEventArgs> OnError;
        public event EventHandler<WebSocketMessageEventArgs> OnMessage;
        public event EventHandler OnOpen;

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
                OnClose.Emit(this, new WebSocketCloseEventArgs(evt.Code, evt.Reason));
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

                OnMessage.Emit(this, new WebSocketMessageEventArgs(evt.RawData));
            };
            socket.OnError += (sender, evt) =>
            {
                if (OnError != null)
                {
                    OnError.Emit(sender, new WebSocketErrorEventArgs(evt.Exception));
                }
            };
            socket.OnOpen += (sender, evt) =>
            {
                if (OnOpen != null)
                {
                    OnOpen.Emit(sender, evt);
                }
            };

        }

        public void Connect(string uri)
        {
            if (socket == null)
            {
                createWebSocket(uri);
            }
            socket.Connect();
        }

        public void ConnectAsync(string uri, Action<bool> callback)
        {
            if (socket == null)
            {
                createWebSocket(uri);
                socket.OnOpen += (sender, _) =>
                {
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

        public void Send(byte[] data)
        {
            if (socket != null)
            {
                socket.Send(data);
            }
        }

        public void SendAsync(byte[] data, Action<bool> completed)
        {
            if (socket != null)
            {
                socket.SendAsync(data, completed);
            }
        }
    }
}