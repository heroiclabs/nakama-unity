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
using System.Threading;
using NUnit.Framework;

namespace Nakama.Tests
{
    [TestFixture]
    public class ConnectTest
    {
        private static readonly string DefaultServerKey = "defaultkey";

        [Test]
        public void Connect_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var connected = false;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Device(id);
            client.Register(message, (INSession session) =>
            {
                client.Connect(session, (bool _) => {
                    connected = true;
                    evt.Set();
                });
            }, (INError _) => {
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.IsTrue(connected);
            client.Disconnect();
        }

        [Test]
        public void GetServerTime_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            long serverTime = 0;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Device(id);
            client.Register(message, (INSession session) =>
            {
                client.Connect(session);
                Thread.Sleep(500);
                serverTime = client.ServerTime;
                evt.Set();
            }, (INError _) => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.That(serverTime, Is.Not.EqualTo(0));
//            Thread.Sleep(30000);
            client.Disconnect();
        }
    }
}
