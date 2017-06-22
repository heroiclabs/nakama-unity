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
 using System.IO;
 using System.Net;
 using System.Text;
 using NUnit.Framework;

namespace Nakama
{
    [TestFixture]
    public class SimpleMetadataTest
    {
        private static readonly Uri baseUri = new UriBuilder("http", "127.0.0.1", 7351).Uri;

        [Test]
        public void GetHealth()
        {
            var request  = setupRequest();
            var response = (HttpWebResponse) request.GetResponse();
            response.Close();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, response.StatusDescription);
        }

        [Test]
        public void GetClusterStats()
        {
            var request  = setupRequest("/v0/cluster/stats");
            var response = (HttpWebResponse) request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var result = reader.ReadToEnd();
            response.Close();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, response.StatusDescription);
            Assert.That(result, Does.StartWith("["));
            Assert.That(result, Contains.Substring("name"));
            Assert.That(result, Contains.Substring("address"));
            Assert.That(result, Contains.Substring("version"));
            Assert.That(result, Contains.Substring("status"));
            Assert.That(result, Contains.Substring("process_count"));
            Assert.That(result, Does.EndWith("]"));
        }

        [Test]
        public void GetConfig()
        {
            var request  = setupRequest("/v0/config");
            var response = (HttpWebResponse) request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var result = reader.ReadToEnd();
            response.Close();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, response.StatusDescription);
            Assert.That(result, Contains.Substring("name"));
            Assert.That(result, Contains.Substring("port"));
            Assert.That(result, Contains.Substring("ops_port"));
            Assert.That(result, Contains.Substring("dsns"));
        }

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
            request.Method = WebRequestMethods.Http.Post;
            var response = extractBadResponse(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, response.StatusDescription);
        }

        [Test]
        public void UnrecognisedContentType()
        {
            var request = setupRequest();
            request.ContentType = "text/plain;";
            request.Accept = "text/plain;";
            var response = extractBadResponse(request);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode, response.StatusDescription);
        }

        private static HttpWebRequest setupRequest()
        {
            return setupRequest("/v0/info");
        }

        private static HttpWebRequest setupRequest(string path)
        {
            var uri = new Uri(baseUri, path);
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = "application/json;";
            request.Accept = "application/json;";
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
