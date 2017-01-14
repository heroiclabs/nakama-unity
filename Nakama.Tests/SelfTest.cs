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
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Nakama.Tests
{
    [TestFixture]
    public class SelfTest
    {
        private static readonly string DefaultServerKey = "defaultkey";

        private INClient client;
        private INSession session;

        [SetUp]
        public void SetUp()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

//            client = NClient.Default(DefaultServerKey);
            client = new NClient.Builder(DefaultServerKey).Trace(true).Build();
            string id = TestContext.CurrentContext.Random.GetString();
            var message = NAuthenticateMessage.Device(id);
            client.Register(message, (INSession authenticated) =>
            {
                session = authenticated;
                client.Connect(session);
                evt.Set();
            }, (INError err) => {
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
        [Ignore("")]
        public void FetchUser()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSelf self = null;

            var message = NSelfFetchMessage.Default();
            client.Send(message, (INSelf result) => {
                self = result;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(self);
            Assert.NotNull(self.Id);
        }

        [Test]
        public void FetchUsers()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INUser> users = null;

            var id = (session as NSession).Id;
            var message = NUsersFetchMessage.Default(id);
            client.Send(message, (INResultSet<INUser> results) => {
                users = results;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(users);
            Assert.IsTrue(users.Results.Count == 1);
            Assert.NotNull(users.Results[0]);
        }

        [Test]
        [Ignore("")]
        public void UpdateUser()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NSelfUpdateMessage.Builder()
                    .AvatarUrl("http://graph.facebook.com/blah")
                    .Fullname("Foo Bar")
                    .Handle(TestContext.CurrentContext.Random.GetString(20))
                    .Lang("en")
                    .Location("San Francisco")
                    .Metadata(Encoding.UTF8.GetBytes("{}"))
                    .Timezone("Pacific Time")
                    .Build();
            client.Send(message, (bool completed) => {
                committed = completed;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsTrue(committed);
        }

        [Test]
        [Ignore("")]
        public void LinkUser()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSelf user = null;

            string id = TestContext.CurrentContext.Random.GetString(20);
            var message = NSelfLinkMessage.Device(id);
            client.Send(message, (bool completed) => {
                var message2 = NSelfFetchMessage.Default();
                client.Send(message2, (INSelf result) => {
                    user = result;
                    evt.Set();
                }, _ => {
                    evt.Set();
                });
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(user);
            Assert.True(user.DeviceIds.Contains(id));
        }

        [Test]
        [Ignore("")]
        public void UnlinkUser()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSelf user = null;

            string id = TestContext.CurrentContext.Random.GetString(20);
            var message = NSelfLinkMessage.Device(id);
            client.Send(message, (bool c1) => {
                var message2 = NSelfUnlinkMessage.Device(id);
                client.Send(message2, (bool c2) => {
                    var message3 = NSelfFetchMessage.Default();
                    client.Send(message3, (INSelf result) => {
                        user = result;
                        evt.Set();
                    }, _ => {
                        evt.Set();
                    });
                }, _ => {
                    evt.Set();
                });
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(user);
            Assert.False(user.DeviceIds.Contains(id));
        }
    }
}
