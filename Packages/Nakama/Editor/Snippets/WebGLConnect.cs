using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama.Snippets
{
    // ReSharper disable once InconsistentNaming
    public class WebGLConnect : MonoBehaviour
    {
        private const string SessionTokenKey = "nksession";
        private const string UdidKey = "udid";

        private IClient _client;
        private ISocket _socket;

        public string serverText;
        public string serverPortText;

        public async void Awake()
        {
            try
            {
                const string scheme = "http";
                string host = serverText;
                int port = Int32.Parse(serverPortText);
                const string serverKey = "defaultkey";

                _client = new Client(scheme, host, port, serverKey, UnityWebRequestAdapter.Instance);
                _socket = _client.NewSocket();
                _socket.Closed += () => Debug.Log("Socket closed.");
                _socket.Connected += () => Debug.Log("Socket connected.");
                _socket.ReceivedError += e => Debug.Log("Socket error: " + e.Message);

                // Cant use SystemInfo.deviceUniqueIdentifier with WebGL builds.
                var udid = PlayerPrefs.GetString(UdidKey, Guid.NewGuid().ToString());
                Debug.Log("Unique Device ID: " + udid);

                ISession session;
                var sessionToken = PlayerPrefs.GetString(SessionTokenKey);
                if (string.IsNullOrEmpty(sessionToken) || (session = Session.Restore(sessionToken)).IsExpired)
                {
                    session = await _client.AuthenticateDeviceAsync(udid);
                    PlayerPrefs.SetString(UdidKey, udid);
                    PlayerPrefs.SetString(SessionTokenKey, session.AuthToken);
                }

                Debug.Log("Session Token: " + session.AuthToken);
                await _socket.ConnectAsync(session, true);
                Debug.Log("Connected ");
                var match = await _socket.CreateMatchAsync();
                Debug.Log("Created match: " + match.Id);

                await _socket.CloseAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }
    }
}