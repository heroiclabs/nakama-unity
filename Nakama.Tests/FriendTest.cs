/**
 * Copyright 2017 GameUp Online, Inc. d/b/a Heroic Labs.
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
    public class FriendTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().First());
        private static readonly string DefaultServerKey = "defaultkey";
        private static readonly string DeviceId = random.GetString();

        private static byte[] FriendUserId;
        private static string FriendHandle = "";
        private INClient client;

        [OneTimeSetUp]
        public void GetFriendId()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var client2 = new NClient.Builder(DefaultServerKey).Build();
            client2.OnDisconnect += (sender, _) =>
            {
                var message = NAuthenticateMessage.Device(DeviceId);
                client2.Register(message, (INSession session) =>
                {
                    evt.Set();
                }, (INError err) =>
                {
                    error = err;
                    evt.Set();
                });
            };

            var friendAuthMessage = NAuthenticateMessage.Device(random.GetString());
            client2.Register(friendAuthMessage, (INSession friendSession) =>
            {
                client2.Connect(friendSession);
                var friendSelfMessage = NSelfFetchMessage.Default();
                client2.Send(friendSelfMessage, (INSelf result) =>
                {
                    FriendUserId = result.Id;
                    FriendHandle = result.Handle;
                    client2.Logout();
                }, (INError err) => {
                    error = err;
                });
            },(INError err) => {
                error = err;
                evt.Set();
            });

            evt.WaitOne(5000, false);
            Assert.IsNull(error);
        }

        [TearDown]
        public void TearDown()
        {
            client.Disconnect();
        }

        [SetUp]
        public void SetUp()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client = new NClient.Builder(DefaultServerKey).Build();
            var message = NAuthenticateMessage.Device(DeviceId);
            client.Login(message, (INSession Session) =>
            {
                client.Connect(Session);
                evt.Set();
            },(INError err) => {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [Test, Order(1)]
        public void AddFriend()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = NFriendAddMessage.Default(FriendUserId);
            client.Send(message, (bool completed) => {
                committed = completed;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsTrue(committed);
        }

        [Test, Order(2)]
        public void ListFriends()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INFriend> friends = null;
            INError error = null;

            var message = NFriendsListMessage.Default();
            client.Send(message, (INResultSet<INFriend> results) =>
            {
                friends = results;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(2000, false);
            Assert.IsNull(error);
            Assert.NotNull(friends);
            Assert.NotNull(friends.Results);
            Assert.IsTrue(friends.Results.Count == 1);
            Assert.NotNull(friends.Results[0]);
            Assert.IsTrue(friends.Results[0].Handle == FriendHandle);
        }

        [Test, Order(3)]
        public void BlockFriend()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = NFriendBlockMessage.Default(FriendUserId);
            client.Send(message, (bool completed) => {
                committed = completed;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsTrue(committed);
        }

        [Test, Order(4)]
        public void RemoveFriend()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = NFriendRemoveMessage.Default(FriendUserId);
            client.Send(message, (bool completed) => {
                committed = completed;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsTrue(committed);
        }
    }
}