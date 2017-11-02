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
    public class NStorageWriteMessage : INCollatedMessage<INResultSet<INStorageKey>>
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
                output += String.Format("(Bucket={0}, Collection={1}, Record={2}, Version={3}, ReadPermission={4}, WritePermission={5}),", 
                    data.Bucket, data.Collection, data.Record, data.Version, data.PermissionRead, data.PermissionWrite);
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

            public Builder Write(string bucket, string collection, string record, string value)
            {
                var data = new TStorageWrite.Types.StorageData
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    Value = value,
                    PermissionRead = GetReadPermission(StoragePermissionRead.OwnerRead),
                    PermissionWrite = GetWritePermission(StoragePermissionWrite.OwnerWrite)
                };
                message.payload.StorageWrite.Data.Add(data);

                return this;
            }

            public Builder Write(string bucket, string collection, string record, string value, string version)
            {
                var data = new TStorageWrite.Types.StorageData
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    Value = value,
                    Version = version,
                    PermissionRead = GetReadPermission(StoragePermissionRead.OwnerRead),
                    PermissionWrite = GetWritePermission(StoragePermissionWrite.OwnerWrite)
                };
                message.payload.StorageWrite.Data.Add(data);

                return this;
            }

            public Builder Write(string bucket, string collection, string record, string value, StoragePermissionRead readPermission, StoragePermissionWrite writePermission)
            {
                var data = new TStorageWrite.Types.StorageData
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    Value = value,
                    PermissionRead = GetReadPermission(readPermission),
                    PermissionWrite = GetWritePermission(writePermission)
                };
                message.payload.StorageWrite.Data.Add(data);

                return this;
            }

            public Builder Write(string bucket, string collection, string record, string value, StoragePermissionRead readPermission, StoragePermissionWrite writePermission, string version)
            {
                var data = new TStorageWrite.Types.StorageData
                {
                    Bucket = bucket,
                    Collection = collection,
                    Record = record,
                    Value = value,
                    Version = version,
                    PermissionRead = GetReadPermission(readPermission),
                    PermissionWrite = GetWritePermission(writePermission)
                };
                message.payload.StorageWrite.Data.Add(data);

                return this;
            }
        }

        internal static int GetReadPermission(StoragePermissionRead readPermission)
        {
            switch (readPermission)
            {
                case StoragePermissionRead.NoRead:
                    return 0;
                case StoragePermissionRead.OwnerRead:
                    return 1;
                case StoragePermissionRead.PublicRead:
                    return 2;
                default:
                    return 1;
            }
        }

        internal static int GetWritePermission(StoragePermissionWrite writePermission)
        {
            switch (writePermission)
            {
                case StoragePermissionWrite.NoWrite:
                    return 0;
                case StoragePermissionWrite.OwnerWrite:
                    return 1;
                default:
                    return 1;
            }
        }
    }
}
