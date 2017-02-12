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
    public class NGroupsListMessage : INMessage<INResultSet<INGroup>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupsListMessage()
        {
            payload = new Envelope {GroupsList = new TGroupsList()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NGroupsListMessage(PageLimit={0},OrderByAsc={1},Cursor={2},Lang={3},CreatedAt={4},Count={5},FilterCase={6})";
            var p = payload.GroupsList;
            return String.Format(f, p.PageLimit, p.OrderByAsc, p.Cursor, p.Lang, p.CreatedAt, p.Count, p.FilterCase);
        }

        public class Builder
        {
            private NGroupsListMessage message;

            public Builder()
            {
                message = new NGroupsListMessage();
            }

            public Builder PageLimit(long pageLimit)
            {
                message.payload.GroupsList.PageLimit = pageLimit;
                return this;
            }

            public Builder OrderByAsc(bool orderByAsc)
            {
                message.payload.GroupsList.OrderByAsc = orderByAsc;
                return this;
            }

            public Builder Cursor(INCursor cursor)
            {
                message.payload.GroupsList.Cursor = ByteString.CopyFrom(cursor.Value);
                return this;
            }

            public Builder FilterByLang(string lang)
            {
                message.payload.GroupsList.ClearFilter();
                message.payload.GroupsList.Lang = lang;
                return this;
            }

            public Builder FilterByCreatedAt(long createdAt)
            {
                message.payload.GroupsList.ClearFilter();
                message.payload.GroupsList.CreatedAt = createdAt;
                return this;
            }

            public Builder FilterByCount(long count)
            {
                message.payload.GroupsList.ClearFilter();
                message.payload.GroupsList.Count = count;
                return this;
            }

            public NGroupsListMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NGroupsListMessage();
                message.payload.GroupsList = new TGroupsList(original.payload.GroupsList);
                return original;
            }
        }
    }
}
