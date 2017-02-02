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

using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Nakama.Tests
{
    [TestFixture]
    public class StorageTest
    {
        private static readonly string DefaultServerKey = "defaultkey";
        private static readonly string DeviceId = TestContext.CurrentContext.Random.GetString();

        private static readonly string Bucket = "testBucket";
        private static readonly string Collection = "testCollection";
        private static readonly string Record = "testRecord";
        private static readonly byte[] StorageValue = Encoding.UTF8.GetBytes("{\"jsonkey\":\"jsonvalue\"}");
        private static readonly byte[] IfNoneMatchVersion = Encoding.UTF8.GetBytes("*");
        private static readonly byte[] InvalidVersion = Encoding.UTF8.GetBytes("InvalidIfMatch");

        private static byte[] UserId;

        private INClient client;

        [OneTimeSetUp]
        public void GetFriendId()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var client2 = new NClient.Builder(DefaultServerKey).Build();
            var message = NAuthenticateMessage.Device(DeviceId);
            client2.Register(message, (INSession friendSession) =>
            {
                client2.Connect(friendSession);
                var selfMessage = NSelfFetchMessage.Default();
                client2.Send(selfMessage, (INSelf result) =>
                {
                    UserId = result.Id;
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
            client.Login(message, (INSession friendSession) =>
            {
                client.Connect(friendSession);
                evt.Set();
            },(INError err) => {
                error = err;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(error);
        }

        [Test, Order(1)]
        public void WriteStorageInvalidIfMatch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NStorageWriteMessage.Builder().Write(Bucket, Collection, Record, StorageValue, InvalidVersion).Build();
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
        public void WriteStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NStorageWriteMessage.Builder().Write(Bucket, Collection, Record, StorageValue).Build();
            client.Send(message, (bool completed) => {
                committed = completed;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsTrue(committed);
        }

        [Test, Order(3)]
        public void WriteStorageIfNoneMatch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NStorageWriteMessage.Builder().Write(Bucket, Collection, Record, StorageValue, IfNoneMatchVersion).Build();
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
        public void FetchStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageData> storageData = null;
            INError error = null;

            var message = new NStorageFetchMessage.Builder().Fetch(Bucket, Collection, Record, UserId).Build();
            client.Send(message, (INResultSet<INStorageData> results) =>
            {
                storageData = results;
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(2000, false);
            Assert.IsNull(error);
            Assert.NotNull(storageData);
            Assert.NotNull(storageData.Results);
            Assert.Equals(storageData.Results.Count, 1);
            Assert.NotNull(storageData.Results[0]);
            Assert.Equals(storageData.Results[0].Bucket, Bucket);
            Assert.Equals(storageData.Results[0].Collection, Collection);
            Assert.Equals(storageData.Results[0].Record, Record);
            Assert.Equals(storageData.Results[0].Value, StorageValue);
        }

        [Test, Order(5)]
        public void RemoveStorageIfMatch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NStorageRemoveMessage.Builder().Remove(Bucket, Collection, Record, InvalidVersion).Build();
            client.Send(message, (bool completed) => {
                committed = completed;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsTrue(committed);
        }

        [Test, Order(6)]
        public void RemoveStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NStorageRemoveMessage.Builder().Remove(Bucket, Collection, Record).Build();
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