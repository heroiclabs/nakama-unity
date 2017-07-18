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
using System.Net;
using NUnit.Framework;

namespace Nakama.Tests
{
    [TestFixture]
    public class SimpleServerTest
    {
        private static readonly Uri baseUri = new UriBuilder("http", "127.0.0.1", 7350).Uri;

        [Test]
        public void UnrecognisedPath()
        {
            var request  = setupRequest("/blah");
            var response = extractBadResponse(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, response.StatusDescription);
        }

        [Test]
        public void UnrecognisedMethod()
        {
            var request = setupRequest();
            request.Method = WebRequestMethods.Http.Get;
            var response = extractBadResponse(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, response.StatusDescription);
        }

        [Test]
        public void UnrecognisedContentType()
        {
            var request = setupRequest();
            request.ContentType = "application/json;";
            request.Accept = "application/json;";
            var response = extractBadResponse(request);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, response.StatusDescription);
        }

        [Test]
        public void NoBasicAuthHeader()
        {
            var request  = setupRequest();
            var response = extractBadResponse(request);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, response.StatusDescription);
        }

        private static HttpWebRequest setupRequest()
        {
            return setupRequest("/user/register");
        }

        private static HttpWebRequest setupRequest(string path)
        {
            var uri = new Uri(baseUri, path);
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/octet-stream;";
            request.Accept = "application/octet-stream;";
            return request;
        }

        private static HttpWebResponse extractBadResponse(HttpWebRequest request)
        {
            try
            {
                var _ = (HttpWebResponse) request.GetResponse();
                _.Close();
                Assert.Fail("Don't use extractBadResponse when the HttpWebResponse isn't a > 400 status code.");
            }
            catch (WebException e)
            {
                if (e.Response is HttpWebResponse)
                {
                    return e.Response as HttpWebResponse;
                }
                Assert.Fail("A Nakama server must be started.");
            }
            return null;
        }
    }
}
