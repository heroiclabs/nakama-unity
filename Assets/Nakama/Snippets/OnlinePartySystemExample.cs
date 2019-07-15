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
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama.Snippets
{
    public class OnlinePartyServiceExample : MonoBehaviour
    {
        private ISocket _socket1;
        private ISocket _socket2;

        private async void Awake()
        {
            var client = new Client("http", "127.0.0.1", 7350, "defaultkey");

            // Authenticate two different users (with generated ids).
            var session1 = await client.AuthenticateCustomAsync(Guid.NewGuid().ToString());
            _socket1 = SetupSocket(client, session1);
            await _socket1.ConnectAsync(session1);

            var session2 = await client.AuthenticateCustomAsync(Guid.NewGuid().ToString());
            _socket2 = SetupSocket(client, session2);
            await _socket2.ConnectAsync(session2);

            var partyService1 = new OnlinePartyService(client, _socket1, session1);
            partyService1.OnPartyJoinRequestReceived += (netId1, partyId, netId2, string1, onlinePartyData) =>
            {
                Debug.LogFormat("OnPartyJoinRequestReceived netId1: {0}, partyId: {1}, netId2: {2}, string1: {3}, onlinePartyData: {4}", netId1, partyId, netId2, string1, onlinePartyData);

                const bool isApproved = true;
                partyService1.ApproveJoinRequest(session1.UserId, partyId, netId2, isApproved, 0);
            };
            partyService1.CreateParty(session1.UserId, 11, "", (netId, partyId, code) =>
            {
                Debug.LogFormat("CreateParty netId: {0}, partyId: {1}, code: {2}", netId, partyId, code);

                var partyService2 = new OnlinePartyService(client, _socket2, session2);
                // Second player will join party.
                partyService2.JoinParty(session2.UserId, partyId, async (netId2, partyId2, code2, otherCode) =>
                {
                    Debug.LogFormat("JoinParty netId: {0}, partyId: {1}, code: {2}, othercode: {3}", netId2, partyId2, code2, otherCode);

                    await Task.Delay(TimeSpan.FromSeconds(5));
                    Debug.Log("delay complete");

                    partyService2.LeaveParty(session2.UserId, partyId, (netId3, partyId3, code3) =>
                    {
                        Debug.LogFormat("LeaveParty netId: {0}, partyId: {1}, code: {2}", netId3, partyId3, code3);
                    });
                });
            });
        }

        private static ISocket SetupSocket(IClient client, ISession session)
        {
            var socket = client.NewSocket();
            socket.Connected += () => Debug.LogFormat("userid: '{0}', socket connected.", session.UserId);
            socket.Closed += () => Debug.LogFormat("userid: '{0}', socket closed.", session.UserId);
            socket.ReceivedError += Debug.LogError;
            return socket;
        }

        private void OnApplicationQuit()
        {
            _socket1?.CloseAsync();
            _socket2?.CloseAsync();
        }
    }
}
