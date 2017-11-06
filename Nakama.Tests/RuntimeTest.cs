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

using System.Threading;
using NUnit.Framework;

namespace Nakama.Tests
{
    [TestFixture]
    public class RuntimeTest
    {
        private static readonly string DefaultServerKey = "defaultkey";

        private INClient client;
        private INSession session;

        [SetUp]
        public void SetUp()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client = new NClient.Builder(DefaultServerKey).Build();
            string id = TestContext.CurrentContext.Random.GetString();
            var message = NAuthenticateMessage.Device(id);
            client.Register(message, (INSession authenticated) =>
            {
                session = authenticated;
                client.Connect(session);
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [TearDown]
        public void TearDown()
        {
            client.Disconnect();
        }

        [Test]
        [Ignore("Requires runtime module to be available.")]
        public void RpcLoopback()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INRuntimeRpc rpc = null;

            string payload = "payload-data";
            var message = new NRuntimeRpcMessage.Builder("loopback").Payload(payload).Build();
            client.Send(message, (INRuntimeRpc result) =>
            {
                rpc = result;
                evt.Set();
            }, _ =>
            {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(rpc);
            Assert.AreEqual("loopback", rpc.Id);
            Assert.AreEqual(payload, rpc.Payload);
        }
    }
}
