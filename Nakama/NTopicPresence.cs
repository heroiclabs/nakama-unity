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
    public class NTopicPresence : INTopicPresence
    {
        public INTopicId Topic { get; private set; }

        public IList<INUserPresence> Join { get; private set; }

        public IList<INUserPresence> Leave { get; private set; }

        internal NTopicPresence(TopicPresence message)
        {
            Topic = new NTopicId(message.Topic);
            Join = new List<INUserPresence>();
            Leave = new List<INUserPresence>();

            foreach (var item in message.Joins)
            {
                Join.Add(new NUserPresence(item));
            }
            foreach (var item in message.Leaves)
            {
                Leave.Add(new NUserPresence(item));
            }
        }

        public override string ToString()
        {
            var f = "NTopicPresence(Topic={0},Join={1},Leave={2})";
            return String.Format(f, Topic, Join, Leave);
        }
    }
}
