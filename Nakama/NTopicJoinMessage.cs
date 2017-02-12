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
using Google.Protobuf;

namespace Nakama
{
    public class NTopicJoinMessage : INMessage<INTopic>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NTopicJoinMessage()
        {
            payload = new Envelope {TopicJoin = new TTopicJoin()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NTopicJoinMessage(Id={0},IdCase={1})";
            var p = payload.TopicJoin;
            byte[] id = null;
            switch (p.IdCase)
            {
                case TTopicJoin.IdOneofCase.UserId:
                    id = p.UserId.ToByteArray();
                    break;
                case TTopicJoin.IdOneofCase.Room:
                    id = p.Room.ToByteArray();
                    break;
                case TTopicJoin.IdOneofCase.GroupId:
                    id = p.GroupId.ToByteArray();
                    break;
            }
            return String.Format(f, id, p.IdCase);
        }

        public class Builder
        {
            private NTopicJoinMessage message;

            public Builder()
            {
                message = new NTopicJoinMessage();
            }

            public Builder TopicDirectMessage(byte[] userId)
            {
                message.payload.TopicJoin.ClearId();
                message.payload.TopicJoin.UserId = ByteString.CopyFrom(userId);
                return this;
            }

            public Builder TopicRoom(byte[] room)
            {
                message.payload.TopicJoin.ClearId();
                message.payload.TopicJoin.Room = ByteString.CopyFrom(room);
                return this;
            }

            public Builder TopicGroup(byte[] groupId)
            {
                message.payload.TopicJoin.ClearId();
                message.payload.TopicJoin.GroupId = ByteString.CopyFrom(groupId);
                return this;
            }

            public NTopicJoinMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NTopicJoinMessage();
                message.payload.TopicJoin = new TTopicJoin(original.payload.TopicJoin);
                return original;
            }
        }
    }
}
