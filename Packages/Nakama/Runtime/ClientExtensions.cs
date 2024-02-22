// Copyright 2019 The Nakama Authors
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

using UnityEngine;

namespace Nakama
{
    /// <summary>
    /// A set of client extensions to help with conditional Unity engine code.
    /// </summary>
    public static class ClientExtensions
    {
        /// <summary>
        /// Build a new socket with conditional compilation on the adapter.
        /// </summary>
        /// <param name="client">The client object.</param>
        /// <param name="useMainThread">Whether or not socket events should be dispatched on Unity's main thread.</param>
        /// <returns>A new socket.</returns>
        public static ISocket NewSocket(this IClient client, bool useMainThread = false, ISocketAdapter defaultAdapter = null)
        {
            ISocketAdapter threadedAdapter;
#if UNITY_WEBGL && !UNITY_EDITOR
            threadedAdapter = new JsWebSocketAdapter();
#else
            threadedAdapter = defaultAdapter ?? new WebSocketStdlibAdapter();
#endif

            var adapter = useMainThread ? UnitySocket.Create(threadedAdapter) : threadedAdapter;
            var socket = Socket.From(client, adapter);
#if UNITY_EDITOR
            socket.ReceivedError += Debug.LogError;
#endif
            return socket;
        }
    }
}
