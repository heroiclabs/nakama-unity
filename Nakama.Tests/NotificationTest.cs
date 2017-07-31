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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Nakama.Tests
{
    [TestFixture]
    public class NotificationTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().Last());
        private static readonly string DeviceId = random.GetString();
        private static readonly string DefaultServerKey = "defaultkey";
        private INClient client;
        private byte[] userId;

        private static IList<INNotification> notifications;

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
        public void SendNotificationsRpc()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError err = null;

            var message = new NRuntimeRpcMessage.Builder("notification_send").Build();

            client.OnNotification = (INNotification notification) =>
            {
                Assert.IsTrue(notification.CreatedAt > 0);
                evt.Set();
            };

            client.Send(message, (INRuntimeRpc result) =>
            {
            }, (INError e) => {
                err = e;
                evt.Set();
            });

            evt.WaitOne(2000, false);
            Assert.IsNull(err);
        }

        [Test, Order(2)]
        public void ListNotifications()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INNotification> res = null;
            INError error = null;

            var message = NNotificationsListMessage.Default(10);
            client.Send(message, (INResultSet<INNotification> results) =>
            {
                res = results;
                evt.Set();
            }, (INError e) => {
                error = e;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Results.Count > 0);
            Assert.IsNotNull(res.Cursor);

            notifications = res.Results;
        }

        [Test, Order(3)]
        public void RemoveNotifications()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError err = null;

            var ids = new List<byte[]>();
            foreach (var n in notifications)
            {
                ids.Add(n.Id);
            }
            var message = NNotificationsRemoveMessage.Default(ids);
            client.Send(message, (bool results) =>
            {
                evt.Set();
            }, (INError error) => {
                evt.Set();
                err = error;
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(err);
        }
    }
}
