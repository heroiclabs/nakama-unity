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
    public class NTopicMessageSendMessage : INCollatedMessage<INTopicMessageAck>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NTopicMessageSendMessage(INTopicId topic, string data)
        {
            payload = new Envelope {TopicMessageSend = new TTopicMessageSend()};
            payload.TopicMessageSend.Topic = new TopicId();
            switch (topic.Type)
            {
                case TopicType.DirectMessage:
                    payload.TopicMessageSend.Topic.Dm = topic.Id;
                    break;
                case TopicType.Room:
                    payload.TopicMessageSend.Topic.Room = topic.Id;
                    break;
                case TopicType.Group:
                    payload.TopicMessageSend.Topic.GroupId = topic.Id;
                    break;
            }
            payload.TopicMessageSend.Data = data;
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NTopicMessageSendMessage(Topic={0},Data={1})";
            return String.Format(f, payload.TopicMessageSend.Topic, payload.TopicMessageSend.Data);
        }

        public static NTopicMessageSendMessage Default(INTopicId topic, string data)
        {
            return new NTopicMessageSendMessage(topic, data);
        }
    }
}
