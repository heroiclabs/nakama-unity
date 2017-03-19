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
    public class NLeaderboardsListMessage : INMessage<INResultSet<INLeaderboard>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NLeaderboardsListMessage()
        {
            payload = new Envelope {LeaderboardsList = new TLeaderboardsList()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NLeaderboardsListMessage(Limit={0},Cursor={1})";
            var p = payload.LeaderboardsList;
            return String.Format(f, p.Limit, p.Cursor);
        }

        public class Builder
        {
            private NLeaderboardsListMessage message;

            public Builder()
            {
                message = new NLeaderboardsListMessage();
            }

            public Builder Limit(long limit)
            {
                message.payload.LeaderboardsList.Limit = limit;
                return this;
            }

            public Builder Cursor(INCursor cursor)
            {
                message.payload.LeaderboardsList.Cursor = ByteString.CopyFrom(cursor.Value);
                return this;
            }

            public NLeaderboardsListMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NLeaderboardsListMessage();
                message.payload.LeaderboardsList = new TLeaderboardsList(original.payload.LeaderboardsList);
                return original;
            }
        }
    }
}
