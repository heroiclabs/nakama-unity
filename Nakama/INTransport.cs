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

namespace Nakama
{
    public enum TransportType : int
    {
        // WebSocket transport default.
        WebSocket = 0,
        // UDP transport.
        Udp = 1
    }
    
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

        void Connect(string uri, string token);
        void ConnectAsync(string uri, string token, Action<bool> callback);
        void Close();
        void CloseAsync(Action callback);
        void Send(byte[] data, bool reliable);
        void SendAsync(byte[] data, bool reliable, Action<bool> completed);

        void SetOnClose(Action<SocketCloseEventArgs> OnClose);
        void SetOnError(Action<SocketErrorEventArgs> OnError);
        void SetOnMessage(Action<SocketMessageEventArgs> OnMessage);
        void SetOnOpen(Action OnOpen);
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