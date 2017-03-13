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
    public class NLeaderboardRecordsFetchMessage : INMessage<INResultSet<INLeaderboardRecord>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NLeaderboardRecordsFetchMessage()
        {
            payload = new Envelope {LeaderboardRecordsFetch = new TLeaderboardRecordsFetch()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var p = payload.LeaderboardRecordsFetch;
            string output = "";
            foreach (var id in p.LeaderboardIds)
            {
                output += id.ToBase64() + ",";
            }
            return String.Format("NLeaderboardRecordsFetchMessage(LeaderboardIds={0},Limit={1},Cursor={2})", output, p.Limit, p.Cursor);
        }

        public class Builder
        {
            private NLeaderboardRecordsFetchMessage message;

            public Builder(byte[] leaderboardId)
            {
                message = new NLeaderboardRecordsFetchMessage();
                message.payload.LeaderboardRecordsFetch.LeaderboardIds.Add(ByteString.CopyFrom(leaderboardId));
            }

            public Builder Fetch(byte[] leaderboardId)
            {
                message.payload.LeaderboardRecordsFetch.LeaderboardIds.Add(ByteString.CopyFrom(leaderboardId));
                return this;
            }

            public Builder Limit(long limit)
            {
                message.payload.LeaderboardRecordsFetch.Limit = limit;
                return this;
            }

            public Builder Cursor(INCursor cursor)
            {
                message.payload.LeaderboardRecordsFetch.Cursor = ByteString.CopyFrom(cursor.Value);
                return this;
            }

            public NLeaderboardRecordsFetchMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NLeaderboardRecordsFetchMessage();
                message.payload.LeaderboardRecordsFetch = new TLeaderboardRecordsFetch(original.payload.LeaderboardRecordsFetch);
                return original;
            }
        }
    }
}
