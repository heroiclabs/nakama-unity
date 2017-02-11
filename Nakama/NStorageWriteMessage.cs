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
    public class NStorageWriteMessage : INMessage<INResultSet<INStorageKey>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NStorageWriteMessage()
        {
            payload = new Envelope { StorageWrite = new TStorageWrite() };
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var data in payload.StorageWrite.Data)
            {
                output += String.Format("(Bucket={0}, Collection={1}, Record={2}, Version={3}),", data.Bucket, data.Collection, data.Record, data.Version);
            }
            return String.Format("NStorageWriteMessage(Keys={0})", output);
        }

        public class Builder
        {
            private NStorageWriteMessage message;

            public Builder()
            {
                message = new NStorageWriteMessage();
            }

            public NStorageWriteMessage Build()
            {
                var original = message;
                message = new NStorageWriteMessage
                {
                    payload = {StorageWrite = new TStorageWrite(original.payload.StorageWrite)}
                };
                return original;
            }

            public Builder Write(string bucket, string collection, string record, byte[] value)
            {
                var data = new TStorageWrite.Types.StorageData
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    Value = ByteString.CopyFrom(value)
                };
                message.payload.StorageWrite.Data.Add(data);

                return this;
            }

            public Builder Write(string bucket, string collection, string record, byte[] value, byte[] version)
            {
                var data = new TStorageWrite.Types.StorageData
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    Value = ByteString.CopyFrom(value),
                    Version = ByteString.CopyFrom(version)
                };
                message.payload.StorageWrite.Data.Add(data);

                return this;
            }
        }
    }
}
