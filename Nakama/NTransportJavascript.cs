#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Google.Protobuf;

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

        private static bool _trace;
        public bool Trace {
            get { return _trace; }
            set { _trace = value; }
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

        private static readonly IDictionary<string, Action<SocketCloseEventArgs>> SocketCloseHandlers = new Dictionary<string, Action<SocketCloseEventArgs>>();
        private static readonly IDictionary<string, Action<SocketErrorEventArgs>> SocketErrorHandlers = new Dictionary<string, Action<SocketErrorEventArgs>>();
        private static readonly IDictionary<string, Action<SocketMessageEventArgs>> SocketMessageHandlers = new Dictionary<string, Action<SocketMessageEventArgs>>();
        private static readonly IDictionary<string, Action> SocketOpenHandlers = new Dictionary<string, Action>();

        private static readonly IDictionary<string, Action> SocketCloseCallbacks = new Dictionary<string, Action>();
        private static readonly IDictionary<string, Action<bool>> SocketOpenCallbacks = new Dictionary<string, Action<bool>>();

        private readonly string _socketId = Guid.NewGuid().ToString();

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
            Action<string, string> socketClose
        );

        [DllImport("__Internal")]
        private static extern int FetchPost(string handlerId, string uri, string payload, string authHeader, string langHeader);

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        public static void AuthSuccessCallback(string handlerId, string data)
        {
            _logger.TraceFormatIf(_trace, "WebGL auth-success callback");
            var kv = AuthHandlers[handlerId];
            AuthHandlers.Remove(handlerId);
            kv.Key(Convert.FromBase64String(data));
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void AuthErrorCallback(string handlerId)
        {
            _logger.TraceFormatIf(_trace, "WebGL auth-error callback");
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
        private static extern int CreateSocket(string socketId, string uri);
        [DllImport("__Internal")]
        private static extern void CloseSocket(int socketRef);
        [DllImport("__Internal")]
        private static extern void SendData(int socketRef, string data);
        [DllImport("__Internal")]
        private static extern int SocketState(int socketRef);

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnSocketOpen(string socketId)
        {
            _logger.TraceFormatIf(_trace, "WebGL onsocketopen callback - socket: {0}", socketId);

            if (SocketOpenHandlers.ContainsKey(socketId))
            {
                SocketOpenHandlers[socketId]();
            }

            if (SocketOpenCallbacks.ContainsKey(socketId))
            {
                SocketOpenCallbacks[socketId](true);
            }
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnSocketError(string socketId)
        {
            _logger.TraceFormatIf(_trace, "WebGL onsocketerror callback - socket: {0}", socketId);

            if (SocketErrorHandlers.ContainsKey(socketId))
            {
                SocketErrorHandlers[socketId](new SocketErrorEventArgs(new Exception("WebSocket error occured")));
            }
        }

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        public static void OnSocketMessage(string socketId, string data)
        {
            _logger.TraceFormatIf(_trace, "WebGL onsocketmessage callback - socket: {0}", socketId);

            if (SocketMessageHandlers.ContainsKey(socketId))
            {
                var dataBytes = Convert.FromBase64String(data);
                SocketMessageHandlers[socketId](new SocketMessageEventArgs(dataBytes));
            }
        }

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        public static void OnSocketClose(string socketId, string closeStatus)
        {
            _logger.TraceFormatIf(_trace, "WebGL onsocketclose callback - socket {0} - status: {1}", socketId, closeStatus);
            var closeStatusCode = Convert.ToInt32(closeStatus);

            if (SocketCloseHandlers.ContainsKey(socketId))
            {
                SocketCloseHandlers[socketId](new SocketCloseEventArgs(closeStatusCode, CloseErrorMessages[closeStatusCode]));
            }

            if (SocketCloseCallbacks.ContainsKey(socketId))
            {
                SocketCloseCallbacks[socketId]();
            }

            SocketCloseHandlers.Remove(socketId);
            SocketCloseCallbacks.Remove(socketId);
        }

        public void Connect(string uri, string token)
        {
            // This is not a blocking call

            // connection happen on socket creation
            if (_socketNativeRef == -1)
            {
                _socketNativeRef = CreateSocket(_socketId, uri);
            }
        }

        public void ConnectAsync(string uri, string token, Action<bool> callback)
        {
            // connection happen on socket creation
            if (_socketNativeRef == -1)
            {
                SocketOpenCallbacks.Add(_socketId, callback);
                _socketNativeRef = CreateSocket(_socketId, uri);
            }
        }

        public void Close()
        {
            CloseSocket(_socketNativeRef);
        }

        public void CloseAsync(Action callback)
        {
            SocketCloseCallbacks.Add(_socketId, callback);
            CloseSocket(_socketNativeRef);
        }

        public void Send(byte[] data, bool reliable)
        {
            var base64Payload = Convert.ToBase64String(data);
            SendData(_socketNativeRef, base64Payload);
        }

        public void SendAsync(byte[] data, bool reliable, Action<bool> callback)
        {
            var base64Payload = Convert.ToBase64String(data);
            SendData(_socketNativeRef, base64Payload);
            callback(true);
        }
        
        public void SetOnClose(Action<SocketCloseEventArgs> OnClose)
        {
            SocketCloseHandlers.Add(_socketId, OnClose);
        }

        public void SetOnError(Action<SocketErrorEventArgs> OnError)
        {
            SocketErrorHandlers.Add(_socketId, OnError);
        }

        public void SetOnMessage(Action<SocketMessageEventArgs> OnMessage)
        {
            SocketMessageHandlers.Add(_socketId, OnMessage);
        }

        public void SetOnOpen(Action OnOpen)
        {
            SocketOpenHandlers.Add(_socketId, OnOpen);
        }
    }
}
#endif