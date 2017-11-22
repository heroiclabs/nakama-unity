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
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public long CreatedAt { get; private set; }

        public long ExpiresAt { get; private set; }

        public string Handle { get; private set; }

        public string Id { get; private set; }

        public string Token { get; private set; }
        
        public string UdpToken { get; private set; }

        internal NSession(string token, string udpToken, long createdAt)
        {
            CreatedAt = createdAt;
            Token = token;
            UdpToken = udpToken;

            var decoded = JwtUnpack(Token);

            // Set computed fields
            Handle = decoded.Split('"')[5];
            Id = decoded.Split('"')[9];
            var expiresAt = Convert.ToInt64(decoded.Split('"')[2].TrimStart(':').TrimEnd(','));
            ExpiresAt = Convert.ToInt64(TimeSpan.FromSeconds(expiresAt).TotalMilliseconds);
        }

        public bool HasExpired(DateTime dateTime)
        {
            var expireDate = Epoch + TimeSpan.FromMilliseconds(ExpiresAt);
            return dateTime > expireDate;
        }

        public static INSession Restore(string session)
        {
            string[] sessionParts = session.Split('|');
            string token = sessionParts[0];
            string udpToken = "";
            if (sessionParts.Length >= 2)
            {
                udpToken = sessionParts[1];
            }
            TimeSpan span = DateTime.UtcNow - Epoch;
            return new NSession(token, udpToken, System.Convert.ToInt64(span.TotalMilliseconds));
        }

        public override string ToString()
        {
            return Token + "|" + UdpToken;
        }

        private static string JwtUnpack(string jwt)
        {
            // Hack decode JSON payload from JWT
            var payload = jwt.Split('.')[1];

            var padLength = Math.Ceiling(payload.Length / 4.0) * 4;
            payload = payload.PadRight(Convert.ToInt32(padLength), '=');

            return Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        }
    }
}
