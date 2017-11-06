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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Nakama.Tests
{
    [TestFixture]
    public class MatchmakeTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().First());
        private static readonly string DefaultServerKey = "defaultkey";

        private static INClient client1;
        private static INClient client2;

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

        [Test, Order(1)]
        public void MatchmakeAdd()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;
            INMatchmakeMatched res = null;

            client1.OnMatchmakeMatched = (INMatchmakeMatched matched) =>
            {
                res = matched;
                evt.Set();
            };

            client1.Send(NMatchmakeAddMessage.Default(2), (INMatchmakeTicket ticket1) =>
            {
            }, (INError err) =>
            {
                error = err;
            });

            evt.WaitOne(2000, false);
            Assert.IsNull(error);
            Assert.IsNull(res);
        }

        [Test, Order(2)]
        public void MatchmakeMatched()
        {
            ManualResetEvent evt1 = new ManualResetEvent(false);
            ManualResetEvent evt2 = new ManualResetEvent(false);
            INError error = null;
            INError error1 = null;
            INError error2 = null;
            INMatchmakeMatched res1 = null;
            INMatchmakeMatched res2 = null;

            client1.OnMatchmakeMatched = (INMatchmakeMatched matched) =>
            {
                res1 = matched;
                evt1.Set();
            };
            client2.OnMatchmakeMatched = (INMatchmakeMatched matched) =>
            {
                res2 = matched;
                evt2.Set();
            };
            
            

            var b1 = new NMatchmakeAddMessage.Builder(2);
            b1.addProperty("rank", 12);
            b1.addProperty("modes", new HashSet<string> {"tdm", "ffa"});
            b1.addProperty("divisions", new HashSet<string>{"silver1"});
            b1.addRangeFilter("rank", 10, 15);
            b1.addTermFilter("modes", new HashSet<string>{"tdm", "ffa"}, false);
            b1.addTermFilter("divisions", new HashSet<string>{"bronze3","silver1", "silver2"}, false); // like RocketLeague
            client1.Send(b1.Build(), (INMatchmakeTicket ticket1) =>
            {
                var b2 = new NMatchmakeAddMessage.Builder(2);
                b2.addProperty("rank", 10);
                b2.addProperty("modes", new HashSet<string>{"tdm", "ffa"});
                b2.addProperty("divisions", new HashSet<string>{"bronze3"});
                b2.addRangeFilter("rank", 8, 12);
                b2.addTermFilter("modes", new HashSet<string>{"tdm", "ffa"}, false);
                b2.addTermFilter("divisions", new HashSet<string>{"bronze2","bronze3", "silver1"}, false);
                client2.Send(b2.Build(), (INMatchmakeTicket ticket2) =>
                {
                    // No action.
                }, (INError err) =>
                {
                    error = err;
                });
            }, (INError err) =>
            {
                error = err;
            });

            evt1.WaitOne(5000, false);
            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNull(error1);
            Assert.IsNull(error2);
            Assert.IsNotNull(res1);
            Assert.IsNotNull(res2);
            Assert.IsNotNull(res1.Token);
            Assert.IsNotNull(res2.Token);
            Assert.AreEqual(res1.Token.Token, res2.Token.Token);

            var c1Props = res1.UserProperties[0]; 
            if (res1.Self.UserId != res1.UserProperties[0].Id)
            {
                c1Props = res1.UserProperties[1];
            }
            
            Assert.AreEqual(((long)c1Props.Properties["rank"]), 12);
            Assert.AreEqual(((INMatchmakeRangeFilter)c1Props.Filters["rank"]).Lowerbound, 10);
        }

        [Test, Order(3)]
        public void MatchmakeRemove()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;
            INMatchmakeMatched res1 = null;
            INMatchmakeMatched res2 = null;

            client1.OnMatchmakeMatched = (INMatchmakeMatched matched) =>
            {
                res1 = matched;
                evt.Set();
            };
            client2.OnMatchmakeMatched = (INMatchmakeMatched matched) =>
            {
                res2 = matched;
                evt.Set();
            };

            client1.Send(NMatchmakeAddMessage.Default(2), (INMatchmakeTicket ticket1) =>
            {
                client1.Send(NMatchmakeRemoveMessage.Default(ticket1), (bool done) =>
                {
                    client2.Send(NMatchmakeAddMessage.Default(2), (INMatchmakeTicket ticket2) =>
                    {
                        // No action.
                    }, (INError err) =>
                    {
                        error = err;
                    });
                }, (INError err) =>
                {
                    error = err;
                });
            }, (INError err) =>
            {
                error = err;
            });

            evt.WaitOne(2000, false);
            Assert.IsNull(error);
            Assert.IsNull(res1);
            Assert.IsNull(res2);
        }

        [Test, Order(4)]
        public void MatchmakingJoin()
        {
            ManualResetEvent evt1 = new ManualResetEvent(false);
            ManualResetEvent evt2 = new ManualResetEvent(false);
            INError error = null;
            INError error1 = null;
            INError error2 = null;
            INMatchmakeMatched res1 = null;
            INMatchmakeMatched res2 = null;

            client1.OnMatchmakeMatched = (INMatchmakeMatched matched) =>
            {
                res1 = matched;
                evt1.Set();
            };
            client2.OnMatchmakeMatched = (INMatchmakeMatched matched) =>
            {
                res2 = matched;
                evt2.Set();
            };

            client1.Send(NMatchmakeAddMessage.Default(2), (INMatchmakeTicket ticket1) =>
            {
                client2.Send(NMatchmakeAddMessage.Default(2), (INMatchmakeTicket ticket2) =>
                {
                    // No action.
                }, (INError err) =>
                {
                    error = err;
                });
            }, (INError err) =>
            {
                error = err;
            });

            evt1.WaitOne(5000, false);
            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNull(error1);
            Assert.IsNull(error2);
            Assert.AreEqual(res1.Token.Token, res2.Token.Token);

            ManualResetEvent evt1m = new ManualResetEvent(false);
            ManualResetEvent evt2m = new ManualResetEvent(false);
            INMatch m1 = null;
            INMatch m2 = null;
            INError error1m = null;
            INError error2m = null;
            client1.Send(NMatchJoinMessage.Default(res1.Token), (INResultSet<INMatch> matches) =>
            {
                m1 = matches.Results[0];
                evt1m.Set();
            }, (INError err) =>
            {
                error1m = err;
                evt1m.Set();
            });
            client2.Send(NMatchJoinMessage.Default(res2.Token), (INResultSet<INMatch> matches) =>
            {
                m2 = matches.Results[0];
                evt2m.Set();
            }, (INError err) =>
            {
                error2m = err;
                evt2m.Set();
            });
            evt1m.WaitOne(5000, false);
            Assert.IsNull(error1m);
            Assert.IsNull(error2m);
            Assert.IsNotNull(m1);
            Assert.IsNotNull(m2);
            Assert.AreEqual(m1.Id, m2.Id);
        }
    }
}
