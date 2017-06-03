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
    /// <summary>
    ///  A collection of extension methods to make it easier to work with some
    ///  data types.
    /// </summary>
    public static class NExtensionMethods
    {
        /// <summary>
        ///  An extension for byte array which makes it easy to compare them for
        ///  equality by value.
        /// </summary>
        /// <seealso cref="Nakama.NIds.Equals"/>
        public static bool Equals(this byte[] id, byte[] other)
        {
            return NIds.Equals(id, other);
        }
    }
}
