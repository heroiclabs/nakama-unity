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

namespace Nakama
{
    internal class NNotification : INNotification
    {
        public string Id { get; private set; }
        public string Subject { get; private set; }
        public string Content { get; private set; }
        public long Code { get; private set; }
        public string SenderId { get; private set; }
        public long CreatedAt { get; private set; }
        public long ExpiresAt { get; private set; }
        public bool Persistent { get; private set; }
        internal NNotification(Notification n)
        {
            Id = n.Id;
            Subject = n.Subject;
            Content = n.Content;
            Code = n.Code;
            SenderId = n.SenderId;
            CreatedAt = n.CreatedAt;
            ExpiresAt = n.ExpiresAt;
            Persistent = n.Persistent;
        }
        
        public override string ToString()
        {
            var f = "NNotification(Id={0},Subject={1},Content={2},Code={3},SenderId={4},CreatedAt={5},ExpiresAt={6},Persistent={7})";
            return String.Format(f, Id, Subject, Content, Code, SenderId, CreatedAt, ExpiresAt, Persistent);
        }
    }
}