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
    public class NTopicJoinMessage : INCollatedMessage<INResultSet<INTopic>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NTopicJoinMessage()
        {
            payload = new Envelope {TopicsJoin = new TTopicsJoin {Joins =
            {
                new List<TTopicsJoin.Types.TopicJoin>()
            }}};   
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var p = payload.TopicsJoin;
            string output = "";
            foreach (var j in p.Joins)
            {
                switch (j.IdCase)
                {
                    case TTopicsJoin.Types.TopicJoin.IdOneofCase.UserId:
                        output += String.Format("(Id={0},IdCase={1}),", j.UserId, j.IdCase);
                        break;
                    case TTopicsJoin.Types.TopicJoin.IdOneofCase.Room:
                        output += String.Format("(Id={0},IdCase={1}),", j.Room, j.IdCase);
                        break;
                    case TTopicsJoin.Types.TopicJoin.IdOneofCase.GroupId:
                        output += String.Format("(Id={0},IdCase={1}),", j.GroupId, j.IdCase);
                        break;
                }
            }
            return String.Format("NTopicJoinMessage({0})", output);
        }

        public class Builder
        {
            private NTopicJoinMessage message;

            public Builder()
            {
                message = new NTopicJoinMessage();
            }

            public Builder TopicDirectMessage(string userId)
            {
                message.payload.TopicsJoin.Joins.Add(new TTopicsJoin.Types.TopicJoin
                {
                    UserId = userId
                });
                return this;
            }

            public Builder TopicRoom(string room)
            {
                message.payload.TopicsJoin.Joins.Add(new TTopicsJoin.Types.TopicJoin
                {
                    Room = room
                });
                return this;
            }

            public Builder TopicGroup(string groupId)
            {
                message.payload.TopicsJoin.Joins.Add(new TTopicsJoin.Types.TopicJoin
                {
                    GroupId = groupId
                });
                return this;
            }

            public NTopicJoinMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NTopicJoinMessage();
                message.payload.TopicsJoin = new TTopicsJoin(original.payload.TopicsJoin);
                return original;
            }
        }
    }
}
