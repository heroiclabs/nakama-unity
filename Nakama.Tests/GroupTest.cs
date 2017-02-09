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
    public class GroupTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().First());
        private static readonly string DefaultServerKey = "defaultkey";
        private static readonly string DeviceId = random.GetString();

        private static readonly string GroupName = "testGroup";
        private static readonly string GroupDescription = "testGroupDescription";
        private static readonly bool PrivateGroup = true;

        private static byte[] FriendUserId;
        private static byte[] MyUserId;
        private static INGroup FriendGroup;
        private INClient client;
        private INGroup myGroup;

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
                    MyUserId = ((NSession) session).Id;
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
                FriendUserId = ((NSession) friendSession).Id;
                client2.Connect(friendSession);

                var builder = new NGroupCreateMessage.Builder(GroupName).Description(GroupDescription).Lang("fa").Private(PrivateGroup);
                client.Send(builder.Build(), (INGroup group) =>
                {
                    FriendGroup = group;
                    client2.Logout();
                }, (INError err) => {
                    error = err;
                });
            },(INError err) => {
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
        public void CreateGroup()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var builder = new NGroupCreateMessage.Builder(GroupName).Description(GroupDescription).Lang("en").Private(PrivateGroup);
            client.Send(builder.Build(), (INGroup group) =>
            {
                myGroup = group;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(myGroup);
            Assert.AreEqual(GroupDescription, myGroup.Name);
            Assert.AreEqual(GroupDescription, myGroup.Description);
            Assert.AreEqual(PrivateGroup, myGroup.Private);
        }

        [Test, Order(2)]
        public void GroupsList()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;
            INResultSet<INGroup> groups = null;

            var message = new NGroupListsMessage.Builder().OrderByAsc(true).FilterByLang("en").Build();
            client.Send(message, (INResultSet<INGroup> results) =>
            {
                groups = results;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.NotNull(groups);
            Assert.NotNull(groups.Results);
            Assert.IsTrue(groups.Results.Count == 1);
            Assert.NotNull(groups.Results[0]);
            Assert.AreEqual(groups.Results[0].Id, myGroup.Id);
        }

        [Test, Order(3)]
        public void GroupUpdate()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = new NGroupUpdateMessage.Builder(myGroup.Id).AvatarUrl(GroupDescription).Build();
            client.Send(message, (bool completed) => {
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [Test, Order(4)]
        public void GroupsSelfList()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;
            INResultSet<INGroup> groups = null;

            var message = NGroupsSelfListMessage.Default();
            client.Send(message, (INResultSet<INGroup> results) =>
            {
                groups = results;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.NotNull(groups);
            Assert.NotNull(groups.Results);
            Assert.IsTrue(groups.Results.Count == 1);
            Assert.NotNull(groups.Results[0]);
            Assert.AreEqual(groups.Results[0].Id, myGroup.Id);
            Assert.AreEqual(groups.Results[0].AvatarUrl, GroupDescription);
        }

        [Test, Order(5)]
        public void GroupRemove()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = NGroupRemoveMessage.Default(myGroup.Id);
            client.Send(message, (bool completed) => {
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [Test, Order(6)]
        public void GroupsFetch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;
            INResultSet<INGroup> groups = null;

            var message = new NGroupsFetchMessage.Builder(myGroup.Id).Build();
            client.Send(message, (INResultSet<INGroup> results) =>
            {
                groups = results;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.NotNull(groups);
            Assert.NotNull(groups.Results);
            Assert.IsTrue(groups.Results.Count == 1);
            Assert.NotNull(groups.Results[0]);
            Assert.AreEqual(groups.Results[0].Id, myGroup.Id);
            Assert.AreEqual(groups.Results[0].Lang, "fa"); // make sure that only group left is friend's group with FA lang
        }

        [Test, Order(7)]
        public void GroupJoin()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = NGroupJoinMessage.Default(FriendGroup.Id);
            client.Send(message, (bool completed) => {
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [Test, Order(8)]
        public void GroupUsersList()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;
            INResultSet<INGroupUser> groupUsers = null;

            var message = NGroupUsersListMessage.Default(FriendGroup.Id);
            client.Send(message, (INResultSet<INGroupUser> results) => {
                groupUsers = results;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.NotNull(groupUsers);
            Assert.NotNull(groupUsers.Results);
            Assert.IsTrue(groupUsers.Results.Count == 2);
            Assert.NotNull(groupUsers.Results[0]);
            Assert.AreEqual(groupUsers.Results[0].Id, FriendUserId);
            Assert.AreEqual(groupUsers.Results[1].Id, MyUserId);
        }

        [Test, Order(9)]
        public void GroupPromote()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = NGroupPromoteUserMessage.Default(FriendGroup.Id, MyUserId);
            client.Send(message, (bool completed) => {
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(error);  // this is expected to fail as you aren't admin
        }

        [Test, Order(10)]
        public void GroupKick()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = NGroupKickUserMessage.Default(FriendGroup.Id, FriendUserId);
            client.Send(message, (bool completed) => {
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(error); // this is expected to fail as you aren't admin
        }

        [Test, Order(11)]
        public void GroupLeave()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = NGroupLeaveMessage.Default(FriendGroup.Id);
            client.Send(message, (bool completed) => {
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }
    }
}