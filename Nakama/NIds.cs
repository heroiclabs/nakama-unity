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
    ///  A helper class with utilities to operate on byte arrays used as IDs in
    ///  various messages with the Nakama server protocol.
    /// </summary>
    public static class NIds
    {
        /// <summary>
        ///  Compares two byte arrays and returns true if they are equal by value.
        /// </summary>
        /// <param name="id">
        ///  The first byte array to compare.
        /// </param>
        /// <param name="other">
        ///  The byte array to compare for value equality against the first.
        /// </param>
        /// <returns>
        ///  True if the byte arrays are equal by value.
        /// </returns>
        public static bool Equals(byte[] id, byte[] other)
        {
            if (id == other)
            {
                return true; // same reference
            }
            if (id == null || other == null)
            {
                return false;
            }
            if (id.Length != other.Length)
            {
                return false;
            }
            for (int i = 0, l = id.Length; i < l; i++)
            {
                if (id[i] != other[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
