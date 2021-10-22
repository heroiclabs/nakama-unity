/**
* Copyright 2021 The Nakama Authors
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
using UnityEngine;
using Nakama;
using NakamaSync;
using System.Threading.Tasks;

namespace Pong
{
    public class Game : MonoBehaviour
    {
        private readonly VarRegistry _registry = new VarRegistry();
        private SyncMatch _match;
        private ISocket _socket;

        [SerializeField] private SelfPaddle _leftPaddle;
        [SerializeField] private OpponentPaddle _rightPaddle;
        [SerializeField] private Ball _ball;
        [SerializeField] Wall _leftWall;
        [SerializeField] Wall _rightWall;
        [SerializeField] KeyCode paddleUpKey;
        [SerializeField] KeyCode paddleDownKey;


        private async Task Start()
        {
            // so balls don't collide with one another
            Physics2D.IgnoreLayerCollision(15, 16);
            _leftWall.OnBallCollided += HandleBallCollided;
            _rightWall.OnBallCollided += HandleBallCollided;

            _leftPaddle.Init(_registry, paddleUpKey, paddleDownKey);
            _rightPaddle.Init(_registry);
            _ball.Init(_registry);

            var client = new Client("https", "lukenewproj.us-east1.nakamacloud.io", 443, "we4INqzP5e1E");

            try
            {
                ISession session = await client.AuthenticateCustomAsync(Guid.NewGuid().ToString());

                ISocket socket = client.NewSocket(useMainThread: true);
                await socket.ConnectAsync(session);
                await socket.AddMatchmakerAsync("*", minCount: 2, maxCount: 2);

                socket.ReceivedMatchmakerMatched += async matched =>
                {
                    var opcodes = new SyncOpcodes(handshakeRequestOpcode: 0, handshakeResponseOpcode: 1, dataOpcode: 2);
                    _match = await socket.JoinSyncMatch(session, opcodes, matched, _registry, e => new UnityLogger().ErrorFormat(e.Message), new UnityLogger());
                    _ball.ReceiveMatch(_match);
                    _ball.SetStartVelocity();
                };
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Application.Quit();
            }
        }

        private void HandleBallCollided(Wall wall, Ball ball)
        {
            ball.SetStartVelocity();
        }

        private async Task OnApplicationQuit()
        {
            await _socket.CloseAsync();
        }
    }
}
