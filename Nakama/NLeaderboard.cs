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
    public class NLeaderboard : INLeaderboard
    {
        public string Id { get; private set; }
        public bool Authoritative { get; private set; }
        public long Sort { get; private set; }
        public long Count { get; private set; }
        public string ResetSchedule { get; private set; }
        public string Metadata { get; private set; }
        public string NextId { get; private set; }
        public string PrevId { get; private set; }

        internal NLeaderboard(Leaderboard message)
        {
            Id = message.Id;
            Authoritative = message.Authoritative;
            Sort = message.Sort;
            Count = message.Count;
            ResetSchedule = message.ResetSchedule;
            Metadata = message.Metadata;
            NextId = message.NextId;
            PrevId = message.PrevId;
        }

        public override string ToString()
        {
            var f = "NLeaderboard(Id={0},Authoritative={1},Sort={2},Count={3},ResetSchedule={4},Metadata={5},NextId={6},PrevId={7})";
            return String.Format(f, Id, Authoritative, Sort, Count, ResetSchedule, Metadata, NextId, PrevId);
        }
    }
}
