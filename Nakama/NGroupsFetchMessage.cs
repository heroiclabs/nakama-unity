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
    public class NGroupsFetchMessage : INMessage<INResultSet<INGroup>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupsFetchMessage(params byte[] ids)
        {
            var request = new TGroupsFetch();
            request.GroupIds.Add(ByteString.CopyFrom(ids));
            payload = new Envelope {GroupsFetch = request};
        }

        private NGroupsFetchMessage()
        {
            payload = new Envelope {GroupsFetch = new TGroupsFetch()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var id in payload.GroupsFetch.GroupIds)
            {
                output += id.ToBase64() + ",";
            }
            return String.Format("NGroupsFetchMessage(GroupIds={0})", output);
        }

        public static NGroupsFetchMessage Default(params byte[] ids)
        {
            return new NGroupsFetchMessage.Builder(ids).Build();
        }

        public class Builder
        {
            private NGroupsFetchMessage message;

            public Builder(params byte[] ids)
            {
                message = new NGroupsFetchMessage(ids);
            }

            public Builder Add(params byte[] ids)
            {
                message.payload.GroupsFetch.GroupIds.Add(ByteString.CopyFrom(ids));
                return this;
            }

            public Builder Add(IEnumerable<byte[]> ids)
            {
                foreach (var id in ids)
                {
                    message.payload.GroupsFetch.GroupIds.Add(ByteString.CopyFrom(id));
                }
                return this;
            }

            public NGroupsFetchMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NGroupsFetchMessage();
                message.payload.GroupsFetch.GroupIds.Add(original.payload.GroupsFetch.GroupIds);
                return original;
            }
        }
    }
}
