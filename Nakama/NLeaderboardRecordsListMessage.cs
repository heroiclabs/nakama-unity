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
using System.Text;
using Google.Protobuf;

namespace Nakama
{
    public class NLeaderboardRecordsListMessage : INMessage<INResultSet<INLeaderboardRecord>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NLeaderboardRecordsListMessage()
        {
            payload = new Envelope {LeaderboardRecordsList = new TLeaderboardRecordsList()};
        }

        private NLeaderboardRecordsListMessage(byte[] leaderboardId)
        {
            var request = new TLeaderboardRecordsList();
            request.LeaderboardId = ByteString.CopyFrom(leaderboardId);
            payload = new Envelope {LeaderboardRecordsList = request};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NLeaderboardRecordsListMessage(LeaderboardId={0},Limit={1},Cursor={2},Filter={3},OwnerId={4},OwnerIdsCount={5},Lang={6},Location={7},Timezone={8})";
            var p = payload.LeaderboardRecordsList;
            return String.Format(f, p.LeaderboardId, p.Limit, p.Limit, p.FilterCase, p.OwnerId, p.OwnerIds.OwnerIds.Count, p.Lang, p.Location, p.Timezone);
        }

        public class Builder
        {
            private NLeaderboardRecordsListMessage message;

            public Builder(byte[] leaderboardId)
            {
                message = new NLeaderboardRecordsListMessage(leaderboardId);
            }

            public Builder FilterByPagingToOwnerId(byte[] ownerId)
            {
                message.payload.LeaderboardRecordsList.ClearFilter();
                message.payload.LeaderboardRecordsList.OwnerId = ByteString.CopyFrom(ownerId);
                return this;
            }

            public Builder FilterByOwnerIds(IList<byte[]> ownerIds)
            {
                message.payload.LeaderboardRecordsList.ClearFilter();
                foreach (var id in ownerIds)
                {
                    message.payload.LeaderboardRecordsList.OwnerIds.OwnerIds.Add(ByteString.CopyFrom(id));
                }
                return this;
            }

            public Builder FilterByLang(string lang)
            {
                message.payload.LeaderboardRecordsList.ClearFilter();
                message.payload.LeaderboardRecordsList.Lang = lang;
                return this;
            }

            public Builder FilterByLocation(string location)
            {
                message.payload.LeaderboardRecordsList.ClearFilter();
                message.payload.LeaderboardRecordsList.Location = location;
                return this;
            }

            public Builder FilterByTimezone(string timezone)
            {
                message.payload.LeaderboardRecordsList.ClearFilter();
                message.payload.LeaderboardRecordsList.Timezone = timezone;
                return this;
            }

            public Builder Limit(long limit)
            {
                message.payload.LeaderboardRecordsList.Limit = limit;
                return this;
            }

            public Builder Cursor(INCursor cursor)
            {
                message.payload.LeaderboardRecordsList.Cursor = ByteString.CopyFrom(cursor.Value);
                return this;
            }

            public NLeaderboardRecordsListMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NLeaderboardRecordsListMessage();
                message.payload.LeaderboardRecordsList = new TLeaderboardRecordsList(original.payload.LeaderboardRecordsList);
                return original;
            }
        }
    }
}
