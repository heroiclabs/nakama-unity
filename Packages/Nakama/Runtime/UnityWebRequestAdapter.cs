/**
 * Copyright 2019 The Nakama Authors
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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.Networking;

namespace Nakama
{
    /// <summary>
    /// Unity web request adapter which uses the UnityWebRequest to send requests.
    /// </summary>
    /// <remarks>
    /// Note Content-Type header is always set as 'application/json'.
    /// </remarks>
    public class UnityWebRequestAdapter : MonoBehaviour, IHttpAdapter
    {
        /// <inheritdoc cref="IHttpAdapter.Logger"/>
        public ILogger Logger { get; set; }

        private static readonly object Lock = new object();
        private static UnityWebRequestAdapter _instance;

        public static UnityWebRequestAdapter Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance != null) return _instance;

                    var go = GameObject.Find("/[Nakama]");
                    if (go == null)
                    {
                        go = new GameObject("[Nakama]");
                    }

                    if (go.GetComponent<UnityWebRequestAdapter>() == null)
                    {
                        go.AddComponent<UnityWebRequestAdapter>();
                    }

                    DontDestroyOnLoad(go);
                    _instance = go.GetComponent<UnityWebRequestAdapter>();
                    return _instance;
                }
            }
        }

        private UnityWebRequestAdapter()
        {
        }

        /// <inheritdoc cref="IHttpAdapter"/>
        public Task<string> SendAsync(string method, Uri uri, IDictionary<string, string> headers, byte[] body,
            int timeout)
        {
            var www = BuildRequest(method, uri, headers, body, timeout);
            var tcs = new TaskCompletionSource<string>();
            StartCoroutine(SendRequest(www, resp => tcs.SetResult(resp), err => tcs.SetException(err)));
            return tcs.Task;
        }

        private static UnityWebRequest BuildRequest(string method, Uri uri, IDictionary<string, string> headers,
            byte[] body, int timeout)
        {
            UnityWebRequest www;
            if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase))
            {
                www = new UnityWebRequest(uri, method)
                {
                    uploadHandler = new UploadHandlerRaw(body),
                    downloadHandler = new DownloadHandlerBuffer()
                };
            }
            else if (string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
            {
                www = UnityWebRequest.Delete(uri);
            }
            else
            {
                www = UnityWebRequest.Get(uri);
            }

            www.SetRequestHeader("Content-Type", "application/json");
            foreach (var kv in headers)
            {
                www.SetRequestHeader(kv.Key, kv.Value);
            }

            www.timeout = timeout;
            return www;
        }

        private static IEnumerator SendRequest(UnityWebRequest www, Action<string> callback,
            Action<ApiResponseException> errback)
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError)
            {
                errback(new ApiResponseException(www.error));
            }
            else if (www.isHttpError)
            {
                var decoded = www.downloadHandler.text.FromJson<Dictionary<string, object>>();

                ApiResponseException e = new ApiResponseException(www.downloadHandler.text);

                if (decoded != null)
                {
                    e = new ApiResponseException(www.responseCode, decoded["message"].ToString(), (int) decoded["code"]);

                    if (decoded.ContainsKey("error"))
                    {
                        _instance.CopyResponseError(decoded["error"], e);
                    }
                }

                errback(e);
            }
            else
            {
                callback?.Invoke(www.downloadHandler?.text);
            }
        }
    }
}
