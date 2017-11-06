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
    public class NMatchJoinMessage : INCollatedMessage<INResultSet<INMatch>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NMatchJoinMessage(string matchId)
        {
            payload = new Envelope {MatchesJoin = new TMatchesJoin { Matches =
            {
                new List<TMatchesJoin.Types.MatchJoin>
                {
                    new TMatchesJoin.Types.MatchJoin{ MatchId = matchId }
                }
            }}};   
        }

        private NMatchJoinMessage(INMatchToken token)
        {
            payload = new Envelope {MatchesJoin = new TMatchesJoin { Matches =
            {
                new List<TMatchesJoin.Types.MatchJoin>
                {
                    new TMatchesJoin.Types.MatchJoin{ Token = token.Token }
                }
            }}};   
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NMatchJoinMessage(MatchId={0},Token={1})";
            return String.Format(f, payload.MatchesJoin.Matches[0].MatchId, payload.MatchesJoin.Matches[0].Token);
        }

        public static NMatchJoinMessage Default(string matchId)
        {
            return new NMatchJoinMessage(matchId);
        }

        public static NMatchJoinMessage Default(INMatchToken token)
        {
            return new NMatchJoinMessage(token);
        }
    }
}
