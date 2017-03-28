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

        private NGroupsFetchMessage(params byte[][] ids)
        {
            var request = new TGroupsFetch();
            request.GroupIds = new TGroupsFetch.Types.GroupIds();
            foreach (var id in ids)
            {
                request.GroupIds.GroupIds_.Add(ByteString.CopyFrom(id));
            }
            payload = new Envelope {GroupsFetch = request};
        }

        private NGroupsFetchMessage(params string[] names)
        {
            var request = new TGroupsFetch();
            request.Names = new TGroupsFetch.Types.Names();
            foreach (var name in names)
            {
                request.Names.Names_.Add(name);
            }
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
            switch (payload.GroupsFetch.SetCase)
            {
                case TGroupsFetch.SetOneofCase.GroupIds:
                    output += "GroupIds=";
                    foreach (var id in payload.GroupsFetch.GroupIds.GroupIds_)
                    {
                        output += id.ToBase64() + ",";
                    }
                    break;
                case TGroupsFetch.SetOneofCase.Names:
                    output += "Names=";
                    foreach (var name in payload.GroupsFetch.Names.Names_)
                    {
                        output += name + ",";
                    }
                    break;
                default:
                    output += "unknown";
                    break;
            }
            return String.Format("NGroupsFetchMessage({0})", output);
        }

        public static NGroupsFetchMessage Default(params byte[][] ids)
        {
            return new NGroupsFetchMessage.Builder(ids).Build();
        }

        public class Builder
        {
            private NGroupsFetchMessage message;

            public Builder(params byte[][] ids)
            {
                message = new NGroupsFetchMessage(ids);
            }

            public Builder(params string[] names)
            {
                message = new NGroupsFetchMessage(names);
            }

            public Builder()
            {
                message = new NGroupsFetchMessage();
            }

            public Builder SetGroupIds(params byte[][] ids)
            {
                message.payload.GroupsFetch.ClearSet();
                message.payload.GroupsFetch.GroupIds = new TGroupsFetch.Types.GroupIds();
                foreach (var id in ids)
                {
                    message.payload.GroupsFetch.GroupIds.GroupIds_.Add(ByteString.CopyFrom(id));
                }
                return this;
            }

            public Builder SetGroupIds(IEnumerable<byte[]> ids)
            {
                message.payload.GroupsFetch.ClearSet();
                message.payload.GroupsFetch.GroupIds = new TGroupsFetch.Types.GroupIds();
                foreach (var id in ids)
                {
                    message.payload.GroupsFetch.GroupIds.GroupIds_.Add(ByteString.CopyFrom(id));
                }
                return this;
            }

            public Builder SetNames(params string[] names)
            {
                message.payload.GroupsFetch.ClearSet();
                message.payload.GroupsFetch.Names = new TGroupsFetch.Types.Names();
                foreach (var name in names)
                {
                    message.payload.GroupsFetch.Names.Names_.Add(name);
                }
                return this;
            }

            public Builder SetNames(IEnumerable<string> names)
            {
                message.payload.GroupsFetch.ClearSet();
                message.payload.GroupsFetch.Names = new TGroupsFetch.Types.Names();
                foreach (var name in names)
                {
                    message.payload.GroupsFetch.Names.Names_.Add(name);
                }
                return this;
            }

            public NGroupsFetchMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NGroupsFetchMessage();
                message.payload.GroupsFetch = new TGroupsFetch(original.payload.GroupsFetch);
                return original;
            }
        }
    }
}
