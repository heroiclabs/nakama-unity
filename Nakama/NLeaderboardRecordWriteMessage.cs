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
    public class NLeaderboardRecordWriteMessage : INCollatedMessage<INResultSet<INLeaderboardRecord>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NLeaderboardRecordWriteMessage()
        {
            payload = new Envelope {LeaderboardRecordsWrite = new TLeaderboardRecordsWrite()};
            payload = new Envelope {LeaderboardRecordsWrite = new TLeaderboardRecordsWrite { Records =
            {
                new List<TLeaderboardRecordsWrite.Types.LeaderboardRecordWrite>()
            }}};  
        }

        private NLeaderboardRecordWriteMessage(string leaderboardId)
        {
            payload = new Envelope {LeaderboardRecordsWrite = new TLeaderboardRecordsWrite { Records =
            {
                new List<TLeaderboardRecordsWrite.Types.LeaderboardRecordWrite>
                {
                    new TLeaderboardRecordsWrite.Types.LeaderboardRecordWrite {LeaderboardId = leaderboardId}
                }
            }}};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NLeaderboardRecordWriteMessage(LeaderboardId={0},Location={1},Timezone={2},Metadata={3},Op={4},Incr={5},Decr={6},Set={7},Best={8})";
            var output = "";
            foreach (var p in payload.LeaderboardRecordsWrite.Records)
            {
                output += String.Format(f, p.LeaderboardId, p.Location, p.Timezone, p.Metadata, p.OpCase, p.Incr, p.Decr, p.Set, p.Best); 
            }
            return output;   
        }

        public class Builder
        {
            private NLeaderboardRecordWriteMessage message;

            public Builder(string leaderboardId)
            {
                message = new NLeaderboardRecordWriteMessage(leaderboardId);
            }

            public Builder Location(string location)
            {
                message.payload.LeaderboardRecordsWrite.Records[0].Location = location;
                return this;
            }

            public Builder Timezone(string timezone)
            {
                message.payload.LeaderboardRecordsWrite.Records[0].Timezone = timezone;
                return this;
            }

            public Builder Metadata(string metadata)
            {
                message.payload.LeaderboardRecordsWrite.Records[0].Metadata = metadata;
                return this;
            }

            public Builder Increment(long value)
            {
                message.payload.LeaderboardRecordsWrite.Records[0].ClearOp();
                message.payload.LeaderboardRecordsWrite.Records[0].Incr = value;
                return this;
            }

            public Builder Decrement(long value)
            {
                message.payload.LeaderboardRecordsWrite.Records[0].ClearOp();
                message.payload.LeaderboardRecordsWrite.Records[0].Decr = value;
                return this;
            }

            public Builder Set(long value)
            {
                message.payload.LeaderboardRecordsWrite.Records[0].ClearOp();
                message.payload.LeaderboardRecordsWrite.Records[0].Set = value;
                return this;
            }

            public Builder Best(long value)
            {
                message.payload.LeaderboardRecordsWrite.Records[0].ClearOp();
                message.payload.LeaderboardRecordsWrite.Records[0].Best = value;
                return this;
            }

            public NLeaderboardRecordWriteMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NLeaderboardRecordWriteMessage();
                message.payload.LeaderboardRecordsWrite = new TLeaderboardRecordsWrite(original.payload.LeaderboardRecordsWrite);
                return original;
            }
        }
    }
}
