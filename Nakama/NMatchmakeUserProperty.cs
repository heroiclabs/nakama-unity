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

namespace Nakama
{
    internal class NMatchmakeUserProperty : INMatchmakeUserProperty
    {
        public string Id { get; private set; }

        public IDictionary<string, object> Properties { get; private set; }

        public IDictionary<string, INMatchmakeFilter> Filters { get; private set; }

        internal NMatchmakeUserProperty(MatchmakeMatched.Types.UserProperty message)
        {
            Id = message.UserId;
            Properties = new Dictionary<string, object>();
            Filters = new Dictionary<string, INMatchmakeFilter>();

            foreach (var item in message.Properties)
            {
                switch (item.ValueCase)
                {
                    case PropertyPair.ValueOneofCase.StringSet:
                        var terms = new string[item.StringSet.Values.Count];
                        for (var i = 0; i < item.StringSet.Values.Count; i++)
                        {
                            terms[i] = item.StringSet.Values[i];
                        }
                        Properties.Add(item.Key, terms);
                        break;
                    case PropertyPair.ValueOneofCase.IntValue:
                        Properties.Add(item.Key, item.IntValue);
                        break;
                    case PropertyPair.ValueOneofCase.BoolValue:
                        Properties.Add(item.Key, item.BoolValue);
                        break;
                }
                
            }
            foreach (var item in message.Filters)
            {
                switch (item.ValueCase)
                {
                    case MatchmakeFilter.ValueOneofCase.Term:
                        Filters.Add(item.Name, new NMatchmakeTermFilter(item.Term));
                        break;
                    case MatchmakeFilter.ValueOneofCase.Range:
                        Filters.Add(item.Name, new NMatchmakeRangeFilter(item.Range));
                        break;
                    case MatchmakeFilter.ValueOneofCase.Check:
                        Filters.Add(item.Name, new NMatchmakeCheckFilter(item.Check));
                        break;
                }
            }
        }

        public override string ToString()
        {
            var f = "NMatchmakeUserProperty(Id={0},Properties={1},Filters={2})";
            return String.Format(f, Id, Properties, Filters);
        }
    }
}
