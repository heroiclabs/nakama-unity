// Copyright 2026 The Nakama Authors
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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Satori
{
    /// <summary>
    /// Singleton class designed to bring more ergonomics on top of the Satori client:
    /// manages client connection, session, and hooks into Unity lifetime to
    /// send appropriate events automatically(like appLaunched, gameStart, etc.)
    /// </summary>
    public class SatoriLifecycle : MonoBehaviour
    {
        private const string SatoriEventNameAppLaunched   = "appLaunched";
        private const string SatoriEventNameAppForeground = "appForeground";
        private const string SatoriEventNameAppBackground = "appBackground";
        private const string SatoriEventNameGameStart     = "gameStart";
        private const string SatoriEventNameGameEnd       = "gameEnd";

        private static SatoriLifecycle _instance;
        public static SatoriLifecycle Instance => _instance;

        // Called automatically to always reset the static instance
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;

        public IClient Client { get; private set; }
        public ISession Session { get; private set; }

        private CancellationTokenSource _cts;
        private bool _initialized;
        private bool _isInBackground;

        // TODO: Think if there's a better way to keep having this object
        // auto-connect, but not use the async void.
        private async void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            _cts = new CancellationTokenSource();

            var scheme = "http";
            var host = "0.0.0.0";
            var port = 7450;
            var apiKey = "95e22293-6299-4758-ab6f-8d363e750b21";
            var identityId = "test-player-123"; // TODO: Get this from PlayerPrefs or via parameter?

            try
            {
                Client = new Client(scheme, host, port, apiKey, UnityWebRequestAdapter.Instance);
                Client.ReceivedSessionUpdated += OnSessionUpdated;

                Session = await Client.AuthenticateAsync(identityId, cancellationToken: _cts.Token);
                _initialized = true;

                //
                // TODO: Currently these two always fire, but Unity doesn't distinguish
                // between game scenes and loading screens or similar.
                // We should either:
                // - Expose direct API that users call.
                // - Introduce a marker component that can be added to scenes where we need these events
                // - Some way to filter the scenes in this manner exists after all ..?
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;

                await SendEventAsync(new Event(SatoriEventNameAppLaunched, DateTime.UtcNow));
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnSessionUpdated(ISession session) => Session = session;

        private void OnApplicationPause(bool paused)
        {
            if (!_initialized)
            {
                return;
            }
            if (paused == _isInBackground)
            {
                return;
            }

            _isInBackground = paused;
            _ = SendEventAsync(new Event(
                paused ? SatoriEventNameAppBackground : SatoriEventNameAppForeground,
                DateTime.UtcNow));
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) =>
            _ = SendEventAsync(new Event(SatoriEventNameGameStart, DateTime.UtcNow));
        private void OnSceneUnloaded(Scene scene) =>
            _ = SendEventAsync(new Event(SatoriEventNameGameEnd, DateTime.UtcNow));

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            if (Client != null)
            {
                Client.ReceivedSessionUpdated -= OnSessionUpdated;
            }

            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async Task SendEventAsync(Event e)
        {
            if (!_initialized)
            {
                return;
            }
            try
            {
                await Client.EventsAsync(Session, new[] { e }, _cts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
