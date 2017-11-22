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
    public class NGroupCreateMessage : INCollatedMessage<INResultSet<INGroup>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupCreateMessage()
        {
            payload = new Envelope {GroupsCreate = new TGroupsCreate { Groups =
            {
                new List<TGroupsCreate.Types.GroupCreate>()
            }}};   
        }

        private NGroupCreateMessage(string name)
        {
            payload = new Envelope {GroupsCreate = new TGroupsCreate { Groups =
            {
                new List<TGroupsCreate.Types.GroupCreate>
                {
                    new TGroupsCreate.Types.GroupCreate {Name = name}
                }
            }}};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NGroupCreateMessage(Name={0},Description={1},AvatarUrl={2},Lang={3},Metadata={4},Private={5})";
            var output = "";
            foreach (var p in payload.GroupsCreate.Groups)
            {
                output += String.Format(f, p.Name, p.Description, p.AvatarUrl, p.Lang, p.Metadata, p.Private); 
            }
            return output;
        }

        public class Builder
        {
            private NGroupCreateMessage message;

            public Builder(string name)
            {
                message = new NGroupCreateMessage(name);
            }

            public Builder Description(string description)
            {
                message.payload.GroupsCreate.Groups[0].Description = description;
                return this;
            }

            public Builder AvatarUrl(string avatarUrl)
            {
                message.payload.GroupsCreate.Groups[0].AvatarUrl = avatarUrl;
                return this;
            }

            public Builder Lang(string lang)
            {
                message.payload.GroupsCreate.Groups[0].Lang = lang;
                return this;
            }

            public Builder Metadata(string metadata)
            {
                message.payload.GroupsCreate.Groups[0].Metadata = metadata;
                return this;
            }

            public Builder Private(bool privateGroup)
            {
                message.payload.GroupsCreate.Groups[0].Private = privateGroup;
                return this;
            }

            public NGroupCreateMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NGroupCreateMessage();
                message.payload.GroupsCreate = new TGroupsCreate(original.payload.GroupsCreate);
                return original;
            }
        }
    }
}
