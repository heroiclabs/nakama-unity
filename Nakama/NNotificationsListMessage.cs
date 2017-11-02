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
    public class NNotificationsListMessage : INCollatedMessage<INResultSet<INNotification>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NNotificationsListMessage()
        {
            payload = new Envelope {NotificationsList = new TNotificationsList()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NNotificationsListMessage(Limit={0},ResumableCursor={1})";
            var p = payload.NotificationsList;
            return String.Format(f, p.Limit, p.ResumableCursor);
        }

        public static NNotificationsListMessage Default(long limit)
        {
            return new Builder(limit).Build();
        }
        
        public class Builder
        {
            private NNotificationsListMessage message;

            public Builder(long limit)
            {
                message = new NNotificationsListMessage();
                message.payload.NotificationsList.Limit = limit;
            }

            public Builder Cursor(string resumableCursor)
            {
                message.payload.NotificationsList.ResumableCursor = resumableCursor;
                return this;
            }

            public NNotificationsListMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NNotificationsListMessage();
                message.payload.NotificationsList = new TNotificationsList(original.payload.NotificationsList);
                return original;
            }
        }
    }
}