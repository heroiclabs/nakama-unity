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
    public class NTopicLeaveMessage : INCollatedMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NTopicLeaveMessage(INTopicId topic)
        {
            var topicId = new TopicId();
            switch (topic.Type)
            {
                case TopicType.DirectMessage:
                    topicId.Dm = topic.Id;
                    break;
                case TopicType.Room:
                    topicId.Room = topic.Id;
                    break;
                case TopicType.Group:
                    topicId.GroupId = topic.Id;
                    break;
            }
            payload = new Envelope {TopicsLeave = new TTopicsLeave { Topics =
            {
                new List<TopicId> { topicId }
            }}};      
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {   
            return String.Format("NTopicLeaveMessage(Topic={0})", payload.TopicsLeave.Topics);
        }

        public static NTopicLeaveMessage Default(INTopicId topic)
        {
            return new NTopicLeaveMessage(topic);
        }
    }
}
