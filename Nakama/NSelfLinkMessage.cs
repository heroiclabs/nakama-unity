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
using Google.Protobuf;

namespace Nakama
{
    public class NSelfLinkMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NSelfLinkMessage(Envelope envelope)
        {
            payload = envelope;
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public static NSelfLinkMessage Custom(string id)
        {
            var message = new TLink {Custom = id};
            return new NSelfLinkMessage(new Envelope {Link = message});
        }

        public static NSelfLinkMessage Device(string id)
        {
            var message = new TLink {Device = id};
            return new NSelfLinkMessage(new Envelope {Link = message});
        }

        public static NSelfLinkMessage Email(string email, string password)
        {
            var message = new TLink {Email = new AuthenticateRequest.Types.Email
            {
                Email_ = email,
                Password = password
            }};
            return new NSelfLinkMessage(new Envelope {Link = message});
        }

        public static NSelfLinkMessage Facebook(string oauthToken)
        {
            var message = new TLink {Facebook = oauthToken};
            return new NSelfLinkMessage(new Envelope {Link = message});
        }

        public static NSelfLinkMessage GameCenter(string playerId,
                                                  string bundleId,
                                                  int timestamp,
                                                  string salt,
                                                  string signature,
                                                  string publicKeyUrl)
        {
            var message = new TLink {GameCenter = new AuthenticateRequest.Types.GameCenter
            {
                PlayerId = playerId,
                BundleId = bundleId,
                Timestamp = timestamp,
                Salt = salt,
                Signature = signature,
                PublicKeyUrl = publicKeyUrl
            }};
            return new NSelfLinkMessage(new Envelope {Link = message});
        }

        public static NSelfLinkMessage Google(string oauthToken)
        {
            var message = new TLink {Google = oauthToken};
            return new NSelfLinkMessage(new Envelope {Link = message});
        }

        public static NSelfLinkMessage Steam(string sessionToken)
        {
            var message = new TLink {Steam = sessionToken};
            return new NSelfLinkMessage(new Envelope {Link = message});
        }
    }
}
