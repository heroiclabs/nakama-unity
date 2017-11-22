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
    public class NMatchmakeAddMessage : INCollatedMessage<INMatchmakeTicket>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NMatchmakeAddMessage()
        {
            payload = new Envelope {MatchmakeAdd = new TMatchmakeAdd()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NMatchmakeAddMessage(RequiredCount={0}, Filters={1}, Properties={2})";
            return String.Format(f, payload.MatchmakeAdd.RequiredCount, payload.MatchmakeAdd.Filters, payload.MatchmakeAdd.Properties);
        }

        public static NMatchmakeAddMessage Default(long requiredCount)
        {
            return new Builder(requiredCount).Build();
        }
        
        public class Builder
        {
            private NMatchmakeAddMessage message;

            public Builder(long requiredCount)
            {
                message = new NMatchmakeAddMessage();
                message.payload.MatchmakeAdd.RequiredCount = requiredCount;
            }

            public Builder AddTermFilter(string name, HashSet<string> terms, bool matchAllTerms)
            {
                var f = new MatchmakeFilter()
                {
                    Name = name,
                    Term = new MatchmakeFilter.Types.TermFilter()
                    {
                        MatchAllTerms = matchAllTerms
                    }
                };
                f.Term.Terms.AddRange(terms);
                
                message.payload.MatchmakeAdd.Filters.Add(f);
                return this;
            }
            
            public Builder AddRangeFilter(string name, long lowerbound, long upperbound)
            {
                var f = new MatchmakeFilter()
                {
                    Name = name,
                    Range = new MatchmakeFilter.Types.RangeFilter()
                    {
                        LowerBound = lowerbound,
                        UpperBound = upperbound
                    }
                };
                message.payload.MatchmakeAdd.Filters.Add(f);
                return this;
            }
            
            public Builder AddCheckFilter(string name, bool value)
            {
                var f = new MatchmakeFilter()
                {
                    Name = name,
                    Check = value
                };
                message.payload.MatchmakeAdd.Filters.Add(f);
                return this;
            }
            
            public Builder AddProperty(string key, bool value)
            {
                var property = new PropertyPair()
                {
                    Key = key,
                    BoolValue = value
                };
                message.payload.MatchmakeAdd.Properties.Add(property);
                return this;
            }
            
            public Builder AddProperty(string key, long value)
            {
                var property = new PropertyPair()
                {
                    Key = key,
                    IntValue = value
                };
                message.payload.MatchmakeAdd.Properties.Add(property);
                return this;
            }
            
            public Builder AddProperty(string key, HashSet<string> values)
            {
                var property = new PropertyPair()
                {
                    Key = key,
                    StringSet = new PropertyPair.Types.StringSet()
                };
                property.StringSet.Values.AddRange(values);
                
                message.payload.MatchmakeAdd.Properties.Add(property);
                return this;
            }

            public NMatchmakeAddMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NMatchmakeAddMessage();
                message.payload.MatchmakeAdd = new TMatchmakeAdd(original.payload.MatchmakeAdd);
                return original;
            }
        }
    }
}
