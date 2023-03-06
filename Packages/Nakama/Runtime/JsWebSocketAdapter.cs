// Copyright 2020 The Nakama Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_WEBGL && !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama
{
    /// <summary>
    /// An adapter which uses the WebSocket protocol with Nakama server on a JavaScript bridge with Unity engine.
    /// </summary>
    public class JsWebSocketAdapter : ISocketAdapter
    {
        /// <inheritdoc cref="ISocketAdapter.Connected"/>
        public event Action Connected;

        /// <inheritdoc cref="ISocketAdapter.Closed"/>
        public event Action Closed;

        /// <inheritdoc cref="ISocketAdapter.ReceivedError"/>
        public event Action<Exception> ReceivedError;

        /// <inheritdoc cref="ISocketAdapter.Received"/>
        public event Action<ArraySegment<byte>> Received;

        /// <summary>
        /// If the WebSocket is connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// If the WebSocket is connecting.
        /// </summary>
        public bool IsConnecting { get; private set; }

        private int Ref { get; set; } // unique jslib socket ref
        private Uri _uri;

        public JsWebSocketAdapter()
        {
            Ref = -1;
        }

        /// <inheritdoc cref="ISocketAdapter.CloseAsync"/>
        public Task CloseAsync()
        {
            UnityWebGLSocketBridge.Instance.Close(Ref);
            Ref = -1;
            IsConnecting = false;
            IsConnected = false;
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="ISocketAdapter.ConnectAsync"/>
        public Task ConnectAsync(Uri uri, int timeout)
        {
            // TODO will need to use window.setTimeout to implement timeouts on DOM WebSocket.
            if (Ref > -1)
            {
                return Task.CompletedTask;
            }

            _uri = uri;
            IsConnecting = true;
            var tcs = new TaskCompletionSource<bool>();

            Action open = () =>
            {
                IsConnected = true;
                IsConnecting = false;
                tcs.TrySetResult(true);
                Connected?.Invoke();
            };
            Action<int, string> close = (code, reason) =>
            {
                IsConnected = false;
                IsConnecting = false;
                Ref = -1;
                Closed?.Invoke();
            };
            Action<string> error = reason =>
            {
                IsConnected = false;
                Ref = -1;
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetException(new Exception(reason));
                }
                else
                {
                    ReceivedError?.Invoke(new Exception(reason));
                }
            };
            Action<string> handler = message =>
            {
                Received?.Invoke(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)));
            };
            Ref = UnityWebGLSocketBridge.Instance.CreateSocket(uri.AbsoluteUri, open, close, error, handler);
            return tcs.Task;
        }

        /// <inheritdoc cref="ISocketAdapter.SendAsync"/>
        public Task SendAsync(ArraySegment<byte> buffer, bool reliable = true, CancellationToken canceller = default)
        {
            if (Ref == -1)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }

            var payload = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            UnityWebGLSocketBridge.Instance.Send(Ref, payload);
            return Task.CompletedTask;
        }

        public override string ToString() =>
            $"JsWebSocketAdapter(IsConnected={IsConnected}, IsConnecting={IsConnecting}, Uri='{_uri}')";
    }

    // ReSharper disable once InconsistentNaming
    public class UnityWebGLSocketBridge : MonoBehaviour
    {
        private static readonly IDictionary<int, string> CloseErrorMessages = new Dictionary<int, string>
        {
            { 1000, "Normal" },
            { 1001, "Away" },
            { 1002, "ProtocolError" },
            { 1003, "UnsupportedData" },
            { 1004, "Undefined" },
            { 1005, "NoStatus" },
            { 1006, "Abnormal" },
            { 1007, "InvalidData" },
            { 1008, "PolicyViolation" },
            { 1009, "TooBig" },
            { 1010, "MandatoryExtension" },
            { 1011, "ServerError" },
            { 1015, "TlsHandshakeFailure" }
        };

        private static int _globalSocketRef;

        private readonly Dictionary<int, UnityWebGLSocketBridgeHandler> _handlers =
            new Dictionary<int, UnityWebGLSocketBridgeHandler>();

        private static readonly object Lock = new object();
        private static UnityWebGLSocketBridge _instance;

        public static UnityWebGLSocketBridge Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance != null) return _instance;

                    var go = GameObject.Find("/[Nakama]");
                    if (go == null)
                    {
                        go = new GameObject("[Nakama]");
                    }

                    if (go.GetComponent<UnityWebGLSocketBridge>() == null)
                    {
                        go.AddComponent<UnityWebGLSocketBridge>();
                    }

                    DontDestroyOnLoad(go);
                    _instance = go.GetComponent<UnityWebGLSocketBridge>();
                    return _instance;
                }
            }
        }

        private UnityWebGLSocketBridge()
        {
        }

        public int CreateSocket(string address, Action socketOpenHandle, Action<int, string> socketCloseHandle,
            Action<string> socketErrorHandle, Action<string> socketMessageHandle)
        {
            var handler = new UnityWebGLSocketBridgeHandler
            {
                OnOpen = socketOpenHandle,
                OnClose = socketCloseHandle,
                OnMessage = socketMessageHandle,
                OnError = socketErrorHandle
            };

            var socketRef = _globalSocketRef++;
            _handlers.Add(socketRef, handler);
            NKCreateSocket(socketRef, address);
            return socketRef;
        }

        public void Close(int socketRef)
        {
            NKCloseSocket(socketRef);
        }

        public void Send(int socketRef, string payload)
        {
            NKSendData(socketRef, payload);
        }

        private UnityWebGLSocketBridgeHandler GetHandler(int socketRef)
        {
            UnityWebGLSocketBridgeHandler handler;
            _handlers.TryGetValue(socketRef, out handler);
            return handler;
        }

        [DllImport("__Internal")]
        private static extern void NKCreateSocket(int socketRef, string address);

        [DllImport("__Internal")]
        private static extern void NKCloseSocket(int socketRef);

        [DllImport("__Internal")]
        private static extern void NKSendData(int socketRef, string data);

        [DllImport("__Internal")]
        private static extern int NKSocketState();

        // called by jslib
        private void NKBridgeOnOpen(string bridgeMsg)
        {
            var index = bridgeMsg.IndexOf('_');
            if (index < 0)
            {
                return;
            }

            var socketRef = Convert.ToInt32(bridgeMsg.Substring(0, index));
            GetHandler(socketRef)?.OnOpen?.Invoke();
        }

        // called by jslib
        private void NKBridgeOnMessage(string bridgeMsg)
        {
            var index = bridgeMsg.IndexOf('_');
            if (index < 0 || index + 1 >= bridgeMsg.Length)
            {
                return;
            }

            var socketRef = Convert.ToInt32(bridgeMsg.Substring(0, index));
            var msg = bridgeMsg.Substring(index + 1);
            GetHandler(socketRef)?.OnMessage?.Invoke(msg);
        }

        // called by jslib
        private void NKBridgeOnClose(string bridgeMsg)
        {
            var index = bridgeMsg.IndexOf('_');
            if (index < 0 || index + 1 >= bridgeMsg.Length)
            {
                return;
            }

            var socketRef = Convert.ToInt32(bridgeMsg.Substring(0, index));
            var code = Convert.ToInt32(bridgeMsg.Substring(index + 1));
            GetHandler(socketRef)?.OnClose?.Invoke(code, CloseErrorMessages[code]);
            _handlers.Remove(socketRef);
        }

        // called by jslib
        private void NKBridgeOnError(string bridgeMsg)
        {
            var index = bridgeMsg.IndexOf('_');
            if (index >= 0 && index + 1 < bridgeMsg.Length)
            {
                var socketRef = Convert.ToInt32(bridgeMsg.Substring(0, index));
                var msg = bridgeMsg.Substring(index + 1);

                GetHandler(socketRef)?.OnError?.Invoke(msg);
                _handlers.Remove(socketRef);
            }
        }

        // ReSharper disable once InconsistentNaming
        private class UnityWebGLSocketBridgeHandler
        {
            public Action OnOpen;
            public Action<int, string> OnClose;
            public Action<string> OnError;
            public Action<string> OnMessage;
        }
    }
}

#endif
