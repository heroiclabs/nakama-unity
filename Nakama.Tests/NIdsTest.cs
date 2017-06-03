/**
 * Copyright 2017 GameUp Online, Inc. d/b/a Heroic Labs.
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

using NUnit.Framework;

namespace Nakama.Tests
{
    [TestFixture]
    public class NIdsTest
    {
        [Test]
        public void ShouldBeEqual_WhenSameReference()
        {
            byte[] id = {(byte)'a'};
            Assert.IsTrue(NIds.Equals(id, id));
        }

        [Test]
        public void ShouldBeEqual_WhenSameValues()
        {
            byte[] id = {(byte)'a'};
            byte[] other = {(byte)'a'};
            Assert.IsTrue(NIds.Equals(id, other));
        }

        [Test]
        public void ShouldBeNotEqual_WhenNullRerefence()
        {
            byte[] id = {(byte)'a'};
            byte[] other = null;
            Assert.IsFalse(NIds.Equals(id, other));
        }

        [Test]
        public void ShouldBeNotEqual_WhenLengthDifferent()
        {
            byte[] id = {(byte)'a', (byte)'b'};
            byte[] other = {(byte)'a'};
            Assert.IsFalse(NIds.Equals(id, other));
        }
    }
}
