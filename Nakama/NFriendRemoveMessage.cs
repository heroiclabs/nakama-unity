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
    public class NFriendRemoveMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NFriendRemoveMessage(byte[] id)
        {
            payload = new Envelope {FriendRemove = new TFriendRemove {UserId = ByteString.CopyFrom(id)}};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            return String.Format("NFriendRemoveMessage(UserId={0})", payload.FriendRemove.UserId);
        }

        public static NFriendRemoveMessage Default(byte[] id)
        {
            return new NFriendRemoveMessage(id);
        }
    }
}
