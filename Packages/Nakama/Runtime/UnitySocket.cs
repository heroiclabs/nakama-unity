// Copyright 2021 The Nakama Authors
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama
{
    /// <summary>
    /// A WebSocket adapter for Unity that dispatches events to the main thread.
    /// </summary>
    public sealed class UnitySocket : MonoBehaviour, ISocketAdapter
    {
        event Action ISocketAdapter.Connected
        {
            add => _connectedHandlers.Add(value);

            remove => _connectedHandlers.Remove(value);
        }

        event Action ISocketAdapter.Closed
        {
            add => _closedHandlers.Add(value);

            remove => _closedHandlers.Remove(value);
        }

        event Action<Exception> ISocketAdapter.ReceivedError
        {
            add => _errorHandlers.Add(value);

            remove => _errorHandlers.Remove(value);
        }

        event Action<ArraySegment<byte>> ISocketAdapter.Received
        {
            add => _receivedHandlers.Add(value);

            remove => _receivedHandlers.Remove(value);
        }

        bool ISocketAdapter.IsConnected => _socketAdapter.IsConnected;
        bool ISocketAdapter.IsConnecting => _socketAdapter.IsConnecting;

        private readonly ConcurrentQueue<QueuedEvent> _eventQueue = new ConcurrentQueue<QueuedEvent>();
        private readonly List<Action> _connectedHandlers = new List<Action>();
        private readonly List<Action> _closedHandlers = new List<Action>();
        private readonly List<Action<Exception>> _errorHandlers = new List<Action<Exception>>();
        private readonly List<Action<ArraySegment<byte>>> _receivedHandlers = new List<Action<ArraySegment<byte>>>();

        private ISocketAdapter _socketAdapter;

        public static UnitySocket Create(ISocketAdapter adapter)
        {
            var adapterGO = new GameObject("[Nakama Socket]");
            DontDestroyOnLoad(adapterGO);
            var unityAdapter = adapterGO.AddComponent<UnitySocket>();
            unityAdapter._socketAdapter = adapter;
            unityAdapter._socketAdapter.Closed += unityAdapter.OnClosed;
            unityAdapter._socketAdapter.Connected += unityAdapter.OnConnected;
            unityAdapter._socketAdapter.Received += unityAdapter.OnReceived;
            unityAdapter._socketAdapter.ReceivedError += unityAdapter.OnReceivedError;
            return unityAdapter;
        }

        Task ISocketAdapter.CloseAsync() => _socketAdapter.CloseAsync();

        Task ISocketAdapter.ConnectAsync(Uri uri, int timeout) => _socketAdapter.ConnectAsync(uri, timeout);

        Task ISocketAdapter.SendAsync(ArraySegment<byte> buffer, bool reliable, CancellationToken canceller) =>
            _socketAdapter.SendAsync(buffer, reliable, canceller);

        private void OnClosed() => _eventQueue.Enqueue(new QueuedEvent(_closedHandlers));

        private void OnConnected() => _eventQueue.Enqueue(new QueuedEvent(_connectedHandlers));

        private void OnReceivedError(Exception obj) => _eventQueue.Enqueue(new QueuedEvent(_errorHandlers, obj));

        private void OnReceived(ArraySegment<byte> obj)
        {
            // copy the segment into a new segment with a new backing array
            // this avoids threading issues with the socket and unity main thread
            // accessing the same range within the same backing array.

            var copy = new byte[obj.Count];
            var j = 0;
            for (var i = obj.Offset; i < obj.Offset + obj.Count; i++)
            {
                copy[j] = obj.Array[i];
                j++;
            }

            _eventQueue.Enqueue(new QueuedEvent(_receivedHandlers, new ArraySegment<byte>(copy)));
        }

        private void Update()
        {
            while (_eventQueue.Count > 0)
            {
                QueuedEvent evt;

                if (_eventQueue.TryDequeue(out evt))
                {
                    evt.Dispatch();
                }
            }
        }

        private class QueuedEvent
        {
            private readonly IEnumerable<Delegate> _listeners;
            private readonly object[] _eventArgs;

            public QueuedEvent(IEnumerable<Delegate> listeners, params object[] eventArgs)
            {
                _listeners = listeners;
                _eventArgs = eventArgs;
            }

            public void Dispatch()
            {
                var listenersCopy = new List<Delegate>(_listeners);
                foreach (var listener in listenersCopy)
                {
                    listener.DynamicInvoke(_eventArgs);
                }
            }
        }
    }
}