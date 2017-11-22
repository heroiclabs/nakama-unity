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
    internal class NError : INError
    {
        public ErrorCode Code { get; private set; }
        public string Message { get; private set; }
        public string CollationId { get; private set; }

        internal NError(AuthenticateResponse.Types.Error error, String collationId)
        {
            if (Enum.IsDefined(typeof(ErrorCode), Convert.ToUInt32(error.Code + 1)))
            {
                Code = (ErrorCode) Convert.ToUInt32(error.Code + 1);
            }
            else
            {
                Code = ErrorCode.Unknown;
            }
            Message = error.Message;
            CollationId = collationId;
        }

        internal NError(Error error, String collationId)
        {
            if (Enum.IsDefined(typeof(ErrorCode), Convert.ToUInt32(error.Code + 1)))
            {
                Code = (ErrorCode) Convert.ToUInt32(error.Code + 1);
            }
            else
            {
                Code = ErrorCode.Unknown;
            }
            Message = error.Message;
            CollationId = collationId;
        }

        internal NError(string message)
        {
            Code = ErrorCode.Unknown;
            Message = message;
            CollationId = null;
        }

        public override string ToString()
        {
            return String.Format("NError(Code={0},Message={1},CollationId={2})", Code, Message, CollationId);
        }
    }
}
