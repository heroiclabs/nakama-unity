// Copyright 2023 The Nakama Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.Networking;

namespace Satori
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

        public TransientExceptionDelegate TransientExceptionDelegate => IsTransientException;

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
            int timeout, CancellationToken? cancellationToken)
        {
            var www = BuildRequest(method, uri, headers, body, timeout);
            var tcs = new TaskCompletionSource<string>();
            cancellationToken?.Register(() => tcs.TrySetCanceled());
            StartCoroutine(SendRequest(www, resp => tcs.TrySetResult(resp), err => tcs.TrySetException(err)));
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
            if (IsNetworkError(www))
            {
                errback(new ApiResponseException(www.error));
            }
            else if (IsHttpError(www))
            {
                if (www.responseCode >= 500)
                {
                    // TODO think of best way to map HTTP code to GRPC code since we can't rely
                    // on server to process it. Manually adding the mapping to SDK seems brittle.
                    errback(new ApiResponseException((int) www.responseCode, www.downloadHandler.text, -1));
                    www.Dispose();
                    yield break;
                }

                var decoded = www.downloadHandler.text.FromJson<Dictionary<string, object>>();

                var e = new ApiResponseException(www.downloadHandler.text);

                if (decoded != null)
                {
                    var msg = decoded.ContainsKey("message") ? decoded["message"].ToString() : string.Empty;
                    var grpcCode = decoded.ContainsKey("code") ? (int) decoded["code"] : -1;

                    e = new ApiResponseException(www.responseCode, msg, grpcCode);

                    if (decoded.ContainsKey("error"))
                    {
                        HttpAdapterUtil.CopyResponseError(Instance, decoded["error"], e);
                    }
                }

                errback(e);
            }
            else
            {
                callback?.Invoke(www.downloadHandler?.text);
            }

            www.Dispose();
        }

        private static bool IsHttpError(UnityWebRequest www)
        {
#if UNITY_2020_2_OR_NEWER
            return www.result == UnityWebRequest.Result.ProtocolError;
#else
            return www.isHttpError;
#endif
        }

        private static bool IsNetworkError(UnityWebRequest www)
        {
#if UNITY_2020_2_OR_NEWER
            return www.result == UnityWebRequest.Result.ConnectionError;
#else
            return www.isNetworkError;
#endif
        }

        private static bool IsTransientException(Exception e)
        {
            return e is ApiResponseException apiException && (apiException.StatusCode >= 500 || apiException.StatusCode == -1);
        }
    }
}
