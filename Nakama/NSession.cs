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

namespace Nakama
{
    public class NSession : INSession
    {
        public long CreatedAt { get; private set; }

        public byte[] Id {
            get {
                // Hack decode JSON payload from JWT
                var payload = Token.Split('.')[1];

                var padLength = Math.Ceiling(payload.Length / 4.0) * 4;
                payload = payload.PadRight(Convert.ToInt32(padLength), '=');

                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var guid = new Guid(decoded.Split('"')[9]).ToByteArray();
                // NOTE http://stackoverflow.com/a/16722909
                Array.Reverse(guid, 6, 2);
                Array.Reverse(guid, 4, 2);
                Array.Reverse(guid, 0, 4);
                return guid;
            }
        }

        public string Token { get; private set; }

        internal NSession(string token, long createdAt)
        {
            CreatedAt = createdAt;
            Token = token;
        }

        public static INSession Restore(string token)
        {
            TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new NSession(token, System.Convert.ToInt64(span.TotalMilliseconds));
        }

        public override string ToString()
        {
            return String.Format("NSession(CreatedAt={0},Token={1})", CreatedAt, Token);
        }
    }
}
