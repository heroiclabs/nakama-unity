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
using Google.Protobuf;

namespace Nakama
{
    public class NStorageFetchMessage : INMessage<INResultSet<INStorageData>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NStorageFetchMessage()
        {
            payload = new Envelope { StorageFetch = new TStorageFetch() };
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var key in payload.StorageFetch.Keys)
            {
                output += String.Format("(Bucket={0}, Collection={1}, Record={2}, UserId={3}),", key.Bucket, key.Collection, key.Record, key.UserId.ToBase64());
            }
            return String.Format("NStorageFetchMessage(Keys={0})", output);
        }

        public class Builder
        {
            private NStorageFetchMessage message;

            public Builder()
            {
                message = new NStorageFetchMessage();
            }

            public NStorageFetchMessage Build()
            {
                var original = message;
                message = new NStorageFetchMessage
                {
                    payload = {StorageFetch = new TStorageFetch(original.payload.StorageFetch)}
                };
                return original;
            }

            public Builder Fetch(string bucket, string collection, string record, byte[] userId)
            {
                var storageKey = new TStorageFetch.Types.StorageKey
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    UserId = ByteString.CopyFrom(userId)
                };
                message.payload.StorageFetch.Keys.Add(storageKey);

                return this;
            }
        }
    }
}
