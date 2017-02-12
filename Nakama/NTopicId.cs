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
    public class NTopicId : INTopicId
    {
        public byte[] Id { get; private set; }

        public TopicType Type { get; private set; }

        internal NTopicId(TopicId message)
        {
            switch (message.IdCase)
            {
                case TopicId.IdOneofCase.Dm:
                    Id = message.Dm.ToByteArray();
                    Type = TopicType.DirectMessage;
                    break;
                case TopicId.IdOneofCase.Room:
                    Id = message.Room.ToByteArray();
                    Type = TopicType.Room;
                    break;
                case TopicId.IdOneofCase.GroupId:
                    Id = message.GroupId.ToByteArray();
                    Type = TopicType.Group;
                    break;
                default:
                    // TODO log a warning?
                    break;
            }
        }

        public override string ToString()
        {
            var f = "NTopicId(Id={0},Type={1})";
            return String.Format(f, Id, Type);
        }
    }
}
