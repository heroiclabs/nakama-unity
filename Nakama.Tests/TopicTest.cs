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
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Nakama.Tests
{
    [TestFixture]
    public class TopicTest
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
        public void JoinTopic()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = new NTopicJoinMessage.Builder().TopicRoom("test-room").Build();
            client1.Send(message, (INResultSet<INTopic> topics) =>
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

        [Test, Order(2)]
        public void LeaveTopic()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            var message = new NTopicJoinMessage.Builder().TopicRoom("test-room").Build();
            client1.Send(message, (INResultSet<INTopic> topics) =>
            {
                var topic = topics.Results[0];
                client1.Send(NTopicLeaveMessage.Default(topic.Topic), (bool complete) =>
                {
                    evt.Set();
                }, (INError err) => {
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

        [Test, Order(3)]
        public void PresenceUpdateJoinTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            string room = "test-room";
            var message = new NTopicJoinMessage.Builder().TopicRoom(room).Build();
            client1.Send(message, (INResultSet<INTopic> topics) =>
            {
                evt1.Set();
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);

            string joinUserId = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.OnTopicPresence = (INTopicPresence presence) =>
            {
                joinUserId = presence.Join[0].UserId;
                evt2.Set();
            };
            client2.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INResultSet<INTopic> topic) =>
            {
                // No action.
            }, (INError err) =>
            {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.AreEqual(joinUserId, userId2);
        }

        [Test, Order(4)]
        public void PresenceUpdateLeaveTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            string room = "test-room";
            INTopic topic = null;
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INResultSet<INTopic> topics1) =>
            {
                topic = topics1.Results[0];
                client2.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INResultSet<INTopic> topics2) =>
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
            Assert.IsNotNull(topic);

            string leaveUserId = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.OnTopicPresence = (INTopicPresence presence) =>
            {
                leaveUserId = presence.Leave[0].UserId;
                evt2.Set();
            };
            client2.Send(NTopicLeaveMessage.Default(topic.Topic), (bool complete) =>
            {
                // No action.
            }, (INError err) =>
            {
                error = err;
                evt2.Set();
            });

            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.AreEqual(userId2, leaveUserId);
        }

        [Test, Order(5)]
        public void MessageSendWithoutJoinTopic()
        {
            INError error = null;

            INTopic topic = null;
            ManualResetEvent evt1 = new ManualResetEvent(false);
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom("test-room").Build(), (INResultSet<INTopic> topics) =>
            {
                topic = topics.Results[0];
                client1.Send(NTopicLeaveMessage.Default(topic.Topic), (bool complete) =>
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
            Assert.IsNotNull(topic);

            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.Send(NTopicMessageSendMessage.Default(topic.Topic, "{\"some\":\"data\"}"), (INTopicMessageAck ack) =>
            {
                evt2.Set();
            }, (INError err) =>
            {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(5000, false);
            Assert.IsNotNull(error);
        }

        [Test, Order(6)]
        public void MessageSendTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            string room = "test-room";
            INTopic topic = null;
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INResultSet<INTopic> topics1) =>
            {
                topic = topics1.Results[0];
                client2.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INResultSet<INTopic> topics2) =>
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
            Assert.IsNotNull(topic);

            string data = "{\"some\":\"data\"}";
            INTopicMessage message = null;
            INTopicMessageAck messageAck = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            ManualResetEvent evt3 = new ManualResetEvent(false);
            client1.OnTopicMessage = (INTopicMessage msg) =>
            {
                message = msg;
                evt2.Set();
            };
            client2.Send(NTopicMessageSendMessage.Default(topic.Topic, data), (INTopicMessageAck ack) =>
            {
                messageAck = ack;
                evt3.Set();
            }, (INError err) =>
            {
                error = err;
                evt2.Set();
                evt3.Set();
            });

            evt2.WaitOne(5000, false);
            evt3.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(message);
            Assert.IsNotNull(messageAck);

            Assert.AreEqual(userId2, message.UserId);
            Assert.AreEqual(TopicMessageType.Chat, message.Type);
            Assert.AreEqual(data, message.Data);

            Assert.AreEqual(messageAck.MessageId, message.MessageId);
            Assert.AreEqual(messageAck.CreatedAt, message.CreatedAt);
            Assert.AreEqual(messageAck.ExpiresAt, message.ExpiresAt);
            Assert.AreEqual(messageAck.Handle, message.Handle);
        }

        [Test, Order(7)]
        public void MessagesListTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            string room = "history-room";
            INTopic t = null;
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INResultSet<INTopic> topics) =>
            {
                t = topics.Results[0];
                evt1.Set();
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(t);

            string data = "{\"some\":\"history data " +
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(random.GetString())) + "\"}";
            for (int i = 0; i < 10; i++)
            {
                ManualResetEvent evtFor = new ManualResetEvent(false);
                client1.Send(NTopicMessageSendMessage.Default(t.Topic, data), (INTopicMessageAck ack) =>
                {
                    evtFor.Set();
                }, (INError err) =>
                {
                    error = err;
                    evtFor.Set();
                });
                evtFor.WaitOne(5000, false);
                if (error != null)
                {
                  break;
                }
            }
            Assert.IsNull(error);

            INResultSet<INTopicMessage> messages = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.Send(new NTopicMessagesListMessage.Builder().TopicRoom(room).Forward(false).Limit(10).Build(), (INResultSet<INTopicMessage> msgs) =>
            {
                messages = msgs;
                evt2.Set();
            }, (INError err) =>
            {
                error = err;
                evt2.Set();
            });
            evt2.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(messages);
            Assert.AreEqual(10, messages.Results.Count);
            foreach (var msg in messages.Results)
            {
                Assert.AreEqual(data, msg.Data);
            }
        }

        [Test, Order(8)]
        public void PresenceUpdateChangeHandle()
        {
            string handle = random.GetString(20);
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            string room = "test-room-handle-update";
            INTopic topic = null;

            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INResultSet<INTopic> topics1) =>
            {
                topic = topics1.Results[0];

                client1.OnTopicPresence = (INTopicPresence presence) =>
                {
                    Assert.AreEqual(presence.Leave[0].UserId, presence.Join[0].UserId);
                    Assert.AreEqual(presence.Join[0].Handle, handle);
                    evt1.Set();
                };

                client1.Send(new NSelfUpdateMessage.Builder().Handle(handle).Build(), (bool completed) =>
                {
                    // do nothing
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
            Assert.IsNotNull(topic);
        }
    }
}
