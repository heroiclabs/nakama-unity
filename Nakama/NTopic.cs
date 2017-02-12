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
    public class NTopic : INTopic
    {
        public byte[] Id { get; private set; }
        public TopicType Type { get; private set; }

        internal NTopic(Topic message)
        {
            switch (message.IdCase)
            {
                case Topic.IdOneofCase.Dm:
                    Id = message.Id.ToByteArray();
                    TopicType = TopicType.DirectMessage;
                    break;
                case Topic.IdOneofCase.Room:
                    Id = message.Id.ToByteArray();
                    TopicType = TopicType.Room;
                    break;
                case Topic.IdOneofCase.Group:
                    Id = message.Id.ToByteArray();
                    TopicType = TopicType.Group;
                    break;
                default:
                    // TODO log a warning?
                    break;
            }
        }

        public override string ToString()
        {
            var f = "NTopic(Id={0},Type={1})";
            return String.Format(f, Id, Type);
        }
    }
}
