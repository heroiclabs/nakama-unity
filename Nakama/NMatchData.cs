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
    internal class NMatchData : INMatchData
    {
        public byte[] Data { get; private set; }

        public string Id { get; private set; }

        public long OpCode { get; private set; }

        public INUserPresence Presence { get; private set; }

        internal NMatchData(MatchData message)
        {
            Data = message.Data.ToByteArray();
            Id = message.MatchId;
            OpCode = message.OpCode;
            Presence = new NUserPresence(message.Presence);
        }

        public override string ToString()
        {
            var f = "NMatchData(Data={0},Id={1},OpCode={2},Presence={3})";
            return String.Format(f, Data, Id, OpCode, Presence);
        }
    }
}
