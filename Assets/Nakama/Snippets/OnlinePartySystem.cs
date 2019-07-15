/**
 * Copyright 2019 The Nakama Authors
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Nakama.TinyJson;
using UnityEngine;

namespace Nakama
{
    /// <summary>
    /// An experimental service which simulates the <c>IOnlinePartyService</c> from Unreal Engine.
    /// </summary>
    public interface IOnlinePartyService
    {
        event Action<string, string, string, string, string> OnPartyJoinRequestReceived;

        bool ApproveJoinRequest(string localUserId, string partyId, string recipientId, bool isApproved, int code);

        bool CreateParty(string localUserId, int partyType, string config, Action<string, string, int> callback);

        bool JoinParty(string localUserId, string onlinePartyJoinInfo, JoinPartyComplete joinPartyComplete);

        bool LeaveParty(string localUserId, string partyId, Action<string, string, int> leavePartyComplete);
    }

    public delegate void JoinPartyComplete(string netId, string partyId, int code, int otherCode);

    public sealed class OnlinePartyService : IOnlinePartyService
    {
        public event Action<string, string, string, string, string> OnPartyJoinRequestReceived;

        private readonly IClient _client;
        private readonly ISession _session;
        private readonly ISocket _socket;

        private readonly ConcurrentDictionary<string, JoinPartyComplete> _joinPartyCallbacks;

        public OnlinePartyService(IClient client, ISocket socket, ISession session)
        {
            _client = client;
            _socket = socket;
            _session = session;
            _joinPartyCallbacks = new ConcurrentDictionary<string, JoinPartyComplete>();

            _socket.Closed += () => { _joinPartyCallbacks.Clear(); };
            _socket.ReceivedNotification += notification =>
            {
                switch (notification.Code)
                {
                    case 100:
                        var decoded = notification.Content.FromJson<Dictionary<string, object>>();
                        var partyId = decoded["party_id"].ToString();
                        var code = int.Parse(decoded["join_party_completion_result"].ToString());
                        _joinPartyCallbacks[partyId].Invoke(null, partyId, code, 0);
                        _joinPartyCallbacks.TryRemove(partyId, out var joinPartyComplete);
                        break;
                    default:
                        Debug.LogErrorFormat("unrecognised notification code: {0}", notification.Code);
                        break;
                }
            };
            _socket.ReceivedStreamState += state =>
            {
                // Assume all inputs are join requests right now.
                var decoded = state.State.FromJson<Dictionary<string, object>>();
                OnPartyJoinRequestReceived?.Invoke(null, decoded["party_id"].ToString(),
                    decoded["sender_id"].ToString(), null, null);
            };
        }

        public bool ApproveJoinRequest(string localUserId, string partyId, string recipientId,
            bool isApproved, int code)
        {
            if (!_socket.IsConnected)
            {
                return false;
            }

            var message = new Dictionary<string, object>
            {
                {"party_id", partyId}, {"recipient_id", recipientId}, {"is_approved", isApproved},
                {"denied_result_code", code}
            };
            _socket.RpcAsync("OnlinePartySystem-ApproveJoinRequest", message.ToJson());
            return true;
        }

        // TODO config should be a configuration object.
        public bool CreateParty(string localUserId, int partyType, string config, Action<string, string, int> callback)
        {
            if (!_socket.IsConnected)
            {
                // Socket was not open so "task could not be started".
                return false;
            }

            var message = new Dictionary<string, object>
                {{"party_type_id", partyType}, {"party_configuration", config}};
            _socket.RpcAsync("OnlinePartySystem-CreateParty", message.ToJson()).ContinueWith(t =>
            {
                // TODO Use enum for completion result error code.
                if (t.IsCompleted)
                {
                    var response = t.Result.Payload.FromJson<Dictionary<string, object>>();
                    var completionResult = int.Parse(response["create_party_completion_result"].ToString());
                    callback.Invoke(null, response["party_id"].ToString(), completionResult);
                }
                else
                {
                    callback.Invoke(null, null, -100);
                }
            });
            return true;
        }

        public bool JoinParty(string localUserId, string onlinePartyJoinInfo, JoinPartyComplete joinPartyComplete)
        {
            if (!_socket.IsConnected)
            {
                // The operation could not be triggered because the socket was not connected.
                return false;
            }

            _joinPartyCallbacks[onlinePartyJoinInfo] = joinPartyComplete;
            var message = new Dictionary<string, object> {{"party_id", onlinePartyJoinInfo}};
            _socket.RpcAsync("OnlinePartySystem-JoinParty", message.ToJson()).ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    if (string.IsNullOrEmpty(t.Result.Payload)) return;

                    var response = t.Result.Payload.FromJson<Dictionary<string, object>>();
                    var partyId = response["party_id"].ToString();
                    var completionResult = int.Parse(response["join_party_completion_result"].ToString());
                    joinPartyComplete.Invoke(null, partyId, completionResult, 0);
                    _joinPartyCallbacks.TryRemove(partyId, out joinPartyComplete);
                }
                else
                {
                    joinPartyComplete.Invoke(null, null, -100, 0);
                    _joinPartyCallbacks.TryRemove(onlinePartyJoinInfo, out joinPartyComplete);
                }
            });
            return true;
        }

        public bool LeaveParty(string localUserId, string partyId, Action<string, string, int> leavePartyComplete)
        {
            if (!_socket.IsConnected)
            {
                return false;
            }

            var message = new Dictionary<string, object> {{"party_id", partyId}};
            _socket.RpcAsync("OnlinePartySystem-LeaveParty", message.ToJson()).ContinueWith(t =>
            {
                if (!t.IsCompleted) return;

                var response = t.Result.Payload.FromJson<Dictionary<string, object>>();
                var completionResult = int.Parse(response["leave_party_completion_result"].ToString());
                leavePartyComplete.Invoke(null, response["party_id"].ToString(), completionResult);
            });
            return true;
        }
    }
}
