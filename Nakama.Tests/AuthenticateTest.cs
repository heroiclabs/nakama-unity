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
using System.Threading;
using NUnit.Framework;

namespace Nakama.Tests
{
    [TestFixture]
    public class AuthenticateTest
    {
        private static readonly string DefaultServerKey = "defaultkey";

        private static string[] BlankCases = new string[] {
            "",
            " ",
            "\t",
            "\n"
        };

        [Test]
        public void ClientWithServerKey_Invalid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

//            INClient client = NClient.Default("notvalid");
            INClient client = new NClient.Builder("notvalid").Trace(true).Build();
            var message = NAuthenticateMessage.Device("mydeviceid");
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("Unauthorized", result.Message);
        }

        [Test]
        public void RegisterCustom_Invalid([ValueSource("BlankCases")] string id)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Custom(id);
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (id == string.Empty)
            {
                Assert.AreEqual("Custom ID is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid custom ID, no spaces or control characters allowed", result.Message);
            }
        }

        [Test]
        public void RegisterCustom_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSession result = null;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Custom(id);
            client.Register(message, (INSession session) =>
            {
                result = session;
                evt.Set();
            }, (INError error) =>
            {
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
        }

        [Test]
        public void RegisterDevice_Invalid([ValueSource("BlankCases")] string id)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Device(id);
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (id == string.Empty)
            {
                Assert.AreEqual("Device ID is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid device ID, no spaces or control characters allowed", result.Message);
            }
        }

        [Test]
        public void RegisterDevice_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSession result = null;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Device(id);
            client.Register(message, (INSession session) =>
            {
                result = session;
                evt.Set();
            }, (INError error) =>
            {
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
        }

        [Test, Combinatorial]
        public void RegisterEmail_Invalid([ValueSource("BlankCases")] string email,
                                          [ValueSource("BlankCases")] string password)
        {
            INError error = RegisterEmailError(email, password);
            Assert.NotNull(error);
            if (email == string.Empty)
            {
                Assert.AreEqual("Email address is required", error.Message);
            }
            else
            {
                Assert.AreEqual("Invalid email address, no spaces or control characters allowed", error.Message);
            }
        }

        [Test]
        public void RegisterEmail_InvalidEmail()
        {
            string email = TestContext.CurrentContext.Random.GetString();
            string password = TestContext.CurrentContext.Random.GetString();
            INError error = RegisterEmailError(email, password);
            Assert.NotNull(error);
            Assert.AreEqual("Invalid email address format", error.Message);
        }

        [Test]
        public void RegisterEmail_InvalidPassword()
        {
            string email = TestContext.CurrentContext.Random.GetString();
            string password = TestContext.CurrentContext.Random.GetString(6);
            INError error = RegisterEmailError(email, password);
            Assert.NotNull(error);
            Assert.AreEqual("Password must be longer than 8 characters", error.Message);
        }

        private static INError RegisterEmailError(string email, string password)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Email(email, password);
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            return result;
        }

        [Test]
        public void RegisterEmail_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSession result = null;

            string piece = TestContext.CurrentContext.Random.GetString(8);
            string email = String.Format("{0}@{0}.com", piece);
            string password = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Email(email, password);
            client.Register(message, (INSession session) =>
            {
                result = session;
                evt.Set();
            }, (INError error) =>
            {
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
        }

        [Test]
        public void RegisterFacebook_Invalid([ValueSource("BlankCases")] string oauthToken)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Facebook(oauthToken);
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (oauthToken == string.Empty)
            {
                Assert.AreEqual("Access token is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid Facebook access token, no spaces or control characters allowed", result.Message);
            }
        }
/*
        [Test, Combinatorial]
        [Ignore("Test case causes suite runner to get stuck.")]
        public void RegisterGameCenter_Invalid(
                [ValueSource("BlankCases")] string playerId,
                [ValueSource("BlankCases")] string bundleId,
                [Values(-1, 0)] int timestamp,
                [ValueSource("BlankCases")] string salt,
                [ValueSource("BlankCases")] string signature,
                [ValueSource("BlankCases")] string publicKeyUrl)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.GameCenter(playerId, bundleId, timestamp, salt, signature, publicKeyUrl);
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("Bad Request", result.Message);
        }
*/
        [Test]
        public void RegisterGoogle_Invalid([ValueSource("BlankCases")] string oauthToken)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Google(oauthToken);
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (oauthToken == string.Empty)
            {
                Assert.AreEqual("Access token is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid Google access token, no spaces or control characters allowed", result.Message);
            }
        }

        [Test]
        public void RegisterSteam_Invalid([ValueSource("BlankCases")] string sessionToken)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Steam(sessionToken);
            client.Register(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("Steam registration not available", result.Message);
        }

        [Test]
        public void LoginCustom_Invalid([ValueSource("BlankCases")] string id)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Custom(id);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (id == string.Empty)
            {
                Assert.AreEqual("Custom ID is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid custom ID, no spaces or control characters allowed", result.Message);
            }
        }

        [Test]
        public void LoginCustom_Invalid_NoId()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Custom(id);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("ID not found", result.Message);
        }

        [Test]
        public void LoginCustom_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSession result = null;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Custom(id);
            client.Register(message, (INSession _) =>
            {
                client.Login(message, (INSession session) =>
                {
                    result = session;
                    evt.Set();
                }, (INError error) =>
                {
                    evt.Set();
                });
            }, (INError error) =>
            {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
        }

        [Test]
        public void LoginDevice_Invalid([ValueSource("BlankCases")] string id)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Device(id);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (id == string.Empty)
            {
                Assert.AreEqual("Device ID is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid device ID, no spaces or control characters allowed", result.Message);
            }
        }

        [Test]
        public void LoginDevice_Invalid_NoId()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Device(id);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("ID not found", result.Message);
        }

        [Test]
        public void LoginDevice_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSession result = null;

            string id = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Device(id);
            client.Register(message, (INSession _) =>
            {
                client.Login(message, (INSession session) =>
                {
                    result = session;
                    evt.Set();
                }, (INError error) =>
                {
                    evt.Set();
                });
            }, (INError error) =>
            {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
        }

        [Test]
        public void LoginEmail_Invalid([ValueSource("BlankCases")] string email,
                                       [ValueSource("BlankCases")] string password)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Email(email, password);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (email == string.Empty)
            {
                Assert.AreEqual("Email address is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid email address, no spaces or control characters allowed", result.Message);
            }
        }

        [Test]
        public void LoginEmail_Invalid_Email()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            string email = TestContext.CurrentContext.Random.GetString();
            string password = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Email(email, password);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("Invalid email address format", result.Message);
        }

        [Test]
        public void LoginEmail_Valid()
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INSession result = null;

            string piece = TestContext.CurrentContext.Random.GetString(8);
            string email = String.Format("{0}@{0}.com", piece);
            string password = TestContext.CurrentContext.Random.GetString();
            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Email(email, password);
            client.Register(message, (INSession _) =>
            {
                client.Login(message, (INSession session) =>
                {
                    result = session;
                    evt.Set();
                }, (INError error) =>
                {
                    evt.Set();
                });
            }, (INError error) =>
            {
                evt.Set();
            });

            evt.WaitOne(1000, false);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
        }

        [Test]
        public void LoginFacebook_Invalid([ValueSource("BlankCases")] string oauthToken)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Facebook(oauthToken);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (oauthToken == string.Empty)
            {
                Assert.AreEqual("Access token is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid Facebook access token, no spaces or control characters allowed", result.Message);
            }
        }
/*
        [Test, Combinatorial]
        [Ignore("Test case causes suite runner to get stuck.")]
        public void LoginGameCenter_Invalid(
                [ValueSource("BlankCases")] string playerId,
                [ValueSource("BlankCases")] string bundleId,
                [Values(-1, 0)] int timestamp,
                [ValueSource("BlankCases")] string salt,
                [ValueSource("BlankCases")] string signature,
                [ValueSource("BlankCases")] string publicKeyUrl)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.GameCenter(playerId, bundleId, timestamp, salt, signature, publicKeyUrl);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("Bad Request", result.Message);
        }
*/
        [Test]
        public void LoginGoogle_Invalid([ValueSource("BlankCases")] string oauthToken)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Google(oauthToken);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            if (oauthToken == string.Empty)
            {
                Assert.AreEqual("Access token is required", result.Message);
            }
            else
            {
                Assert.AreEqual("Invalid Google access token, no spaces or control characters allowed", result.Message);
            }
        }

        [Test]
        public void LoginSteam_Invalid([ValueSource("BlankCases")] string sessionToken)
        {
            ManualResetEvent evt = new ManualResetEvent(false);
            INError result = null;

            INClient client = NClient.Default(DefaultServerKey);
            var message = NAuthenticateMessage.Steam(sessionToken);
            client.Login(message, (INSession session) =>
            {
                evt.Set();
            }, (INError error) =>
            {
                result = error;
                evt.Set();
            });

            evt.WaitOne(500, false);
            Assert.NotNull(result);
            Assert.AreEqual("Steam login not available", result.Message);
        }
    }
}
