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
    public class NGroup : INGroup
    {
        public byte[] Id { get; private set; }
        public bool Private { get; private set; }
        public byte[] CreatorId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string AvatarUrl { get; private set; }
        public string Lang { get; private set; }
        public byte[] Metadata { get; private set; }
        public long Count { get; private set; }
        public long CreatedAt { get; private set; }
        public long UpdatedAt { get; private set; }

        internal NGroup(Group message)
        {
            Id = message.Id.ToByteArray();
            Private = message.Private;
            CreatorId = message.CreatorId.ToByteArray();
            Name = message.Name;
            Description = message.Description;
            AvatarUrl = message.AvatarUrl;
            Lang = message.Lang;
            Metadata = message.Metadata.ToByteArray();
            Count = message.Count;
            CreatedAt = message.CreatedAt;
            UpdatedAt = message.UpdatedAt;
        }

        public override string ToString()
        {
            var f = "NGroup(Id={0},Private={1},CreatorId={2},Name={3},Description={4},AvatarUrl={5}," +
                    "Lang={6},Metadata={7},Count={8},CreatedAt={9},UpdatedAt={10})";
            return String.Format(f, Id, Private, CreatorId, Name, Description, AvatarUrl,
                    Lang, Metadata, Count, CreatedAt, UpdatedAt);
        }
    }
}
