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
    internal class NPurchaseRecord : INPurchaseRecord
    {
        public bool Success { get; private set; }
        public bool SeenBefore { get; private set; }
        public bool PurchaseProviderReachable { get; private set; }
        public string Message { get; private set; }
        public string Data { get; private set; }
        
        internal NPurchaseRecord(TPurchaseRecord p)
        {
            Success = p.Success;
            SeenBefore = p.SeenBefore;
            PurchaseProviderReachable = p.PurchaseProviderReachable;
            Message = p.Message;
            Data = p.Data;
        }
        
        public override string ToString()
        {
            var f = "NPurchaseValidation(Success={0},SeenBefore={1},PurchaseProviderReachable={2},Message={3},Data={4})";
            return String.Format(f, Success, SeenBefore, PurchaseProviderReachable, Message, Data);
        }
    }
}