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
    public interface INClient
    {
        uint ConnectTimeout { get; }

        string Host { get; }

        string Lang { get; }

        INLogger Logger { get; }

        Action<INDisconnectEvent> OnDisconnect { get; set; }

        Action<INError> OnError { get; set; }

        Action<INMatchmakeMatched> OnMatchmakeMatched { get; set; }

        Action<INMatchData> OnMatchData { get; set; }

        Action<INMatchPresence> OnMatchPresence { get; set; }

        Action<INTopicMessage> OnTopicMessage { get; set; }

        Action<INTopicPresence> OnTopicPresence { get; set; }

        Action<INNotification> OnNotification { get; set; }

        uint Port { get; }

        string ServerKey { get; }

        long ServerTime { get; }

        bool SSL { get; }

        uint Timeout { get; }

        bool Trace { get; }

        void Connect(INSession session);

        void Connect(INSession session, Action<bool> callback);

        void Disconnect();

        void Disconnect(Action callback);

        void Login(INAuthenticateMessage message, Action<INSession> callback, Action<INError> errback);

        void Logout();

        void Logout(Action<bool> callback);

        void Register(INAuthenticateMessage message, Action<INSession> callback, Action<INError> errback);

        void Send<T>(INCollatedMessage<T> message, Action<T> callback, Action<INError> errback);

        void Send(INUncollatedMessage message, bool reliable, Action<bool> callback, Action<INError> errback);

        void Send(INUncollatedMessage message, Action<bool> callback, Action<INError> errback);
    }
}
