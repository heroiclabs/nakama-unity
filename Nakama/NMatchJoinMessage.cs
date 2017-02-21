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
    public class NMatchJoinMessage : INMessage<INMatch>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NMatchJoinMessage(byte[] matchId)
        {
            payload = new Envelope {MatchJoin = new TMatchJoin()};
            payload.MatchJoin.MatchId = ByteString.CopyFrom(matchId);
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NMatchJoinMessage(MatchId={0})";
            return String.Format(f, payload.MatchJoin.MatchId);
        }

        public static NMatchJoinMessage Default(byte[] matchId)
        {
            return new NMatchJoinMessage(matchId);
        }
    }
}
