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

namespace Nakama
{
    public class NTopic : INTopic
    {
        public INTopicId Topic { get; private set; }
        public IList<INUserPresence> Presences { get; private set; }
        public INUserPresence Self { get; private set; }

        internal NTopic(TTopic message)
        {
            Topic = new NTopicId(message.Topic);
            Presences = new List<INUserPresence>();
            foreach (var presence in message.Presences)
            {
                Presences.Add(new NUserPresence(presence));
            }
            Self = new NUserPresence(message.Self);
        }

        public override string ToString()
        {
            var f = "NTopic(Topic={0},Presences={1},Self={2})";
            return String.Format(f, Topic, Presences, Self);
        }
    }
}
