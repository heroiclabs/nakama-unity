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

    internal class WebSocketMessageEventArgs : EventArgs
    {
        public byte[] Data { get ; private set; }

        internal WebSocketMessageEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    internal class WebSocketCloseEventArgs : EventArgs
    {
        public ushort Code { get; private set; }
        public string Reason{ get; private set; }

        internal WebSocketCloseEventArgs(ushort code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }

    internal class WebSocketErrorEventArgs : EventArgs
    {
        public Exception Error { get ; private set; }

        internal WebSocketErrorEventArgs(Exception error)
        {
            Error = error;
        }
    }

    public enum WebSocketCloseStatusCode : ushort
    {
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015,
    }

}