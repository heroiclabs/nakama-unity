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
    public class NStorageUpdateMessage: INCollatedMessage<INResultSet<INStorageKey>>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NStorageUpdateMessage()
        {
            payload = new Envelope { StorageUpdate = new TStorageUpdate() };
        }

        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            string output = "";
            foreach (var update in payload.StorageUpdate.Updates)
            {
                output += String.Format("(Op={0}, ReadPermission={1}, WritePermission={2}, Bucket={3}, Collection={4}, Record={5}, Version={6}),", 
                    update.Ops, update.PermissionRead, update.PermissionWrite, update.Key.Bucket, update.Key.Collection, update.Key.Record, update.Key.Version);
            }
            return String.Format("NStorageUpdateMessage(Updates={0})", output);
        }

        public class Builder
        {
            private NStorageUpdateMessage message;

            public Builder()
            {
                message = new NStorageUpdateMessage();
            }

            public NStorageUpdateMessage Build()
            {
                var original = message;
                message = new NStorageUpdateMessage
                {
                    payload = {StorageUpdate = new TStorageUpdate(original.payload.StorageUpdate)}
                };
                return original;
            }

            public Builder Update (string bucket, string collection, string record, List<TStorageUpdate.Types.StorageUpdate.Types.UpdateOp> ops)
            {
                var update = new TStorageUpdate.Types.StorageUpdate
                {
                    Key = new TStorageUpdate.Types.StorageUpdate.Types.StorageKey
                    {
                        Bucket = bucket,
                        Collection = collection,
                        Record = record
                    },
                    Ops = {ops}
                };
                
                message.payload.StorageUpdate.Updates.Add(update);
                return this;
            }
            
            public Builder Update (string bucket, string collection, string record, string version, StoragePermissionRead readPermission, StoragePermissionWrite writePermission, List<TStorageUpdate.Types.StorageUpdate.Types.UpdateOp> ops)
            {
                var update = new TStorageUpdate.Types.StorageUpdate
                {
                    Key = new TStorageUpdate.Types.StorageUpdate.Types.StorageKey
                    {
                        Bucket = bucket,
                        Collection = collection,
                        Record = record,
                        Version = version
                    },
                    Ops = {ops},
                    PermissionRead = NStorageWriteMessage.GetReadPermission(readPermission),
                    PermissionWrite = NStorageWriteMessage.GetWritePermission(writePermission),
                };
                
                message.payload.StorageUpdate.Updates.Add(update);
                return this;
            }
        }

        public class StorageUpdateBuilder
        {
            private List<TStorageUpdate.Types.StorageUpdate.Types.UpdateOp> ops;
            
            public StorageUpdateBuilder()
            {
                ops = new List<TStorageUpdate.Types.StorageUpdate.Types.UpdateOp>();
            }
            
            public List<TStorageUpdate.Types.StorageUpdate.Types.UpdateOp> Build()
            {
                return ops;
            }
            
            private static int GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode code)
            {
                switch (code)
                {
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Add:
                        return 0;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Append:
                        return 1;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Copy:
                        return 2;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Incr:
                        return 3;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Init:
                        return 4;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Merge:
                        return 5;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Move:
                        return 6;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Patch:
                        return 7;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Remove:
                        return 8;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Replace:
                        return 9;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Test:
                        return 10;
                    case TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Compare:
                        return 11;
                    default:
                        return -1; // make sure server rejects it
                }
            }

            public StorageUpdateBuilder Add (string path, string value)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Add),
                    Path = path,
                    Value = value
                });
                return this;
            }
            
            public StorageUpdateBuilder Append (string path, string value)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Append),
                    Path = path,
                    Value = value
                });
                return this;
            }
            
            public StorageUpdateBuilder Copy (string from, string path)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Copy),
                    From = from,
                    Path = path
                });
                return this;
            }
            
            public StorageUpdateBuilder Incr (string path, long value)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Incr),
                    Path = path,
                    Value = value.ToString()
                });
                return this;
            }
            
            public StorageUpdateBuilder Init (string path, string value)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Init),
                    Path = path,
                    Value = value
                });
                return this;
            }
            
            public StorageUpdateBuilder Merge (string path, string value)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Merge),
                    Path = path,
                    Value = value
                });
                return this;
            }
            
            public StorageUpdateBuilder Move (string from, string path)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Move),
                    From = from,
                    Path = path
                });
                return this;
            }
            
//            // TODO complete this 
//            public StorageUpdateBuilder Patch (string path, bool conditional, byte[] value)
//            {
//                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
//                {
//                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Patch),
//                    Path = path,
//                    Conditional = conditional,
//                    Value = ByteString.CopyFrom(value)
//                });
//                return this;
//            }
            
            public StorageUpdateBuilder Remove (string path)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Remove),
                    Path = path
                });
                return this;
            }
            
            public StorageUpdateBuilder Replace (string path, string value)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Replace),
                    Path = path,
                    Value = value
                });
                return this;
            }
            public StorageUpdateBuilder Test (string path, string value)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Test),
                    Path = path,
                    Value = value
                });
                return this;
            }
            public StorageUpdateBuilder Compare (string path, string value, long assert)
            {
                ops.Add(new TStorageUpdate.Types.StorageUpdate.Types.UpdateOp
                {
                    Op = GetOpCode(TStorageUpdate.Types.StorageUpdate.Types.UpdateOp.Types.UpdateOpCode.Compare),
                    Path = path,
                    Value = value,
                    Assert = assert
                });
                return this;
            }
            
        }
    }
}
