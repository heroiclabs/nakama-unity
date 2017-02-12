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

namespace Nakama
{
    internal class NStorageData : INStorageData
    {
        public string Bucket { get; private set; }

        public string Collection { get; private set; }

        public string Record { get; private set; }

        public byte[] UserId { get; private set; }

        public byte[] Value { get; private set; }

        public byte[] Version { get; private set; }

        public long PermissionRead { get; private set; }

        public long PermissionWrite { get; private set; }

        public long CreatedAt { get; private set; }

        public long UpdatedAt { get; private set; }

        public long ExpiresAt { get; private set; }

        internal NStorageData(TStorageData.Types.StorageData message)
        {
            Bucket = message.Bucket;
            Collection = message.Collection;
            Record = message.Record;
            UserId = message.UserId.ToByteArray();
            Value = message.Value.ToByteArray();
            Version = message.Version.ToByteArray();
            PermissionRead = message.PermissionRead;
            PermissionWrite = message.PermissionWrite;
            CreatedAt = message.CreatedAt;
            UpdatedAt = message.UpdatedAt;
            ExpiresAt = message.ExpiresAt;
        }

        public override string ToString()
        {
            var f = "NStorageData(Bucket={0},Collection={1},Record={2},UserId={3},Value={4},Version={5}," +
                    "PermissionRead={6},PermissionWrite={7},CreatedAt={8},UpdatedAt={9},ExpiresAt={10})";
            return String.Format(f, Bucket, Collection, Record, UserId, Value, Version,
                                    PermissionRead, PermissionWrite, CreatedAt, UpdatedAt, ExpiresAt);
        }
    }
}
