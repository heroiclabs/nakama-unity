using System;
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

        public async void Connect()
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
                    Debug.Log("Before session");
                    session = await _client.AuthenticateDeviceAsync(udid);
                    PlayerPrefs.SetString(UdidKey, udid);
                    PlayerPrefs.SetString(SessionTokenKey, session.AuthToken);
                    Debug.Log("After session");
                }

                Debug.Log("Session Token: " + session.AuthToken);
                await _socket.ConnectAsync(session, true);
                Debug.Log("After socket connected.");
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }
    }
}