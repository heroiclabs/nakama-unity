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

        void Connect(string uri, byte[] token);
        void ConnectAsync(string uri, byte[] token, Action<bool> callback);
        void Close();
        void CloseAsync(Action callback);
        void Send(byte[] data, bool reliable);
        void SendAsync(byte[] data, bool reliable, Action<bool> completed);

        event EventHandler<SocketCloseEventArgs> OnClose;
        event EventHandler<SocketErrorEventArgs> OnError;
        event EventHandler<SocketMessageEventArgs> OnMessage;
        event EventHandler OnOpen;
    }

    public class SocketMessageEventArgs : EventArgs
    {
        public byte[] Data { get ; private set; }

        internal SocketMessageEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    public class SocketCloseEventArgs : EventArgs
    {
        public int Code { get; private set; }
        public string Reason{ get; private set; }

        internal SocketCloseEventArgs(int code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }

    public class SocketErrorEventArgs : EventArgs
    {
        public Exception Error { get ; private set; }

        internal SocketErrorEventArgs(Exception error)
        {
            Error = error;
        }
    }

}