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
    public class NRuntimeRpcMessage : INCollatedMessage<INRuntimeRpc>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NRuntimeRpcMessage(string id)
        {
            var request = new TRpc();
            request.Id = id;
            payload = new Envelope {Rpc = request};
        }

        private NRuntimeRpcMessage()
        {
            payload = new Envelope {Rpc = new TRpc()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NRuntimeRpcMessage(Id={0},Payload={1})";
            var p = payload.Rpc;
            return String.Format(f, p.Id, p.Payload);
        }

        public class Builder
        {
            private NRuntimeRpcMessage message;

            public Builder(string id)
            {
                message = new NRuntimeRpcMessage(id);
            }

            public Builder Payload(string payload)
            {
                message.payload.Rpc.Payload = payload;
                return this;
            }

            public NRuntimeRpcMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NRuntimeRpcMessage();
                message.payload.Rpc = new TRpc(original.payload.Rpc);
                return original;
            }
        }
    }
}
