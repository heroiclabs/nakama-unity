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
    public class TopicTest
    {
        private static readonly Randomizer random = new Randomizer(Guid.NewGuid().ToByteArray().First());
        private static readonly string DefaultServerKey = "defaultkey";

        private static INClient client1;
        private static INClient client2;

        private static byte[] userId1;
        private static byte[] userId2;

        [SetUp]
        public void ConnectClients()
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
        public void JoinTopic()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(Encoding.UTF8.GetBytes("test-room")).Build(), (INTopic topic) =>
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
        public void LeaveTopic()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError error = null;

            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(Encoding.UTF8.GetBytes("test-room")).Build(), (INTopic topic) =>
            {
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

        [Test]
        public void PresenceUpdateJoinTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            byte[] room = Encoding.UTF8.GetBytes("test-room");
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INTopic topic) =>
            {
                evt1.Set();
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);

            byte[] joinUserId = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.OnTopicPresence += (object source, NTopicPresenceEventArgs args) =>
            {
                joinUserId = args.TopicPresence.Join[0].UserId;
                evt2.Set();
            };
            client2.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INTopic topic) =>
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

        [Test]
        public void PresenceUpdateLeaveTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            byte[] room = Encoding.UTF8.GetBytes("test-room");
            INTopic topic = null;
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INTopic topic1) =>
            {
                topic = topic1;
                client2.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INTopic topic2) =>
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

            byte[] leaveUserId = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            client1.OnTopicPresence += (object source, NTopicPresenceEventArgs args) =>
            {
                leaveUserId = args.TopicPresence.Leave[0].UserId;
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

        [Test]
        public void MessageSendWithoutJoinTopic()
        {
            INError error = null;

            INTopic topic = null;
            ManualResetEvent evt1 = new ManualResetEvent(false);
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(Encoding.UTF8.GetBytes("test-room")).Build(), (INTopic top) =>
            {
                topic = top;
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
            client1.Send(NTopicMessageSendMessage.Default(topic.Topic, Encoding.UTF8.GetBytes("{\"some\":\"data\"}")), (INTopicMessageAck ack) =>
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

        [Test]
        public void MessageSendTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            byte[] room = Encoding.UTF8.GetBytes("test-room");
            INTopic topic = null;
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INTopic topic1) =>
            {
                topic = topic1;
                client2.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INTopic topic2) =>
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

            byte[] data = Encoding.UTF8.GetBytes("{\"some\":\"data\"}");
            INTopicMessage message = null;
            INTopicMessageAck messageAck = null;
            ManualResetEvent evt2 = new ManualResetEvent(false);
            ManualResetEvent evt3 = new ManualResetEvent(false);
            client1.OnTopicMessage += (object source, NTopicMessageEventArgs args) =>
            {
                message = args.Message;
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

        [Test]
        public void MessagesListTopic()
        {
            INError error = null;

            ManualResetEvent evt1 = new ManualResetEvent(false);
            byte[] room = Encoding.UTF8.GetBytes("history-room");
            INTopic t = null;
            client1.Send(new NTopicJoinMessage.Builder().TopicRoom(room).Build(), (INTopic topic) =>
            {
                t = topic;
                evt1.Set();
            }, (INError err) =>
            {
                error = err;
                evt1.Set();
            });
            evt1.WaitOne(5000, false);
            Assert.IsNull(error);
            Assert.IsNotNull(t);

            byte[] data = Encoding.UTF8.GetBytes("{\"some\":\"history data " +
                System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(random.GetString())) + "\"}");
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
    }
}
