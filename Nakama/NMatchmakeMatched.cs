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
    internal class NMatchmakeMatched : INMatchmakeMatched
    {
        public INMatchmakeTicket Ticket { get; private set; }
        public INMatchToken Token { get; private set; }
        public IList<INUserPresence> Presence { get; private set; }
        public INUserPresence Self { get; private set; }
        public IList<INMatchmakeUserProperty> UserProperties { get; private set; }

        internal NMatchmakeMatched(MatchmakeMatched message)
        {
            Ticket = new NMatchmakeTicket(message.Ticket);
            Token = new NMatchToken(message.Token);
            Presence = new List<INUserPresence>();
            foreach (var item in message.Presences)
            {
                Presence.Add(new NUserPresence(item));
            }
            Self = new NUserPresence(message.Self);
            UserProperties = new List<INMatchmakeUserProperty>();
            foreach (var item in message.Properties)
            {
                UserProperties.Add(new NMatchmakeUserProperty(item));
            }
        }

        public override string ToString()
        {
            var f = "NMatchmakeMatched(Ticket={0},Token={1},Presence={2},Self={3},UserProperties={4})";
            return String.Format(f, Ticket, Token, Presence, Self, UserProperties);
        }
    }
}
