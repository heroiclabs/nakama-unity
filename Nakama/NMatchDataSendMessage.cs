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
    public class NMatchDataSendMessage : INUncollatedMessage
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NMatchDataSendMessage()
        {
            payload = new Envelope {MatchDataSend = new MatchDataSend()};
        }

        private NMatchDataSendMessage(string matchId, long opCode, byte[] data)
        {
            payload = new Envelope {MatchDataSend = new MatchDataSend()};
            payload.MatchDataSend.MatchId = matchId;
            payload.MatchDataSend.OpCode = opCode;
            payload.MatchDataSend.Data = ByteString.CopyFrom(data);
        }

        public override string ToString()
        {
            var f = "NMatchDataSendMessage(MatchId={0},OpCode={1},Data={2},Presences={3})";
            var p = payload.MatchDataSend;
            return String.Format(f, p.MatchId, p.OpCode, p.Data, p.Presences);
        }

        public static NMatchDataSendMessage Default(string matchId, long opCode, byte[] data)
        {
            return new NMatchDataSendMessage(matchId, opCode, data);
        }

        public class Builder
        {
            private NMatchDataSendMessage message;

            public Builder(string matchId, long opCode, byte[] data)
            {
                message = new NMatchDataSendMessage(matchId, opCode, data);
            }

            public Builder MatchId(string matchId)
            {
                message.payload.MatchDataSend.MatchId = matchId;
                return this;
            }

            public Builder OpCode(long opCode)
            {
                message.payload.MatchDataSend.OpCode = opCode;
                return this;
            }

            public Builder Data(byte[] data)
            {
                message.payload.MatchDataSend.Data = ByteString.CopyFrom(data);
                return this;
            }

            public Builder Presences(INUserPresence[] presences)
            {
                message.payload.MatchDataSend.Presences.Clear();
                foreach (var presence in presences)
                {
                    UserPresence userPresence = new UserPresence();
                    userPresence.UserId = presence.UserId;
                    userPresence.SessionId = presence.SessionId;
                    message.payload.MatchDataSend.Presences.Add(userPresence);
                }
                return this;
            }

            public NMatchDataSendMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NMatchDataSendMessage();
                message.payload.MatchDataSend = new MatchDataSend(original.payload.MatchDataSend);
                return original;
            }
        }
    }
}
