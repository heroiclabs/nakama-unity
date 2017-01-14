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
    public class NSelfUpdateMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NSelfUpdateMessage()
        {
            payload = new Envelope {SelfUpdate = new TSelfUpdate()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NSelfUpdateMessage(AvatarUrl={0},Fullname={1},Handle={2},Lang={3},Location={4},Metadata={5},Timezone={6})";
            var p = payload.SelfUpdate;
            return String.Format(f, p.AvatarUrl, p.Fullname, p.Handle, p.Lang, p.Location, p.Metadata, p.Timezone);
        }

        public class Builder
        {
            private NSelfUpdateMessage message;

            public Builder()
            {
                message = new NSelfUpdateMessage();
            }

            public Builder AvatarUrl(string avatarUrl)
            {
                message.payload.SelfUpdate.AvatarUrl = avatarUrl;
                return this;
            }

            public Builder Handle(string handle)
            {
                message.payload.SelfUpdate.Handle = handle;
                return this;
            }

            public Builder Fullname(string fullname)
            {
                message.payload.SelfUpdate.Fullname = fullname;
                return this;
            }

            public Builder Lang(string lang)
            {
                message.payload.SelfUpdate.Lang = lang;
                return this;
            }

            public Builder Location(string location)
            {
                message.payload.SelfUpdate.Location = location;
                return this;
            }

            public Builder Metadata(byte[] metadata)
            {
                message.payload.SelfUpdate.Metadata = ByteString.CopyFrom(metadata);
                return this;
            }

            public Builder Timezone(string timezone)
            {
                message.payload.SelfUpdate.Timezone = timezone;
                return this;
            }

            public NSelfUpdateMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NSelfUpdateMessage();
                message.payload.SelfUpdate = new TSelfUpdate(original.payload.SelfUpdate);
                return original;
            }
        }
    }
}
