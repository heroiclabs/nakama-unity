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
using System.Collections.Generic;
using Google.Protobuf;

namespace Nakama
{
    public class NGroupUpdateMessage : INCollatedMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupUpdateMessage()
        {
            payload = new Envelope {GroupsUpdate = new TGroupsUpdate { Groups =
            {
                new List<TGroupsUpdate.Types.GroupUpdate>()
            }}};
        }

        private NGroupUpdateMessage(string groupId)
        {
            payload = new Envelope {GroupsUpdate = new TGroupsUpdate { Groups =
            {
                new List<TGroupsUpdate.Types.GroupUpdate>
                {
                    new TGroupsUpdate.Types.GroupUpdate {GroupId = groupId}
                }
            }}};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NGroupUpdateMessage(GroupId={0},Name={1},Description={2},AvatarUrl={3},Lang={4},Metadata={5},Private={6})";
            var output = "";
            foreach (var p in payload.GroupsUpdate.Groups)
            {
                output += String.Format(f, p.GroupId, p.Name, p.Description, p.AvatarUrl, p.Lang, p.Metadata, p.Private); 
            }
            return output;
        }

        public class Builder
        {
            private NGroupUpdateMessage message;

            public Builder(string groupId)
            {
                message = new NGroupUpdateMessage(groupId);
            }

            public Builder Name(string name)
            {
                message.payload.GroupsUpdate.Groups[0].Name = name;
                return this;
            }

            public Builder Description(string description)
            {
                message.payload.GroupsUpdate.Groups[0].Description = description;
                return this;
            }

            public Builder AvatarUrl(string avatarUrl)
            {
                message.payload.GroupsUpdate.Groups[0].AvatarUrl = avatarUrl;
                return this;
            }

            public Builder Lang(string lang)
            {
                message.payload.GroupsUpdate.Groups[0].Lang = lang;
                return this;
            }

            public Builder Metadata(string metadata)
            {
                message.payload.GroupsUpdate.Groups[0].Metadata = metadata;
                return this;
            }

            public Builder Private(bool privateGroup)
            {
                message.payload.GroupsUpdate.Groups[0].Private = privateGroup;
                return this;
            }

            public NGroupUpdateMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NGroupUpdateMessage();
                message.payload.GroupsUpdate = new TGroupsUpdate(original.payload.GroupsUpdate);
                return original;
            }
        }
    }
}
