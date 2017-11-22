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
    internal class NGroupSelf : INGroupSelf
    {
        public string Id { get; private set; }
        public bool Private { get; private set; }
        public string CreatorId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string AvatarUrl { get; private set; }
        public string Lang { get; private set; }
        public string Metadata { get; private set; }
        public long Count { get; private set; }
        public long CreatedAt { get; private set; }
        public long UpdatedAt { get; private set; }
        public GroupState State { get; private set; }

        internal NGroupSelf(TGroupsSelf.Types.GroupSelf message)
        {
            Id = message.Group.Id;
            Private = message.Group.Private;
            CreatorId = message.Group.CreatorId;
            Name = message.Group.Name;
            Description = message.Group.Description;
            AvatarUrl = message.Group.AvatarUrl;
            Lang = message.Group.Lang;
            Metadata = message.Group.Metadata;
            Count = message.Group.Count;
            CreatedAt = message.Group.CreatedAt;
            UpdatedAt = message.Group.UpdatedAt;

            switch (message.State)
            {
                case 0:
                    State = GroupState.Admin;
                    break;
                case 1:
                    State = GroupState.Member;
                    break;
                case 2:
                    State = GroupState.Join;
                    break;
            }
        }

        public override string ToString()
        {
            var f = "NGroupSelf(Id={0},Private={1},CreatorId={2},Name={3},Description={4},AvatarUrl={5}," +
                    "Lang={6},Metadata={7},Count={8},CreatedAt={9},UpdatedAt={10},State={11})";
            return String.Format(f, Id, Private, CreatorId, Name, Description, AvatarUrl,
                Lang, Metadata, Count, CreatedAt, UpdatedAt,State);
        }

    }
}