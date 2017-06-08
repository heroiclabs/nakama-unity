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
using System.Text;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Nakama.Tests
{
    [TestFixture]
    public class LeaderboardTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().First());
        private static readonly string DeviceId = random.GetString();
        private static readonly string DefaultServerKey = "defaultkey";
        private static readonly string LeaderboardIdName = "testLeaderboard";
        private static readonly string LeaderboardId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(LeaderboardIdName));

        private byte[] serverLeaderboardId;
        private INSelf self;
        private INClient client;

        [OneTimeSetUp]
        public void CheckTestLeaderboardExists()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var client2 = new NClient.Builder(DefaultServerKey).Build();
            var message = NAuthenticateMessage.Device(DeviceId);
            client2.Register(message, (INSession ses) =>
            {
                client2.Connect(ses);

                var selfMessage = NSelfFetchMessage.Default();
                client2.Send(selfMessage, (INSelf self) =>
                {
                    this.self = self;
                    var leaderboardListMessage = new NLeaderboardsListMessage.Builder().Build();
                    client2.Send(leaderboardListMessage , (INResultSet<INLeaderboard> results) =>
                    {
                        bool found = false;
                        foreach (var leaderboard in results.Results)
                        {
                            if (LeaderboardId.Equals(Convert.ToBase64String(leaderboard.Id)))
                            {
                                serverLeaderboardId = leaderboard.Id;
                                found = true;
                                break;
                            }
                        }
                        client2.Logout();
                        Assert.IsTrue(found, "Leaderboard not found. Setup the leaderboard ('{0}') in Nakama and run this test again.", LeaderboardIdName);
                        evt.Set();
                    }, (INError err) => {
                        error = err;
                        evt.Set();
                    }); 
                }, (INError err) => {
                    error = err;
                    evt.Set();
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
            client.Login(message, (INSession ses) =>
            {
                client.Connect(ses);
                evt.Set();
            },(INError err) => {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [Test, Order(1)]
        public void LeaderboardsList()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INLeaderboard> res = null;

            var message= new NLeaderboardsListMessage.Builder().Add(Encoding.UTF8.GetBytes(LeaderboardIdName)).Build();
            client.Send(message, (INResultSet<INLeaderboard> results) =>
            {
                res = results;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(res);
            Assert.IsNotEmpty(res.Results);
            Assert.AreEqual(Convert.ToBase64String(res.Results[0].Id), LeaderboardId);
            Assert.GreaterOrEqual(res.Results[0].Count, 0);
        }

        [Test, Order(2)]
        public void LeaderboardRecordWrite()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INLeaderboardRecord res = null;

            var score = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            var message = new NLeaderboardRecordWriteMessage.Builder(serverLeaderboardId)
                .Location("San Francisco")
                .Set(score)
                .Build();

            client.Send(message, (INLeaderboardRecord result) =>
            {
                res = result;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(res);
            Assert.IsNotEmpty(res.Handle);
            Assert.AreEqual("San Francisco", res.Location);
            Assert.AreEqual(score, res.Score);
            Assert.GreaterOrEqual(1, res.NumScore);
        }

        [Test, Order(3)]
        public void LeaderboardRecordsList()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INLeaderboardRecord> res = null;

            var message = new NLeaderboardRecordsListMessage.Builder(serverLeaderboardId)
                .FilterByLocation("San Francisco")
                .Build();
            client.Send(message, (INResultSet<INLeaderboardRecord> results) =>
            {
                res = results;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(res);
            Assert.IsNotEmpty(res.Results);
            Assert.GreaterOrEqual(res.Results.Count, 1);
            Assert.IsNotEmpty(res.Results[0].Handle);
            Assert.AreEqual(res.Results[0].Location, "San Francisco");
            Assert.Greater(res.Results[0].NumScore, 0);
        }
        
        [Test, Order(4)]
        public void LeaderboardRecordsListHaystack()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INLeaderboardRecord> res = null;

            var message = new NLeaderboardRecordsListMessage.Builder(serverLeaderboardId)
                .FilterByPagingToOwnerId(self.Id)
                .Limit(20)
                .Build();
            client.Send(message, (INResultSet<INLeaderboardRecord> results) =>
            {
                res = results;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(res);
            Assert.IsNotEmpty(res.Results);
            Assert.GreaterOrEqual(res.Results.Count, 1);
            Assert.IsNotEmpty(res.Results[0].Handle);
            Assert.Greater(res.Results[0].NumScore, 0);
        }

        [Test, Order(5)]
        public void LeaderboardRecordsFetch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INLeaderboardRecord> res = null;
            INError error = null;

            var message = new NLeaderboardRecordsFetchMessage.Builder(serverLeaderboardId).Build();
            client.Send(message, (INResultSet<INLeaderboardRecord> results) =>
            {
                res = results;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(res);
            Assert.IsNotEmpty(res.Results);
            Assert.GreaterOrEqual(res.Results.Count, 1);
            Assert.IsNotEmpty(res.Results[0].Handle);
            Assert.AreEqual(res.Results[0].Location, "San Francisco");
            Assert.Greater(res.Results[0].NumScore, 0);
        }
    }
}
