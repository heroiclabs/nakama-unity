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
        private IMatch _match;
        private ISocket _socket;

        [SerializeField] private SelfPaddle _leftPaddle;
        [SerializeField] private OpponentPaddle _rightPaddle;

        [SerializeField] private Ball _ball;

        [SerializeField] Wall _leftWall;
        [SerializeField] Wall _rightWall;

        private async Task Start()
        {
            _leftWall.OnBallCollided += HandleBallCollided;
            _rightWall.OnBallCollided += HandleBallCollided;
            var client = new Client("http", "127.0.0.1", 7350, "defaultkey");

            _leftPaddle.Init(_registry);
            _rightPaddle.Init(_registry);
            _ball.Init(_registry);

            //var client = new Client("https", "lukenewproj.us-east1.nakamacloud.io", 443, "we4INqzP5e1E");

            try
            {


                ISession session = await GetSession(client);
                ISocket socket = client.NewSocket(useMainThread: true);
                await socket.ConnectAsync(session);
                await socket.AddMatchmakerAsync("*", minCount: 2, maxCount: 2);

                socket.ReceivedMatchmakerMatched += async matched =>
                {
                    Debug.Log("matchmaker matched.");
                    var opcodes = new SyncOpcodes(handshakeRequestOpcode: 0, handshakeResponseOpcode: 1, dataOpcode: 2);
                    _match = await socket.JoinSyncMatch(session, opcodes, matched, _registry, e => new UnityLogger().ErrorFormat(e.Message), new UnityLogger());
                    Debug.Log("done joining sync match.");
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
            ball.transform.position = Vector3.zero;
            ball.SetStartVelocity();
        }

        private async Task<ISession> GetSession(IClient client)
        {
            const string authJwtKey = "pongAuthJwt";
            const string refreshJwtKey = "pongRefreshJwt";

            if (PlayerPrefs.HasKey(authJwtKey) && PlayerPrefs.HasKey(refreshJwtKey))
            {
                string authJwt = PlayerPrefs.GetString(authJwtKey, defaultValue: null);
                string refreshJwt = PlayerPrefs.GetString(refreshJwtKey, defaultValue: null);
                return Session.Restore(authJwt, refreshJwt);
            }

            ISession session = await client.AuthenticateCustomAsync(Guid.NewGuid().ToString());
            PlayerPrefs.SetString(authJwtKey, session.AuthToken);
            PlayerPrefs.SetString(refreshJwtKey, session.RefreshToken);
            return session;
        }

        private async Task OnApplicationQuit()
        {
            await _socket.CloseAsync();
        }
    }
}