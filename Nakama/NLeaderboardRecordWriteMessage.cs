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
using System.Text;
using Google.Protobuf;

namespace Nakama
{
    public class NLeaderboardRecordWriteMessage : INMessage<INLeaderboardRecord>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NLeaderboardRecordWriteMessage()
        {
            payload = new Envelope {LeaderboardRecordWrite = new TLeaderboardRecordWrite()};
        }

        private NLeaderboardRecordWriteMessage(byte[] leaderboardId)
        {
            var request = new TLeaderboardRecordWrite();
            request.LeaderboardId = ByteString.CopyFrom(leaderboardId);
            payload = new Envelope {LeaderboardRecordWrite = request};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NLeaderboardRecordWriteMessage(LeaderboardId={0},Location={1},Timezone={2},Metadata={3},Op={4},Incr={5},Decr={6},Set={7},Best={8})";
            var p = payload.LeaderboardRecordWrite;
            return String.Format(f, p.LeaderboardId, p.Location, p.Timezone, p.Metadata, p.OpCase, p.Incr, p.Decr, p.Set, p.Best);
        }

        public class Builder
        {
            private NLeaderboardRecordWriteMessage message;

            public Builder(byte[] leaderboardId)
            {
                message = new NLeaderboardRecordWriteMessage(leaderboardId);
            }

            public Builder Location(string location)
            {
                message.payload.LeaderboardRecordWrite.Location = location;
                return this;
            }

            public Builder Timezone(string timezone)
            {
                message.payload.LeaderboardRecordWrite.Timezone = timezone;
                return this;
            }

            public Builder Metadata(byte[] metadata)
            {
                message.payload.LeaderboardRecordWrite.Metadata = ByteString.CopyFrom(metadata);
                return this;
            }

            public Builder Increment(long value)
            {
                message.payload.LeaderboardRecordWrite.ClearOp();
                message.payload.LeaderboardRecordWrite.Incr = value;
                return this;
            }

            public Builder Decrement(long value)
            {
                message.payload.LeaderboardRecordWrite.ClearOp();
                message.payload.LeaderboardRecordWrite.Decr = value;
                return this;
            }

            public Builder Set(long value)
            {
                message.payload.LeaderboardRecordWrite.ClearOp();
                message.payload.LeaderboardRecordWrite.Set = value;
                return this;
            }

            public Builder Best(long value)
            {
                message.payload.LeaderboardRecordWrite.ClearOp();
                message.payload.LeaderboardRecordWrite.Best = value;
                return this;
            }

            public NLeaderboardRecordWriteMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NLeaderboardRecordWriteMessage();
                message.payload.LeaderboardRecordWrite = new TLeaderboardRecordWrite(original.payload.LeaderboardRecordWrite);
                return original;
            }
        }
    }
}
