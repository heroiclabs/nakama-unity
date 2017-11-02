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
    public class NMatchLeaveMessage : INCollatedMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NMatchLeaveMessage(string matchId)
        {
            payload = new Envelope {MatchesLeave = new TMatchesLeave { MatchIds =
            {
                new List<string>{ matchId }
            }}};   
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var output = "";
            foreach (var id in payload.MatchesLeave.MatchIds)
            {
                output += id + ", ";
            }
            return String.Format("NMatchLeaveMessage(GroupIds={0})", output);   
        }

        public static NMatchLeaveMessage Default(string matchId)
        {
            return new NMatchLeaveMessage(matchId);
        }
    }
}
