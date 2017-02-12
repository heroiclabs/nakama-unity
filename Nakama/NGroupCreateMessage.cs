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
using System.Text;
using Google.Protobuf;

namespace Nakama
{
    public class NGroupCreateMessage : INMessage<INGroup>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupCreateMessage()
        {
            payload = new Envelope {GroupCreate = new TGroupCreate()};
        }

        private NGroupCreateMessage(string name)
        {
            var request = new TGroupCreate();
            request.Name = name;
            payload = new Envelope {GroupCreate = request};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NGroupCreateMessage(Name={0},Description={1},AvatarUrl={2},Lang={3},Metadata={4},Private={5})";
            var p = payload.GroupCreate;
            return String.Format(f, p.Name, p.Description, p.AvatarUrl, p.Lang, p.Metadata, p.Private);
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
                message.payload.GroupCreate.Description = description;
                return this;
            }

            public Builder AvatarUrl(string avatarUrl)
            {
                message.payload.GroupCreate.AvatarUrl = avatarUrl;
                return this;
            }

            public Builder Lang(string lang)
            {
                message.payload.GroupCreate.Lang = lang;
                return this;
            }

            public Builder Metadata(byte[] metadata)
            {
                message.payload.GroupCreate.Metadata = ByteString.CopyFrom(metadata);
                return this;
            }

            public Builder Private(bool privateGroup)
            {
                message.payload.GroupCreate.Private = privateGroup;
                return this;
            }

            public NGroupCreateMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NGroupCreateMessage();
                message.payload.GroupCreate = new TGroupCreate(original.payload.GroupCreate);
                return original;
            }
        }
    }
}
