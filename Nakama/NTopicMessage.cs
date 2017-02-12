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

namespace Nakama
{
    public class NTopicMessage : INTopicMessage
    {
        public INTopicId Topic { get; private set; }

        public byte[] UserId { get; private set; }

        public byte[] MessageId { get; private set; }

        public long CreatedAt { get; private set; }

        public long ExpiresAt { get; private set; }

        public string Handle { get; private set; }

        public TopicMessageType Type { get; private set; }

        public byte[] Data { get; private set; }

        internal NTopicMessage(TopicMessage message)
        {
            Topic = new NTopicId(message.Topic);
            UserId = message.UserId.ToByteArray();
            MessageId = message.MessageId.ToByteArray();
            CreatedAt = message.CreatedAt;
            ExpiresAt = message.ExpiresAt;
            Handle = message.Handle;

            switch (message.Type)
            {
                case 0:
                    Type = TopicMessageType.Chat;
                    break;
                case 1:
                    Type = TopicMessageType.GroupJoin;
                    break;
                case 2:
                    Type = TopicMessageType.GroupAdd;
                    break;
                case 3:
                    Type = TopicMessageType.GroupLeave;
                    break;
                case 4:
                    Type = TopicMessageType.GroupKick;
                    break;
                case 5:
                    Type = TopicMessageType.GroupPromoted;
                    break;
            }

            Data = message.Data.ToByteArray();
        }

        public override string ToString()
        {
            var f = "NTopicMessage(Topic={0},UserId={1},MessageId={2},CreatedAt={3},ExpiresAt={4},Handle={5}," +
                    "Type={6},Data={7})";
            return String.Format(f, Topic.ToString(), UserId, MessageId, CreatedAt, ExpiresAt, Handle,
                    Type, Data);
        }
    }
}
