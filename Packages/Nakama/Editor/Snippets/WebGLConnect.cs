using System;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Snippets
{
    // ReSharper disable once InconsistentNaming
    public class WebGLConnect : MonoBehaviour
    {
        private const string SessionTokenKey = "nksession";
        private const string UdidKey = "udid";

        private IClient _client;
        private ISocket _socket;

        public Text logText;
        public Text serverText;
        public Text serverPortText;

        void Log(string msg)
        {
            if (logText != null)
            {
                logText.text += "\n" + msg;
            }
        }
        
        public async void Connect()
        {
            try
            {
                const string scheme = "http";
                string host = serverText.text;
                int port = Int32.Parse(serverPortText.text);
                const string serverKey = "defaultkey";
                
                _client = new Client(scheme, host, port, serverKey, UnityWebRequestAdapter.Instance);
                _socket = _client.NewSocket();
                _socket.Closed += () => Log("Socket closed.");
                _socket.Connected += () => Log("Socket connected.");
                _socket.ReceivedError += e => Log("Socket error: " + e.Message);

                // Cant use SystemInfo.deviceUniqueIdentifier with WebGL builds.
                var udid = PlayerPrefs.GetString(UdidKey, Guid.NewGuid().ToString());
                Log("Unique Device ID: " + udid);

                ISession session;
                var sessionToken = PlayerPrefs.GetString(SessionTokenKey);
                if (string.IsNullOrEmpty(sessionToken) || (session = Session.Restore(sessionToken)).IsExpired)
                {
                    Log("Before session");
                    session = await _client.AuthenticateDeviceAsync(udid);
                    PlayerPrefs.SetString(UdidKey, udid);
                    PlayerPrefs.SetString(SessionTokenKey, session.AuthToken);
                    Log("After session");
                }

                Log("Session Token: " + session.AuthToken);
                await _socket.ConnectAsync(session, true);
                Log("After socket connected.");
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        private void OnApplicationQuit()
        {
            _socket?.CloseAsync();
        }
    }
}