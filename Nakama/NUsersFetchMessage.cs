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
    public class NUsersFetchMessage : INMessage<INResultSet<INUser>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NUsersFetchMessage(params byte[] ids)
        {
            var request = new TUsersFetch();
            request.UserIds.Add(ByteString.CopyFrom(ids));
            payload = new Envelope {UsersFetch = request};
        }

        private NUsersFetchMessage()
        {
            payload = new Envelope {UsersFetch = new TUsersFetch()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var id in payload.UsersFetch.UserIds)
            {
                output += id.ToBase64() + ",";
            }
            return String.Format("NUsersFetchMessage(UserIds={0})", output);
        }

        public static NUsersFetchMessage Default(params byte[] ids)
        {
            return new NUsersFetchMessage.Builder(ids).Build();
        }

        public class Builder
        {
            private NUsersFetchMessage message;

            public Builder(params byte[] ids)
            {
                message = new NUsersFetchMessage(ids);
            }

            public Builder Add(params byte[] ids)
            {
                message.payload.UsersFetch.UserIds.Add(ByteString.CopyFrom(ids));
                return this;
            }

            public Builder Add(IEnumerable<byte[]> ids)
            {
                foreach (var id in ids)
                {
                    message.payload.UsersFetch.UserIds.Add(ByteString.CopyFrom(id));
                }
                return this;
            }

            public NUsersFetchMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NUsersFetchMessage();
                message.payload.UsersFetch.UserIds.Add(original.payload.UsersFetch.UserIds);
                return original;
            }
        }
    }
}
