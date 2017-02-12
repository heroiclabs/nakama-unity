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
    public class NGroupPromoteUserMessage : INMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupPromoteUserMessage(byte[] groupId, byte[] userId)
        {
            payload = new Envelope {GroupUserPromote = new TGroupUserPromote()};
            payload.GroupUserPromote.GroupId = ByteString.CopyFrom(groupId);
            payload.GroupUserPromote.UserId = ByteString.CopyFrom(userId);
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            return String.Format("NGroupPromoteUserMessage(GroupId={0},UserId={1})", payload.GroupUserPromote.GroupId, payload.GroupUserPromote.UserId);
        }

        public static NGroupPromoteUserMessage Default(byte[] groupId, byte[] userId)
        {
            return new NGroupPromoteUserMessage(groupId, userId);
        }
    }
}
