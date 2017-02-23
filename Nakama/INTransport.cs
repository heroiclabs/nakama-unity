using System;

namespace Nakama
{
    internal interface INTransport
    {
        bool Trace { get; set;  }
        INLogger Logger { get; set; }

        void Post(string uri,
            AuthenticateRequest payload,
            string authHeader,
            string langHeader,
            uint timeout,
            uint connectTimeout,
            Action<byte[]> successAction,
            Action<Exception> errorAction);

        void Connect(string uri);
        void ConnectAsync(string uri, Action<bool> callback);
        void Close();
        void CloseAsync(Action callback);
        void Send(byte[] data);
        void SendAsync(byte[] data, Action<bool> completed);

        event EventHandler<WebSocketCloseEventArgs> OnClose;
        event EventHandler<WebSocketErrorEventArgs> OnError;
        event EventHandler<WebSocketMessageEventArgs> OnMessage;
        event EventHandler OnOpen;
    }

    public class WebSocketMessageEventArgs : EventArgs
    {
        public byte[] Data { get ; private set; }

        internal WebSocketMessageEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    public class WebSocketCloseEventArgs : EventArgs
    {
        public int Code { get; private set; }
        public string Reason{ get; private set; }

        internal WebSocketCloseEventArgs(int code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }

    public class WebSocketErrorEventArgs : EventArgs
    {
        public Exception Error { get ; private set; }

        internal WebSocketErrorEventArgs(Exception error)
        {
            Error = error;
        }
    }

}