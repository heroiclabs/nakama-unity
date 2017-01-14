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

namespace Nakama
{
    public class NAuthenticateMessage : INAuthenticateMessage
    {
        public AuthenticateRequest Payload { get; private set; }

        internal NAuthenticateMessage(AuthenticateRequest payload)
        {
            Payload = payload;
        }

        public static NAuthenticateMessage Custom(string id)
        {
            var payload = new AuthenticateRequest {Custom = id};
            return new NAuthenticateMessage(payload);
        }

        public static NAuthenticateMessage Device(string id)
        {
            var payload = new AuthenticateRequest {Device = id};
            return new NAuthenticateMessage(payload);
        }

        public static NAuthenticateMessage Email(string email, string password)
        {
            var payload = new AuthenticateRequest {Email = new AuthenticateRequest.Types.Email
            {
                Email_ = email,
                Password = password
            }};
            return new NAuthenticateMessage(payload);
        }

        public static NAuthenticateMessage Facebook(string oauthToken)
        {
            var payload = new AuthenticateRequest {Facebook = oauthToken};
            return new NAuthenticateMessage(payload);
        }

        public static NAuthenticateMessage GameCenter(string playerId,
                                                      string bundleId,
                                                      int timestamp,
                                                      string salt,
                                                      string signature,
                                                      string publicKeyUrl)
        {
            var payload = new AuthenticateRequest { GameCenter = new AuthenticateRequest.Types.GameCenter
            {
                PlayerId = playerId,
                BundleId = bundleId,
                Timestamp = timestamp,
                Salt = salt,
                Signature = signature,
                PublicKeyUrl = publicKeyUrl
            }};
            return new NAuthenticateMessage(payload);
        }

        public static NAuthenticateMessage Google(string oauthToken)
        {
            var payload = new AuthenticateRequest {Google = oauthToken};
            return new NAuthenticateMessage(payload);
        }

        public static NAuthenticateMessage Steam(string sessionToken)
        {
            var payload = new AuthenticateRequest {Steam = sessionToken};
            return new NAuthenticateMessage(payload);
        }
    }
}
