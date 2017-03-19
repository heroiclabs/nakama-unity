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
    public class NLeaderboardRecord : INLeaderboardRecord
    {
        public byte[] LeaderboardId { get; private set; }
        public byte[] OwnerId { get; private set; }
        public string Handle { get; private set; }
        public string Lang { get; private set; }
        public string Location { get; private set; }
        public string Timezone { get; private set; }
        public long Rank { get; private set; }
        public long Score { get; private set; }
        public long NumScore { get; private set; }
        public byte[] Metadata { get; private set; }
        public long RankedAt { get; private set; }
        public long UpdatedAt { get; private set; }
        public long ExpiresAt { get; private set; }

        internal NLeaderboardRecord(LeaderboardRecord message)
        {
            LeaderboardId = message.LeaderboardId.ToByteArray();
            OwnerId = message.OwnerId.ToByteArray();
            Handle = message.Handle;
            Lang = message.Lang;
            Location = message.Location;
            Timezone = message.Timezone;
            Rank = message.Rank;
            Score = message.Score;
            NumScore = message.NumScore;
            Metadata = message.Metadata.ToByteArray();
            RankedAt = message.RankedAt;
            UpdatedAt = message.UpdatedAt;
            ExpiresAt = message.ExpiresAt;
        }

        public override string ToString()
        {
            var f = "NLeaderboardRecord(LeaderboardId={0},OwnerId={1},Handle={2},Lang={3},Location={4},Timezone={5}," +
                    "Rank={6},Score={7},NumScore={8},Metadata={9},RankedAt={10},UpdatedAt={11},ExpiresAt={12})";
            return String.Format(f, LeaderboardId, OwnerId, Handle, Lang, Location, Timezone,
                Rank, Score, NumScore, Metadata, RankedAt, UpdatedAt, ExpiresAt);
        }
    }
}
