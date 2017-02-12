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
    internal class NStorageKey : INStorageKey
    {
        public string Bucket { get; private set; }

        public string Collection { get; private set; }

        public string Record { get; private set; }

        public byte[] Version { get; private set; }

        internal NStorageKey(TStorageKey.Types.StorageKey message)
        {
            Bucket = message.Bucket;
            Collection = message.Collection;
            Record = message.Record;
            Version = message.Version.ToByteArray();
        }

        public override string ToString()
        {
            var f = "NStorageKey(Bucket={0},Collection={1},Record={2},Version={3}";
            return String.Format(f, Bucket, Collection, Record, Version);
        }
    }
}
