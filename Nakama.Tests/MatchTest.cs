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

using System;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Nakama.Tests
{
    [TestFixture]
    public class MatchTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().First());
        private static readonly string DefaultServerKey = "defaultkey";

        private static INClient client1;
        private static INClient client2;

        private static string userId1;
        private static string userId2;

        [SetUp]
        public void SetUp()
        {
            INError error = null;
            client1 = new NClient.Builder(DefaultServerKey).Build();
            client2 = new NClient.Builder(DefaultServerKey).Build();

            ManualResetEvent c1Evt = new ManualResetEvent(false);
            client1.Register(NAuthenticateMessage.Device(random.GetString()), (INSession session) =>
            {
                userId1 = session.Id;
                client1.Connect(session);
                c1Evt.Set();
            }, (INError err) =>
            {
                error = err;
                c1Evt.Set();
            });
            c1Evt.WaitOne(5000, false);
            Assert.IsNull(error);

            ManualResetEvent c2Evt = new ManualResetEvent(false);
            client2.Register(NAuthenticateMessage.Device(random.GetString()), (INSession session) =>
            {
                userId2 = session.Id;
                client2.Connect(session);
                c2Evt.Set();
            }, (INError err) =>
            {
                error = err;
                c2Evt.Set();
            });
            c2Evt.WaitOne(5000, false);
            Assert.IsNull(error);
        }

        [TearDown]
        public void TearDown()
        {
            client1.Disconnect();
            client2.Disconnect();
        }

        [Test]
        public void CreateMatch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                evt.Set();
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(5000, false);
            Assert.IsNull(error);
        }

        [Test]
        public void CreateAndLeaveMatch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                client1.Send(NMatchLeaveMessage.Default(match.Id), (bool complete) =>
                {
                    evt.Set();
                }, (INError err) =>
                {
                    error = err;
                    evt.Set();
                });
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(5000, false);
            Assert.IsNull(error);
        }

        [Test]
        public void CreateAndJoinMatch()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                client2.Send(NMatchJoinMessage.Default(match.Id), (INResultSet<INMatch> match2) =>
                {
                    evt.Set();
                }, (INError err) =>
                {
                    error = err;
                    evt.Set();
                });
            }, (INError err) =>
            {
                error = err;
                evt.Set();
            });

            evt.WaitOne(5000, false);
            Assert.IsNull(error);
        }

        [Test]
        public void PresenceUpdateJoinMatch()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            INMatch m = null;
            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                m = match;
                evt1.Set();
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(m);

            ManualResetEvent evt2 = new ManualResetEvent(false);
            string joinedUserId = null;
            client1.OnMatchPresence = (INMatchPresence presence) =>
            {
                joinedUserId = presence.Join[0].UserId;
                evt2.Set();
            };
            client2.Send(NMatchJoinMessage.Default(m.Id), (INResultSet<INMatch> match) =>
            {
                // No action.
            }, (INError err) =>
            {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.AreEqual(userId2, joinedUserId);
        }

        [Test]
        public void PresenceUpdateLeaveMatch()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            INMatch m = null;
            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                m = match;
                client2.Send(NMatchJoinMessage.Default(m.Id), (INResultSet<INMatch> match2) =>
                {
                    evt1.Set();
                }, (INError err) =>
                {
                    error = err;
                    evt1.Set();
                });
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(m);

            ManualResetEvent evt2 = new ManualResetEvent(false);
            string leftUserId = null;
            client1.OnMatchPresence = (INMatchPresence presence) =>
            {
                leftUserId = presence.Leave[0].UserId;
                evt2.Set();
            };
            client2.Send(NMatchLeaveMessage.Default(m.Id), (bool complete) =>
            {
                // No action.
            }, (INError err) =>
            {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.AreEqual(userId2, leftUserId);
        }

        [Test]
        public void SendDataMatch()
        {
            INError error = null;
            INMatch m = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                m = match;
                client2.Send(NMatchJoinMessage.Default(match.Id), (INResultSet<INMatch> match2) =>
                {
                    evt1.Set();
                }, (INError err) =>
                {
                    error = err;
                    evt1.Set();
                });
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(m);

            byte[] data = Encoding.ASCII.GetBytes("test-data");
            long opCode = 9;
            INMatchData d = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client2.OnMatchData = (INMatchData matchData) =>
            {
                d = matchData;
                evt2.Set();
            };
            client1.Send(NMatchDataSendMessage.Default(m.Id, opCode, data), false, (bool completed) =>
            {
                // No action.
            }, (INError err) => {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(d);
            Assert.AreEqual(d.Id, m.Id);
            Assert.AreEqual(d.OpCode, opCode);
            Assert.AreEqual(d.Data, data);
        }

        [Test]
        public void SendDataNoEchoMatch()
        {
            INError error = null;
            INMatch m = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                m = match;
                evt1.Set();
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(m);

            INMatchData d = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.OnMatchData = (INMatchData data) =>
            {
                d = data;
                evt2.Set();
            };
            client1.Send(NMatchDataSendMessage.Default(m.Id, 9, Encoding.ASCII.GetBytes("test-data")), false, (bool completed) =>
            {
                // No action.
            }, (INError err) => {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(1000, false);
            Assert.IsNull(error);
            Assert.IsNull(d);
        }

        [Test]
        public void SendDataSubsetMatch()
        {
            INError error = null;
            INMatch m = null;
            INUserPresence p = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            client1.Send(NMatchCreateMessage.Default(), (INMatch match) =>
            {
                m = match;
                client2.Send(NMatchJoinMessage.Default(match.Id), (INResultSet<INMatch> matches2) =>
                {
                    var match2 = matches2.Results[0];
                    foreach (var up in match2.Presence)
                    {
                        if (up.UserId.SequenceEqual(userId2))
                        {
                            p = up;
                            break;
                        }
                    }
                    evt1.Set();
                }, (INError err) =>
                {
                    error = err;
                    evt1.Set();
                });
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(m, "m was null");
            Assert.IsNotNull(p, "p was null");

            byte[] data = Encoding.ASCII.GetBytes("test-data");
            long opCode = 9;
            INMatchData d = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.OnMatchData = (INMatchData matchData) =>
            {
                d = matchData;
                evt2.Set();
            };
            var msg = new NMatchDataSendMessage.Builder(m.Id, opCode, data).Presences(new INUserPresence[]{p}).Build();
            client2.Send(msg, false, (bool completed) =>
            {
                // No action.
            }, (INError err) => {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(2000, false);
            Assert.IsNull(error);
            Assert.IsNull(d);
        }
    }
}
