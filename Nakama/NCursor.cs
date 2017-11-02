﻿/**
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
    internal class NCursor : INCursor
    {
        public string Value { get; private set; }

        internal NCursor(string cursor)
        {
            Value = cursor;
        }

        public string Serialise()
        {
            return Value;
        }

        public void Restore(string cursor)
        {
            Value = cursor;
        }

        public override string ToString()
        {
            return String.Format("NCursor(Value={0})", Value);
        }
    }
}
