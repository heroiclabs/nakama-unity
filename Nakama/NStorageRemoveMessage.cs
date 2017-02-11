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
    public class NStorageRemoveMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NStorageRemoveMessage()
        {
            payload = new Envelope { StorageRemove = new TStorageRemove() };
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var key in payload.StorageRemove.Keys)
            {
                output += String.Format("(Bucket={0}, Collection={1}, Record={2}, Version={3}),", key.Bucket, key.Collection, key.Record, key.Version.ToBase64());
            }
            return String.Format("NStorageRemoveMessage(Keys={0})", output);
        }

        public class Builder
        {
            private NStorageRemoveMessage message;

            public Builder()
            {
                message = new NStorageRemoveMessage();
            }

            public NStorageRemoveMessage Build()
            {
                var original = message;
                message = new NStorageRemoveMessage
                {
                    payload = {StorageRemove = new TStorageRemove(original.payload.StorageRemove)}
                };
                return original;
            }

            public Builder Remove(string bucket, string collection, string record, byte[] version)
            {
                var storageKey = new TStorageRemove.Types.StorageKey
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    Version = ByteString.CopyFrom(version)
                };
                message.payload.StorageRemove.Keys.Add(storageKey);

                return this;
            }

            public Builder Remove(string bucket, string collection, string record)
            {
                var storageKey = new TStorageRemove.Types.StorageKey
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record
                };
                message.payload.StorageRemove.Keys.Add(storageKey);

                return this;
            }
        }
    }
}
