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
    public class StorageTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().Last());
        private static readonly string DeviceId = random.GetString();
        private static readonly string CustomId = random.GetString();
        private static readonly string DefaultServerKey = "defaultkey";

        private static readonly string Bucket = "testBucket";
        private static readonly string Collection = "testCollection";
        private static readonly string Record = "testRecord";
        private static readonly byte[] StorageValue = Encoding.UTF8.GetBytes("{\"jsonkey\":\"jsonvalue\"}");
        private static readonly byte[] IfNoneMatchVersion = Encoding.UTF8.GetBytes("*");
        private static readonly byte[] InvalidVersion = Encoding.UTF8.GetBytes("InvalidIfMatch");

        private static byte[] UserId;

        private INClient client;

        [OneTimeSetUp]
        public void GetSelfId()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var client2 = new NClient.Builder(DefaultServerKey).Build();
            var message = NAuthenticateMessage.Device(DeviceId);
            client2.Register(message, (INSession sess) =>
            {
                client2.Connect(sess);
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
        public void WriteStorageIfMatchRejected()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageKey> res = null;
            INError error = null;

            var message = new NStorageWriteMessage.Builder()
                    .Write(Bucket, Collection, Record, StorageValue, InvalidVersion)
                    .Build();
            client.Send(message, (INResultSet<INStorageKey> results) =>
            {
                res = results;
                evt.Set();
            }, (INError e) => {
                error = e;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(res);
            Assert.IsNotNull(error);
            Assert.AreEqual(ErrorCode.StorageRejected, error.Code);
            Assert.AreEqual("Storage write rejected: not found, version check failed, or permission denied", error.Message);
        }

        [Test, Order(2)]
        public void WriteStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageKey> res = null;

            var message = new NStorageWriteMessage.Builder()
                    .Write(Bucket, Collection, Record, StorageValue, StoragePermissionRead.OwnerRead, StoragePermissionWrite.OwnerWrite)
                    .Build();
            client.Send(message, (INResultSet<INStorageKey> results) =>
            {
                res = results;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(res);
            Assert.IsNotEmpty(res.Results);
            Assert.AreEqual(Bucket, res.Results[0].Bucket);
            Assert.AreEqual(Collection, res.Results[0].Collection);
            Assert.AreEqual(Record, res.Results[0].Record);
        }

        [Test, Order(3)]
        public void WriteStorageIfNoneMatchRejected()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageKey> res = null;
            INError error = null;

            var message = new NStorageWriteMessage.Builder()
                    .Write(Bucket, Collection, Record, StorageValue, IfNoneMatchVersion)
                    .Build();
            client.Send(message, (INResultSet<INStorageKey> results) =>
            {
                res = results;
                evt.Set();
            }, (INError e) => {
                error = e;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNull(res);
            Assert.IsNotNull(error);
            Assert.AreEqual(ErrorCode.StorageRejected, error.Code);
            Assert.AreEqual("Storage write rejected: not found, version check failed, or permission denied", error.Message);
        }

        [Test, Order(4)]
        public void FetchStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageData> storageData = null;
            INError error = null;

            var message = new NStorageFetchMessage.Builder()
                    .Fetch(Bucket, Collection, Record, UserId)
                    .Build();
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
            Assert.AreEqual(1, storageData.Results.Count);
            Assert.NotNull(storageData.Results[0]);
            Assert.AreEqual(Bucket, storageData.Results[0].Bucket);
            Assert.AreEqual(Collection, storageData.Results[0].Collection);
            Assert.AreEqual(Record, storageData.Results[0].Record);
            Assert.AreEqual(StorageValue, storageData.Results[0].Value);
        }
        
        [Test, Order(4)]
        public void ListStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageData> storageData = null;
            INError error = null;

            var message = new NStorageListMessage.Builder()
                .Bucket(Bucket)
                .Collection(Collection)
                .UserId(UserId)
                .Build();
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
            Assert.AreEqual(1, storageData.Results.Count);
            Assert.NotNull(storageData.Results[0]);
            Assert.AreEqual(Bucket, storageData.Results[0].Bucket);
            Assert.AreEqual(Collection, storageData.Results[0].Collection);
            Assert.AreEqual(Record, storageData.Results[0].Record);
            Assert.AreEqual(StorageValue, storageData.Results[0].Value);
        }

        [Test, Order(6)]
        public void RemoveStorageInvalidIfMatch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;
            INError error = null;

            var message = new NStorageRemoveMessage.Builder()
                    .Remove(Bucket, Collection, Record, InvalidVersion)
                    .Build();
            client.Send(message, (bool completed) => {
                committed = completed;
                evt.Set();
            }, (INError e) => {
                error = e;
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsFalse(committed);
            Assert.IsNotNull(error);
            Assert.AreEqual(ErrorCode.StorageRejected, error.Code);
            Assert.AreEqual("Storage remove rejected: not found, version check failed, or permission denied", error.Message);
        }

        [Test, Order(7)]
        public void RemoveStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NStorageRemoveMessage.Builder()
                    .Remove(Bucket, Collection, Record)
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
        
        [Test, Order(8)]
        public void WritePublicStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageKey> res = null;

            var message = new NStorageWriteMessage.Builder()
                .Write(Bucket, Collection, Record, StorageValue, StoragePermissionRead.PublicRead, StoragePermissionWrite.OwnerWrite)
                .Build();
            client.Send(message, (INResultSet<INStorageKey> results) =>
            {
                res = results;
                evt.Set();
            }, _ => {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.IsNotNull(res);
            Assert.IsNotEmpty(res.Results);
            Assert.AreEqual(Bucket, res.Results[0].Bucket);
            Assert.AreEqual(Collection, res.Results[0].Collection);
            Assert.AreEqual(Record, res.Results[0].Record);
        }
        
        [Test, Order(9)]
        public void ReadPublicStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageData> storageData = null;
            INError error = null;

            var message = new NStorageFetchMessage.Builder()
                .Fetch(Bucket, Collection, Record, UserId)
                .Build();
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
            Assert.AreEqual(1, storageData.Results.Count);
            Assert.NotNull(storageData.Results[0]);
            Assert.AreEqual(Bucket, storageData.Results[0].Bucket);
            Assert.AreEqual(Collection, storageData.Results[0].Collection);
            Assert.AreEqual(Record, storageData.Results[0].Record);
            Assert.AreEqual(StorageValue, storageData.Results[0].Value);
        }
        
        [Test, Order(10)]
        public void ReadPublicStorageOtherUsers()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INResultSet<INStorageData> storageData = null;
            INError error = null;
            
            var client2 = new NClient.Builder(DefaultServerKey).Build();
            var regMessage = NAuthenticateMessage.Custom(CustomId);
            client2.Register(regMessage, (INSession friendSession) =>
            {
                client2.Connect(friendSession);
                
                var message = new NStorageFetchMessage.Builder()
                    .Fetch(Bucket, Collection, Record, UserId)
                    .Build();
                client2.Send(message, (INResultSet<INStorageData> results) =>
                {
                    storageData = results;
                    evt.Set();
                }, (INError err) =>
                {
                    error = err;
                    evt.Set();
                });
            },(INError err) => {
                error = err;
                evt.Set();
            });

            evt.WaitOne(2000, false);
            Assert.IsNull(error);
            Assert.NotNull(storageData);
            Assert.NotNull(storageData.Results);
            Assert.AreEqual(1, storageData.Results.Count);
            Assert.NotNull(storageData.Results[0]);
            Assert.AreEqual(Bucket, storageData.Results[0].Bucket);
            Assert.AreEqual(Collection, storageData.Results[0].Collection);
            Assert.AreEqual(Record, storageData.Results[0].Record);
            Assert.AreEqual(StorageValue, storageData.Results[0].Value);
        }
        
        [Test, Order(11)]
        public void RemovePublicStorage()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            var committed = false;

            var message = new NStorageRemoveMessage.Builder()
                .Remove(Bucket, Collection, Record)
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
    }
}
