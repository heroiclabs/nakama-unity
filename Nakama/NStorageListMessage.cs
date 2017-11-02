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
    public class NStorageListMessage : INCollatedMessage<INResultSet<INStorageData>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NStorageListMessage()
        {
            payload = new Envelope { StorageList = new TStorageList() };
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string output = "";
            var key = payload.StorageList;
            output += String.Format("(Bucket={0}, Collection={1}, UserId={2}, Limit={3}, Cursor={4}),", key.Bucket, key.Collection, key.UserId, key.Limit, key.Cursor);
            return String.Format("NStorageListMessage({0})", output);
        }

        public class Builder
        {
            private NStorageListMessage message;

            public Builder()
            {
                message = new NStorageListMessage();
            }

            public NStorageListMessage Build()
            {
                var original = message;
                message = new NStorageListMessage
                {
                    payload = {StorageList = new TStorageList(original.payload.StorageList)}
                };
                return original;
            }

            public Builder Bucket(string bucket)
            {
                message.payload.StorageList.Bucket = bucket;
                return this;
            }

            public Builder Collection(string collection)
            {
                message.payload.StorageList.Collection = collection;
                return this;
            }

            public Builder UserId(string userId)
            {
                message.payload.StorageList.UserId = userId;
                return this;
            }

            public Builder Limit(int limit)
            {
                message.payload.StorageList.Limit = limit;
                return this;
            }
            
            public Builder Cursor(INCursor cursor)
            {
                message.payload.StorageList.Cursor = cursor.Value;
                return this;
            }
        }
    }
}
