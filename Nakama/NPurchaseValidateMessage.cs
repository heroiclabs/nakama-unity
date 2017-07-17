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
    public class NPurchaseValidateMessage : INCollatedMessage<INPurchaseRecord>
    {
        private Envelope payload;
        public IMessage Payload {
            get {
                return payload;
            }
        }

        private NPurchaseValidateMessage(string productId, string receiptData)
        {
            payload = new Envelope {Purchase = new TPurchaseValidation()};
            payload.Purchase.ApplePurchase = new TPurchaseValidation.Types.ApplePurchase
            {
                ProductId = productId,
                ReceiptData = receiptData
            };
        }
        
        private NPurchaseValidateMessage(string productId, string productType, string purchaseToken)
        {
            payload = new Envelope {Purchase = new TPurchaseValidation()};
            payload.Purchase.GooglePurchase = new TPurchaseValidation.Types.GooglePurchase
            {
                ProductId = productId,
                ProductType = productType,
                PurchaseToken = purchaseToken
            };
        }
        
        public void SetCollationId(string id)
        {
            payload.CollationId = id;
        }

        public override string ToString()
        {
            var f = "NPurchaseValidate(Apple={0},Google={1})";
            var p = payload.NotificationsList;
            return String.Format(f, p.Limit, p.ResumableCursor);
        }
        
        public static NPurchaseValidateMessage Apple(string productId, string receiptData)
        {
            return new NPurchaseValidateMessage(productId, receiptData);
        }
        
        public static NPurchaseValidateMessage Google(string productId, string productType, string purchaseToken)
        {
            return new NPurchaseValidateMessage(productId, productType, purchaseToken);
        }
    }
}
