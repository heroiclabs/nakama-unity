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
    public class NTopicMessageAck : INTopicMessageAck
    {
        public byte[] MessageId { get; private set; }
        public long CreatedAt { get; private set; }
        public long ExpiresAt { get; private set; }
        public string Handle { get; private set; }

        internal NTopicMessageAck(TTopicMessageAck message)
        {
            MessageId = message.MessageId.ToByteArray();
            CreatedAt = message.CreatedAt;
            ExpiresAt = message.ExpiresAt;
            Handle = message.Handle;
        }

        public override string ToString()
        {
            var f = "NTopicMessageAck(MessageId={0},CreatedAt={1},ExpiresAt={2},Handle={3})";
            return String.Format(f, MessageId, CreatedAt, ExpiresAt, Handle);
        }
    }
}
