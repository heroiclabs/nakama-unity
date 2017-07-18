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
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Nakama.Tests
{
    [TestFixture]
    public class PurchaseTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().Last());
        private static readonly string DeviceId = random.GetString();
        private static readonly string DefaultServerKey = "defaultkey";
        private INClient client;
        private byte[] userId;

        [SetUp]
        public void SetUp()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client = new NClient.Builder(DefaultServerKey).Build();
            var message = NAuthenticateMessage.Device(DeviceId);
            client.Login(message, (INSession session) =>
            {
                userId = session.Id;
                client.Connect(session);
                evt.Set();
            },(INError err) => {
                client.Register(message, (INSession session) =>
                {
                    userId = session.Id;
                    client.Connect(session);
                    evt.Set();
                },(INError err2) => {
                    error = err2;
                    evt.Set();
                });
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [Test, Order(1)]
        public void ApplePurchaseTest()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INPurchaseRecord res = null;
            INError error = null;

            var message = NPurchaseValidateMessage.Apple("product_id", "base64_receipt_data");
            client.Send(message, (INPurchaseRecord record) =>
            {
                res = record;
                evt.Set();
            }, (INError e) => {
                error = e;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(res);
        }

        [Test, Order(2)]
        public void GooglePurchaseTest()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INPurchaseRecord res = null;
            INError error = null;

            var message = NPurchaseValidateMessage.Google("product_id", "product", "purchase_token");
            client.Send(message, (INPurchaseRecord record) =>
            {
                res = record;
                evt.Set();
            }, (INError e) => {
                error = e;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(res);
        }
    }
}