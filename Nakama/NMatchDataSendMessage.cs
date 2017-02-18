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
    public class NMatchDataSendMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NMatchDataSendMessage(byte[] matchId, long opCode, byte[] data)
        {
            payload = new Envelope {MatchDataSend = new TMatchDataSend()};
            payload.MatchDataSend.MatchId = ByteString.CopyFrom(matchId);
            payload.MatchDataSend.OpCode = opCode;
            payload.MatchDataSend.Data = ByteString.CopyFrom(data);
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NMatchDataSendMessage(MatchId={0},OpCode={1},Data={2})";
            var p = payload.MatchDataSend;
            return String.Format(f, p.MatchId, p.OpCode, p.Data);
        }

        public static NMatchDataSendMessage Default(byte[] matchId, long opCode, byte[] data)
        {
            return new NMatchDataSendMessage(matchId, opCode, data);
        }
    }
}
