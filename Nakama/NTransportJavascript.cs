using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Google.Protobuf;

namespace Nakama
{
    public class NTransportJavascript : INTransport
    {
        public bool Trace { get; set; }
        public INLogger Logger { get; set; }
        public static INLogger sl;

        int socketNativeRef = -1;

        private static IDictionary<string, KeyValuePair<Action<byte[]>, Action<Exception>>> authHandlers = new Dictionary<string, KeyValuePair<Action<byte[]>, Action<Exception>>>();

        public event EventHandler<WebSocketCloseEventArgs> OnClose;
        public event EventHandler<WebSocketErrorEventArgs> OnError;
        public event EventHandler<WebSocketMessageEventArgs> OnMessage;
        public event EventHandler OnOpen;

        public NTransportJavascript()
        {
            InitTransport(AuthSuccessCallback, AuthErrorCallback);
        }

        [DllImport("__Internal")]
        public static extern void InitTransport(Action<string, string> successCallback, Action<string> failureCallback);

        public void Post(string uri, AuthenticateRequest payload, string authHeader, string langHeader, uint timeout,
            uint connectTimeout, Action<byte[]> successAction, Action<Exception> errorAction)
        {
            var handlerId = Guid.NewGuid().ToString();
            authHandlers.Add(handlerId, new KeyValuePair<Action<byte[]>, Action<Exception>>(successAction, errorAction));

            var base64Payload = Convert.ToBase64String(payload.ToByteArray());

            FetchPost(handlerId, uri, base64Payload, authHeader, langHeader);
        }

        [DllImport("__Internal")]
        private static extern int FetchPost(string handlerId, string uri, string payload, string authHeader, string langHeader);

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        public static void AuthSuccessCallback(string handlerId, string data)
        {
            sl.Info("OnAuthSuccess");
            var kv = authHandlers[handlerId];
            authHandlers.Remove(handlerId);
            kv.Key(Convert.FromBase64String(data));
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void AuthErrorCallback(string handlerId)
        {
            var kv = authHandlers[handlerId];
            authHandlers.Remove(handlerId);
            kv.Value(new Exception("Network request failed."));
        }

        public void Connect(string uri)
        {
            if (socketNativeRef == -1)
            {
                CreateSocket(uri);
            }
        }

        public void ConnectAsync(string uri, Action<bool> callback)
        {
            if (socketNativeRef == -1)
            {
                CreateSocket(uri);
            }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void CloseAsync(Action callback)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SendAsync(byte[] data, Action<bool> completed)
        {
            throw new NotImplementedException();
        }

        [DllImport("__Internal")]
        private static extern int CreateSocket(string uri);

        public void OnSocketOpen()
        {
            Logger.Info("OnSocketOpen");
        }

        public static void OnAuthenticateSuccess(byte[] data)
        {
            sl.Info("OnAuthSuccess");
        }

        public static void OnAuthenticateError(byte[] data)
        {
            sl.Info("OnAuthenticateError");
        }
    }
}