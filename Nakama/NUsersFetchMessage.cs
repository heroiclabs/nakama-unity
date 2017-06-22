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

        private NUsersFetchMessage(params byte[][] ids)
        {
            var uf = new List<TUsersFetch.Types.UsersFetch>();
            foreach (var id in ids)
            {
                var u = new TUsersFetch.Types.UsersFetch();
                u.UserId = ByteString.CopyFrom(id);
                uf.Add(u);
            }
            payload = new Envelope {UsersFetch = new TUsersFetch { Users = {uf}}};   
        }
        private NUsersFetchMessage(params string[] handles)
        {
            var uf = new List<TUsersFetch.Types.UsersFetch>();
            foreach (var h in handles)
            {
                var u = new TUsersFetch.Types.UsersFetch();
                u.Handle = h;
                uf.Add(u);
            }
            payload = new Envelope {UsersFetch = new TUsersFetch { Users = {uf}}};
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
            var p = payload.UsersFetch;
            string ids = "";
            string handles = "";
            foreach (var u in p.Users)
            {
                switch (u.IdCase)
                {
                    case TUsersFetch.Types.UsersFetch.IdOneofCase.Handle:
                        handles += "handle=" + u.Handle + ",";
                        break;
                    case TUsersFetch.Types.UsersFetch.IdOneofCase.UserId:
                        ids += "id=" + u.UserId + ",";
                        break;
                }
            }
            return String.Format("NUsersFetchMessage(UserIds={0}, Handles={1})", ids, handles);
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

            public Builder Add(params byte[][] ids)
            { 
                return Add(new List<byte[]>(ids));
            }

            public Builder Add(IEnumerable<byte[]> ids)
            {
                foreach (var id in ids)
                {
                    message.payload.UsersFetch.Users.Add(new TUsersFetch.Types.UsersFetch
                    {
                        UserId = ByteString.CopyFrom(id)
                    }); 
                }
                
                return this;
            }
            
            public Builder AddHandles(params string[] handles)
            {
                return AddHandles(new List<string>(handles));
            }

            public Builder AddHandles(IEnumerable<string> handles)
            {
                foreach (var h in handles)
                {
                    message.payload.UsersFetch.Users.Add(new TUsersFetch.Types.UsersFetch
                    {
                        Handle = h
                    }); 
                }
                
                return this;
            }

            public NUsersFetchMessage Build()
            {
                // Clone object so builder now operates on new copy.
                var original = message;
                message = new NUsersFetchMessage();
                message.payload.UsersFetch = new TUsersFetch(original.payload.UsersFetch);
                return original;   
            }
        }
    }
}
