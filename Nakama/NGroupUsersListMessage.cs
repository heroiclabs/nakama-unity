﻿/**
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
    public class NGroupUsersListMessage : INCollatedMessage<INResultSet<INGroupUser>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupUsersListMessage(string groupId)
        {
            payload = new Envelope {GroupUsersList = new TGroupUsersList()};
            payload.GroupUsersList.GroupId = groupId;
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            return String.Format("NGroupUsersListMessage(GroupId={0})", payload.GroupUsersList.GroupId);
        }

        public static NGroupUsersListMessage Default(string groupId)
        {
            return new NGroupUsersListMessage(groupId);
        }
    }
}
