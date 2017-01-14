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
    public class NSelfUnlinkMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NSelfUnlinkMessage(Envelope request)
        {
            payload = request;
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public static NSelfUnlinkMessage Custom(string id)
        {
            var message = new TUnlink {Custom = id};
            return new NSelfUnlinkMessage(new Envelope {Unlink = message});
        }

        public static NSelfUnlinkMessage Device(string id)
        {
            var message = new TUnlink {Device = id};
            return new NSelfUnlinkMessage(new Envelope {Unlink = message});
        }

        public static NSelfUnlinkMessage Email(string email)
        {
            var message = new TUnlink {Email = email};
            return new NSelfUnlinkMessage(new Envelope {Unlink = message});
        }

        public static NSelfUnlinkMessage Facebook(string id)
        {
            var message = new TUnlink {Facebook = id};
            return new NSelfUnlinkMessage(new Envelope {Unlink = message});
        }

        public static NSelfUnlinkMessage GameCenter(string id)
        {
            var message = new TUnlink {GameCenter = id};
            return new NSelfUnlinkMessage(new Envelope {Unlink = message});
        }

        public static NSelfUnlinkMessage Google(string id)
        {
            var message = new TUnlink {Google = id};
            return new NSelfUnlinkMessage(new Envelope {Unlink = message});
        }

        public static NSelfUnlinkMessage Steam(string id)
        {
            var message = new TUnlink {Steam = id};
            return new NSelfUnlinkMessage(new Envelope {Unlink = message});
        }
    }
}
