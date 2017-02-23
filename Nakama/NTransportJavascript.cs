using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Google.Protobuf;
using WebSocketSharp;

namespace Nakama
{
    public class NTransportJavascript : INTransport
    {
        private static INLogger _logger;
        public INLogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        private static readonly IDictionary<int, string> CloseErrorMessages = new Dictionary<int, string> {
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

        private static readonly IDictionary<string, KeyValuePair<Action<byte[]>, Action<Exception>>> AuthHandlers = new Dictionary<string, KeyValuePair<Action<byte[]>, Action<Exception>>>();
        private static readonly IDictionary<string, EventHandler<WebSocketCloseEventArgs>> SocketCloseHandlers = new Dictionary<string, EventHandler<WebSocketCloseEventArgs>>();
        private static readonly IDictionary<string, EventHandler<WebSocketErrorEventArgs>> SocketErrorHandlers = new Dictionary<string, EventHandler<WebSocketErrorEventArgs>>();
        private static readonly IDictionary<string, EventHandler<WebSocketMessageEventArgs>> SocketMessageHandlers = new Dictionary<string, EventHandler<WebSocketMessageEventArgs>>();
        private static readonly IDictionary<string, EventHandler> SocketOpenHandlers = new Dictionary<string, EventHandler>();

        private static readonly IDictionary<string, Action> SocketCloseCallbacks = new Dictionary<string, Action>();
        private static readonly IDictionary<string, Action<bool>> SocketOpenCallbacks = new Dictionary<string, Action<bool>>();

        private readonly string _socketId = Guid.NewGuid().ToString();
        public bool Trace { get; set; }
        public event EventHandler<WebSocketCloseEventArgs> OnClose {
            add { SocketCloseHandlers.Add(_socketId, value); }
            remove { SocketCloseHandlers.Remove(_socketId); }
        }
        public event EventHandler<WebSocketErrorEventArgs> OnError {
            add { SocketErrorHandlers.Add(_socketId, value); }
            remove { SocketErrorHandlers.Remove(_socketId); }
        }
        public event EventHandler<WebSocketMessageEventArgs> OnMessage {
            add { SocketMessageHandlers.Add(_socketId, value); }
            remove { SocketMessageHandlers.Remove(_socketId); }
        }
        public event EventHandler OnOpen {
            add { SocketOpenHandlers.Add(_socketId, value); }
            remove { SocketOpenHandlers.Remove(_socketId); }
        }

        private int _socketNativeRef = -1;

        public NTransportJavascript()
        {
            InitTransport(AuthSuccessCallback, AuthErrorCallback, OnSocketOpen, OnSocketError, OnSocketMessage, OnSocketClose);
        }

        [DllImport("__Internal")]
        public static extern void InitTransport(Action<string, string> successCallback,
            Action<string> failureCallback,
            Action<string> socketOpen,
            Action<string> socketError,
            Action<string, string> socketMessage,
            Action<string, int> socketClose
        );

        [DllImport("__Internal")]
        private static extern int FetchPost(string handlerId, string uri, string payload, string authHeader, string langHeader);

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        public static void AuthSuccessCallback(string handlerId, string data)
        {
            _logger.Debug("WebGL auth-success callback");
            var kv = AuthHandlers[handlerId];
            AuthHandlers.Remove(handlerId);
            kv.Key(Convert.FromBase64String(data));
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void AuthErrorCallback(string handlerId)
        {
            _logger.Debug("WebGL auth-error callback");
            var kv = AuthHandlers[handlerId];
            AuthHandlers.Remove(handlerId);
            kv.Value(new Exception("Network request failed."));
        }

        public void Post(string uri, AuthenticateRequest payload, string authHeader, string langHeader, uint timeout,
            uint connectTimeout, Action<byte[]> successAction, Action<Exception> errorAction)
        {
            var handlerId = Guid.NewGuid().ToString();
            AuthHandlers.Add(handlerId, new KeyValuePair<Action<byte[]>, Action<Exception>>(successAction, errorAction));

            var base64Payload = Convert.ToBase64String(payload.ToByteArray());

            FetchPost(handlerId, uri, base64Payload, authHeader, langHeader);
        }

        // ----

        [DllImport("__Internal")]
        private static extern int CreateSocket(string uri);
        [DllImport("__Internal")]
        private static extern void ConnectSocket(string socketId);
        [DllImport("__Internal")]
        private static extern void CloseSocket(string socketId);
        [DllImport("__Internal")]
        private static extern void SendData(string socketId, string data);

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnSocketOpen(string socketId)
        {
            _logger.DebugFormat("WebGL onsocketopen callback - socket: {0}", socketId);
            SocketOpenHandlers[socketId].Emit(null, EventArgs.Empty);
            var callback = SocketOpenCallbacks[socketId];
            if (callback != null)
            {
                callback(true);
            }
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnSocketError(string socketId)
        {
            _logger.DebugFormat("WebGL onsocketerror callback - socket: {0}", socketId);
            SocketErrorHandlers[socketId].Emit(null, new WebSocketErrorEventArgs(new Exception("WebSocket error occured")));
        }

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        public static void OnSocketMessage(string socketId, string data)
        {
            _logger.DebugFormat("WebGL onsocketmessage callback - socket: {0}", socketId);
            SocketMessageHandlers[socketId].Emit(null, new WebSocketMessageEventArgs(Convert.FromBase64String(data)));
        }

        [MonoPInvokeCallback(typeof(Action<string, int>))]
        public static void OnSocketClose(string socketId, int closeStatus)
        {
            _logger.DebugFormat("WebGL onsocketclose callback - socket {0} - status: {1}", socketId, closeStatus);
            SocketCloseHandlers[socketId].Emit(null, new WebSocketCloseEventArgs(closeStatus, CloseErrorMessages[closeStatus]));
            var callback = SocketCloseCallbacks[socketId];
            if (callback != null)
            {
                callback();
            }
        }

        public void Connect(string uri)
        {
            if (_socketNativeRef == -1)
            {
                _socketNativeRef = CreateSocket(uri);
            }
            ConnectSocket(_socketId);
        }

        public void ConnectAsync(string uri, Action<bool> callback)
        {
            if (_socketNativeRef == -1)
            {
                _socketNativeRef = CreateSocket(uri);
            }
            SocketOpenCallbacks.Add(_socketId, callback);
            ConnectSocket(_socketId);
        }

        public void Close()
        {
            CloseSocket(_socketId);
        }

        public void CloseAsync(Action callback)
        {
            SocketCloseCallbacks.Add(_socketId, callback);
            CloseSocket(_socketId);
        }

        public void Send(byte[] data)
        {
            var base64Payload = Convert.ToBase64String(data);
            SendData(_socketId, base64Payload);
        }

        public void SendAsync(byte[] data, Action<bool> callback)
        {
            var base64Payload = Convert.ToBase64String(data);
            SendData(_socketId, base64Payload);
            callback(true);
        }
    }
}