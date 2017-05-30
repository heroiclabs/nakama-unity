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
    public class NUsersFetchMessage : INCollatedMessage<INResultSet<INUser>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NUsersFetchMessage(params byte[] ids)
        {
            var request = new TUsersFetch();
            request.UserIds = new TUsersFetch.Types.UserIds();
            request.UserIds.UserIds_.Add(ByteString.CopyFrom(ids));
            payload = new Envelope {UsersFetch = request};
        }
        private NUsersFetchMessage(params string[] handles)
        {
            var request = new TUsersFetch();
            request.Handles = new TUsersFetch.Types.Handles();
            request.Handles.Handles_.Add(handles);
            payload = new Envelope {UsersFetch = request};
        }

        private NUsersFetchMessage()
        {
            payload = new Envelope {UsersFetch = new TUsersFetch()};
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string ids = "";
            string handles = "";
            foreach (var id in payload.UsersFetch.UserIds.UserIds_)
            {
                ids += id.ToBase64() + ",";
            }
            foreach (var handle in payload.UsersFetch.Handles.Handles_)
            {
                handles += handle + ",";
            }
            return String.Format("NUsersFetchMessage(UserIds={0},Handles={1})", ids, handles);
        }

        public static NUsersFetchMessage Default(params byte[] ids)
        {
            return new NUsersFetchMessage.Builder(ids).Build();
        }
        
        public static NUsersFetchMessage Default(params string[] handles)
        {
            return new NUsersFetchMessage.Builder(handles).Build();
        }

        public class Builder
        {
            private NUsersFetchMessage message;

            public Builder()
            {
                message = new NUsersFetchMessage();
            }
            
            public Builder(params byte[] ids)
            {
                message = new NUsersFetchMessage(ids);
            }
            
            public Builder(params string[] handles)
            {
                message = new NUsersFetchMessage(handles);
            }

            public Builder Add(params byte[] ids)
            {
                if (message.payload.UsersFetch.SetCase != TUsersFetch.SetOneofCase.UserIds)
                {
                    message.payload.UsersFetch.ClearSet();
                    message.payload.UsersFetch.UserIds = new TUsersFetch.Types.UserIds();
                }
                
                message.payload.UsersFetch.UserIds.UserIds_.Add(ByteString.CopyFrom(ids));
                return this;
            }

            public Builder Add(IEnumerable<byte[]> ids)
            {
                if (message.payload.UsersFetch.SetCase != TUsersFetch.SetOneofCase.UserIds)
                {
                    message.payload.UsersFetch.ClearSet();
                    message.payload.UsersFetch.UserIds = new TUsersFetch.Types.UserIds();
                }
                
                foreach (var id in ids)
                {
                    message.payload.UsersFetch.UserIds.UserIds_.Add(ByteString.CopyFrom(id));
                }
                return this;
            }
            
            public Builder AddHandles(params string[] handles)
            {
                if (message.payload.UsersFetch.SetCase != TUsersFetch.SetOneofCase.Handles)
                {
                    message.payload.UsersFetch.ClearSet();
                    message.payload.UsersFetch.Handles = new TUsersFetch.Types.Handles();
                }
                
                message.payload.UsersFetch.Handles.Handles_.Add(handles);
                return this;
            }

            public Builder AddHandles(IEnumerable<string> handles)
            {
                if (message.payload.UsersFetch.SetCase != TUsersFetch.SetOneofCase.Handles)
                {
                    message.payload.UsersFetch.ClearSet();
                    message.payload.UsersFetch.Handles = new TUsersFetch.Types.Handles();
                }
                
                message.payload.UsersFetch.Handles.Handles_.Add(handles);
                return this;
            }

            public NUsersFetchMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NUsersFetchMessage();
                if (original.payload.UsersFetch.SetCase == TUsersFetch.SetOneofCase.UserIds)
                {
                    message.payload.UsersFetch.UserIds = new TUsersFetch.Types.UserIds();
                    message.payload.UsersFetch.UserIds.UserIds_.Add(original.payload.UsersFetch.UserIds.UserIds_);   
                } else if (original.payload.UsersFetch.SetCase == TUsersFetch.SetOneofCase.Handles)
                {
                    message.payload.UsersFetch.Handles = new TUsersFetch.Types.Handles();
                    message.payload.UsersFetch.Handles.Handles_.Add(original.payload.UsersFetch.Handles.Handles_);    
                }
                
                return original;
            }
        }
    }
}
