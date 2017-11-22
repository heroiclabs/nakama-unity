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

namespace Nakama
{
    internal class NMatchmakeTermFilter : INMatchmakeTermFilter
    {
        public string[] Terms { get; private set; }
        public bool MatchAllTerms { get; private set; }
        
        internal NMatchmakeTermFilter(MatchmakeFilter.Types.TermFilter filter)
        {
            MatchAllTerms = filter.MatchAllTerms;
            Terms = new string[filter.Terms.Count];
            for (int i = 0; i < filter.Terms.Count; i++)
            {
                Terms[i] = filter.Terms[i];
            }
        }
        
        public override string ToString()
        {
            var f = "NMatchmakeTermFilter(Terms={0}, AllTerms={1})";
            return String.Format(f, Terms, MatchAllTerms);
        }
    }
    
    internal class NMatchmakeRangeFilter : INMatchmakeRangeFilter
    {
        public long Lowerbound { get; private set; }
        public long Upperbound { get; private set; }
        
        internal NMatchmakeRangeFilter(MatchmakeFilter.Types.RangeFilter filter)
        {
            Lowerbound = filter.LowerBound;
            Upperbound = filter.UpperBound;
        }
        
        public override string ToString()
        {
            var f = "NMatchmakeRangeFilter(Lowerbound={0}, Upperbound={1})";
            return String.Format(f, Lowerbound, Upperbound);
        }
    }
    
    internal class NMatchmakeCheckFilter : INMatchmakeCheckFilter
    {
        public bool Value { get; private set; }
        
        internal NMatchmakeCheckFilter(bool filter)
        {
            Value = filter;
        }
        
        public override string ToString()
        {
            var f = "NMatchmakeCheckFilter(Value={0})";
            return String.Format(f, Value);
        }
    }
}