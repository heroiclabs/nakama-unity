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

namespace Nakama
{
    public interface INPurchaseRecord
    {
        // Whether or not the transaction is valid and all the information matches.
        bool Success { get; }
        // If this is a new transaction or if Nakama has a log of it.
        bool SeenBefore { get; }
        // Indicates whether or not Nakama was able to reach the remote purchase service.
        bool PurchaseProviderReachable { get; }
        // A string indicating why the purchase verification failed, if appropriate.
        string Message { get; }
        // The complete response Nakama received from the remote service.
        string Data { get; }
    }
}