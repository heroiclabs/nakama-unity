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
    public class NTopicLeaveMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NTopicLeaveMessage(INTopicId topic)
        {
            payload = new Envelope {TopicLeave = new TTopicLeave()};
            payload.TopicLeave.Topic = new TopicId();
            switch (topic.Type)
            {
                case TopicType.DirectMessage:
                    payload.TopicLeave.Topic.Dm = ByteString.CopyFrom(topic.Id);
                    break;
                case TopicType.Room:
                    payload.TopicLeave.Topic.Room = ByteString.CopyFrom(topic.Id);
                    break;
                case TopicType.Group:
                    payload.TopicLeave.Topic.GroupId = ByteString.CopyFrom(topic.Id);
                    break;
            }
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            return String.Format("NTopicLeaveMessage(Topic={0})", payload.TopicLeave.Topic);
        }

        public static NTopicLeaveMessage Default(INTopicId topic)
        {
            return new NTopicLeaveMessage(topic);
        }
    }
}
