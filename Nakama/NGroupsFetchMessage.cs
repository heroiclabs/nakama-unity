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
    public class NGroupsFetchMessage : INCollatedMessage<INResultSet<INGroup>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupsFetchMessage()
        {
            payload = new Envelope {GroupsFetch = new TGroupsFetch()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var p = payload.GroupsFetch;
            string ids = "";
            string names = "";
            foreach (var g in p.Groups)
            {
                switch (g.IdCase)
                {
                    case TGroupsFetch.Types.GroupFetch.IdOneofCase.Name:
                        names += "name=" + g.Name + ",";
                        break;
                    case TGroupsFetch.Types.GroupFetch.IdOneofCase.GroupId:
                        ids += "id=" + g.GroupId + ",";
                        break;
                }
            }
            return String.Format("NGroupsFetchMessage(GroupIds={0}, Names={1})", ids, names);
        }

        public static NGroupsFetchMessage Default(params string[] ids)
        {
            return new NGroupsFetchMessage.Builder().SetGroupIds(ids).Build();
        }

        public class Builder
        {
            private NGroupsFetchMessage message;

            public static Builder ById(params string[] ids)
            {
                return new Builder().SetGroupIds(ids);
            }

            public static Builder ByName(params string[] names)
            {
                return new Builder().SetNames(names);
            }

            public Builder()
            {
                message = new NGroupsFetchMessage();
            }

            public Builder SetGroupIds(params string[] ids)
            {
                return SetGroupIds(new List<string>(ids));
            }

            public Builder SetGroupIds(IEnumerable<string> ids)
            {
                var groups = new List<TGroupsFetch.Types.GroupFetch>();
                foreach (var id in ids)
                {
                    var g = new TGroupsFetch.Types.GroupFetch();
                    g.GroupId = id;
                    groups.Add(g);
                }
                message.payload = new Envelope {GroupsFetch = new TGroupsFetch { Groups = {groups}}}; 
                return this;
            }

            public Builder SetNames(params string[] names)
            {
                return SetNames(new List<string>(names));
            }

            public Builder SetNames(IEnumerable<string> names)
            {
                var groups = new List<TGroupsFetch.Types.GroupFetch>();
                foreach (var name in names)
                {
                    var g = new TGroupsFetch.Types.GroupFetch();
                    g.Name = name;
                    groups.Add(g);
                }
                message.payload = new Envelope {GroupsFetch = new TGroupsFetch { Groups = {groups}}};
                return this;
            }

            public NGroupsFetchMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NGroupsFetchMessage();
                message.payload.GroupsFetch = new TGroupsFetch(original.payload.GroupsFetch);
                return original;
            }
        }
    }
}
