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
using System.Collections.Generic;
using Google.Protobuf;

namespace Nakama
{
    public class NGroupKickUserMessage : INCollatedMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NGroupKickUserMessage(string groupId, string userId)
        {
            payload = new Envelope {GroupUsersKick = new TGroupUsersKick { GroupUsers =
            {
                new List<TGroupUsersKick.Types.GroupUserKick>
                {
                    new TGroupUsersKick.Types.GroupUserKick
                    {
                        UserId = userId,
                        GroupId = groupId
                    }
                }
            }}};   
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var p = payload.GroupUsersKick;
            string output = "";
            foreach (var g in p.GroupUsers)
            {
                output += String.Format("(UserId={0},GroupId={1}),", g.UserId, g.GroupId);
            }
            return String.Format("NGroupKickUserMessage({0})", output);
        }

        public static NGroupKickUserMessage Default(string groupId, string userId)
        {
            return new NGroupKickUserMessage(groupId, userId);
        }
    }
}
