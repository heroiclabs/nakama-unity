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
    public class NTopicMessagesListMessage : INMessage<INResultSet<INTopicMessage>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NTopicMessagesListMessage()
        {
            payload = new Envelope {TopicMessagesList = new TTopicMessagesList()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NTopicMessagesListMessage(Id={0},IdCase={1},Cursor={2},Forward={3},Limit={4})";
            var p = payload.TopicMessagesList;
            byte[] id = null;
            switch (p.IdCase)
            {
                case TTopicMessagesList.IdOneofCase.UserId:
                    id = p.UserId.ToByteArray();
                    break;
                case TTopicMessagesList.IdOneofCase.Room:
                    id = p.Room.ToByteArray();
                    break;
                case TTopicMessagesList.IdOneofCase.GroupId:
                    id = p.GroupId.ToByteArray();
                    break;
            }
            return String.Format(f, id, p.IdCase, p.Cursor, p.Forward, p.Limit);
        }

        public class Builder
        {
            private NTopicMessagesListMessage message;

            public Builder()
            {
                message = new NTopicMessagesListMessage();
            }

            public Builder Forward(bool forward)
            {
                message.payload.TopicMessagesList.Forward = forward;
                return this;
            }

            public Builder Cursor(INCursor cursor)
            {
                message.payload.TopicMessagesList.Cursor = ByteString.CopyFrom(cursor.Value);
                return this;
            }

            public Builder Limit(long limit)
            {
                message.payload.TopicMessagesList.Limit = limit;
                return this;
            }

            public Builder TopicDirectMessage(byte[] userId)
            {
                message.payload.TopicMessagesList.ClearId();
                message.payload.TopicMessagesList.UserId = ByteString.CopyFrom(userId);
                return this;
            }

            public Builder TopicRoom(byte[] room)
            {
                message.payload.TopicMessagesList.ClearId();
                message.payload.TopicMessagesList.Room = ByteString.CopyFrom(room);
                return this;
            }

            public Builder TopicGroup(byte[] groupId)
            {
                message.payload.TopicMessagesList.ClearId();
                message.payload.TopicMessagesList.GroupId = ByteString.CopyFrom(groupId);
                return this;
            }

            public NTopicMessagesListMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NTopicMessagesListMessage();
                message.payload.TopicMessagesList = new TTopicMessagesList(original.payload.TopicMessagesList);
                return original;
            }
        }
    }
}
