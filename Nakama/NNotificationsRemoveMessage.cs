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
using System.Collections.Generic;
using Google.Protobuf;

namespace Nakama
{
    public class NNotificationsRemoveMessage : INCollatedMessage<bool>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NNotificationsRemoveMessage()
        {
            payload = new Envelope {NotificationsRemove = new TNotificationsRemove()};
        }
        
        private NNotificationsRemoveMessage(TNotificationsRemove notificationsRemove)
        {
            payload = new Envelope {NotificationsRemove = notificationsRemove};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {   
            var output = "";
            foreach (var id in payload.NotificationsRemove.NotificationIds)
            {
                output += id + ", ";
            }
            return String.Format("NNotificationsRemoveMessage(NotificationIds={0})", output);
        }

        public static NNotificationsRemoveMessage Default(params string[] notificationIds)
        {
            var message = new NNotificationsRemoveMessage();
            message.payload.NotificationsRemove.NotificationIds.Add(notificationIds);   

            return message;
        }
        
        public static NNotificationsRemoveMessage Default(IEnumerable<string> notificationIds)
        {
            var message = new NNotificationsRemoveMessage();
            message.payload.NotificationsRemove.NotificationIds.Add(notificationIds);   

            return message;
        }
    }
}