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
    internal class NMatch : INMatch
    {
        public byte[] Id { get; private set; }
        public IList<INUserPresence> Presence { get; private set; }
        public INUserPresence Self { get; private set; }

        internal NMatch(TMatch message)
        {
            Id = message.MatchId.ToByteArray();
            Presence = new List<INUserPresence>();
            foreach (var item in message.Presences)
            {
                Presence.Add(new NUserPresence(item));
            }
            Self = new NUserPresence(message.Self);
        }

        public override string ToString()
        {
            var f = "NMatch(Id={0},Presence={1},Self={2})";
            return String.Format(f, Id, Presence, Self);
        }
    }
}
