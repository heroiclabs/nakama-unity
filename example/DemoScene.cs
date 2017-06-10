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

using Nakama;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DemoScene : MonoBehaviour {

    private const string ServerHost = "127.0.0.1";
    private const int ServerPort = 7350;
    private const string ServerKey = "defaultkey";
    private const bool ServerSsl = false;

    private const string Email = "email@example.com";
    private const string Password = "SOME_VERY_SECURE_PASSWORD";

    private INClient _client1;
    private INClient _client2;
    private INSession _session1;
    private INSession _session2;

    private bool _client1Connected;
    private bool _client2Connected;
    private byte[] _idPlayer1;
    private byte[] _idPlayer2;
    private INTopicId _topicPlayer1;
    private INTopicId _topicPlayer2;

    private bool _registerButtonPlayer1Enable = true;
    private bool _loginButtonPlayer1Enable = true;
    private bool _connectPlayer1Enable;
    private bool _registerButtonPlayer2Enable = true;
    private bool _loginButtonPlayer2Enable = true;
    private bool _connectPlayer2Enable;
    private bool _selfPlayer1Enable;
    private bool _selfPlayer2Enable;
    private bool _joinTopicPlayer1Enable;
    private bool _joinTopicPlayer2Enable;
    private bool _sendChatPlayer1Enable;
    private bool _sendChatPlayer2Enable;

    private string _outputText = "";

    public Button RegisterButtonPlayer1;
    public Button LoginButtonPlayer1;
    public Button ConnectPlayer1;
    public Button RegisterButtonPlayer2;
    public Button LoginButtonPlayer2;
    public Button ConnectPlayer2;
    public Button SelfPlayer1;
    public Button SelfPlayer2;
    public Button JoinTopicPlayer1;
    public Button JoinTopicPlayer2;
    public Button SendChatPlayer1;
    public Button SendChatPlayer2;
    public Text OutputTextField;

    void Start () {
        RegisterButtonPlayer1 = GameObject.Find("RegisterButtonPlayer1").GetComponent<Button>();
        LoginButtonPlayer1 = GameObject.Find("LoginButtonPlayer1").GetComponent<Button>();
        ConnectPlayer1 = GameObject.Find("ConnectPlayer1").GetComponent<Button>();
        RegisterButtonPlayer2 = GameObject.Find("RegisterButtonPlayer2").GetComponent<Button>();
        LoginButtonPlayer2 = GameObject.Find("LoginButtonPlayer2").GetComponent<Button>();
        ConnectPlayer2 = GameObject.Find("ConnectPlayer2").GetComponent<Button>();
        SelfPlayer1 = GameObject.Find("SelfPlayer1").GetComponent<Button>();
        SelfPlayer2 = GameObject.Find("SelfPlayer2").GetComponent<Button>();
        JoinTopicPlayer1 = GameObject.Find("JoinTopicPlayer1").GetComponent<Button>();
        JoinTopicPlayer2 = GameObject.Find("JoinTopicPlayer2").GetComponent<Button>();
        SendChatPlayer1 = GameObject.Find("SendChatPlayer1").GetComponent<Button>();
        SendChatPlayer2 = GameObject.Find("SendChatPlayer2").GetComponent<Button>();
        OutputTextField = GameObject.Find("OutputTextField").GetComponent<Text>();

        _client1 = new NClient.Builder(ServerKey)
                .Host(ServerHost)
                .Port(ServerPort)
                .SSL(ServerSsl)
                .Build();

        _client2 = new NClient.Builder(ServerKey)
                .Host(ServerHost)
                .Port(ServerPort)
                .SSL(ServerSsl)
                .Build();

        _client1.OnTopicMessage += (object sender, NTopicMessageEventArgs args) => {
            _outputText = Encoding.UTF8.GetString(args.Message.Data);
        };

        _client2.OnTopicMessage += (object sender, NTopicMessageEventArgs args) => {
            _outputText = Encoding.UTF8.GetString(args.Message.Data);
        };
    }

    void Update () {
        RegisterButtonPlayer1.interactable = _registerButtonPlayer1Enable;
        LoginButtonPlayer1.interactable = _loginButtonPlayer1Enable;
        ConnectPlayer1.interactable = _connectPlayer1Enable;
        RegisterButtonPlayer2.interactable = _registerButtonPlayer2Enable;
        LoginButtonPlayer2.interactable = _loginButtonPlayer2Enable;
        ConnectPlayer2.interactable = _connectPlayer2Enable;
        SelfPlayer1.interactable = _selfPlayer1Enable;
        SelfPlayer2.interactable = _selfPlayer2Enable;
        JoinTopicPlayer1.interactable = _joinTopicPlayer1Enable;
        JoinTopicPlayer2.interactable = _joinTopicPlayer2Enable;
        SendChatPlayer1.interactable = _sendChatPlayer1Enable;
        SendChatPlayer2.interactable = _sendChatPlayer2Enable;

        OutputTextField.text = _outputText;
    }

    void OnApplicationQuit() {
        if (_client1Connected) {
            _client1.Disconnect();
        }

        if (_client2Connected) {
            _client2.Disconnect();
        }
    }

    public void Player1RegisterClick() {
        string id = SystemInfo.deviceUniqueIdentifier;
        var message = NAuthenticateMessage.Device(id);
        _client1.Register(message, (INSession session) => {
            Debug.Log("Player 1 Registration successful");
            _loginButtonPlayer1Enable = true;
            _registerButtonPlayer1Enable = false;
        }, (INError error) => {
            Debug.LogErrorFormat("ID register '{0}' failed: {1}", id, error);
        });
    }

    public void Player1LoginClick() {
        string id = SystemInfo.deviceUniqueIdentifier;
        var message = NAuthenticateMessage.Device(id);
        _client1.Login(message, (INSession session) => {
            Debug.Log("Player 1 Logged in successfully.");
            _session1 = session;
            _registerButtonPlayer1Enable = false;
            _loginButtonPlayer1Enable = false;
            _connectPlayer1Enable = true;
        }, (INError error) => {
            Debug.LogErrorFormat("ID login '{0}' failed: {1}", id, error);
        });
    }

    public void Player2RegisterClick() {
        var message = NAuthenticateMessage.Email(Email, Password);
        _client2.Register(message, (INSession session) => {
            Debug.Log("Player 2 Registration successful");
            _loginButtonPlayer2Enable = true;
            _registerButtonPlayer2Enable = false;
        }, (INError error) => {
            Debug.LogErrorFormat("Email register '{0}' failed: {1}", Email, error);
        });
    }

    public void Player2LoginClick() {
        var message = NAuthenticateMessage.Email(Email, Password);
        _client2.Login(message, (INSession session) => {
            Debug.Log("Player 2 Logged in successfully");
            _session2 = session;
            _registerButtonPlayer2Enable = false;
            _loginButtonPlayer2Enable = false;
            _connectPlayer2Enable = true;
        }, (INError error) => {
            Debug.LogErrorFormat("Email login '{0}' failed: {1}", Email, error);
        });
    }

    public void Player1Connect() {
        _client1.Connect(_session1);
        Debug.Log("Player 1 connected successfully");
        _connectPlayer1Enable = false;
        _selfPlayer1Enable = true;
        _client1Connected = true;
    }

    public void Player2Connect() {
        _client2.Connect(_session2);
        Debug.Log("Player 2 connected successfully");
        _connectPlayer2Enable = false;
        _selfPlayer2Enable = true;
        _client2Connected = true;
    }

    public void Player1GetSelf() {
        _client1.Send(NSelfFetchMessage.Default(), (INSelf self) => {
            Debug.LogFormat("Player 1 handle: '{0}'.", self.Handle);
            _idPlayer1 = self.Id;
            _joinTopicPlayer1Enable = true;
        }, (INError error) => {
            Debug.LogErrorFormat("Could not retrieve player 1 self: '{0}'.", error.Message);
        });
    }

    public void Player2GetSelf() {
        _client2.Send(NSelfFetchMessage.Default(), (INSelf self) => {
            Debug.LogFormat("Player 2 handle: '{0}'.", self.Handle);
            _idPlayer2 = self.Id;
            _joinTopicPlayer2Enable = true;
        }, (INError error) => {
            Debug.LogErrorFormat("Could not retrieve player 2 self: '{0}'.", error.Message);
        });
    }

    public void Player1JoinTopic() {
        NTopicJoinMessage topicJoin = new NTopicJoinMessage.Builder()
                .TopicDirectMessage(_idPlayer2)
                .Build();
        _client1.Send(topicJoin, (INTopic topic) => {
            _topicPlayer1 = topic.Topic;
            _joinTopicPlayer1Enable = false;
            _sendChatPlayer1Enable = true;
        }, (INError error) => {
            Debug.LogErrorFormat("Player 1 could not join topic: '{0}'.", error.Message);
        });
    }

    public void Player2JoinTopic() {
        NTopicJoinMessage topicJoin = new NTopicJoinMessage.Builder()
                .TopicDirectMessage(_idPlayer1)
                .Build();
        _client2.Send(topicJoin, (INTopic topic) => {
            _topicPlayer2 = topic.Topic;
            Player2SendChatMessage();
            _joinTopicPlayer2Enable = false;
            _sendChatPlayer2Enable = true;
        }, (INError error) => {
            Debug.LogErrorFormat("Player 2 could not join topic: '{0}'.", error.Message);
        });
    }

    public void Player1SendChatMessage() {
        var date = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
        var vars = new Dictionary<string, string>
        {
            {"data", "Player 1 says: Current time is " + date}
        };
        var json = JsonUtility.ToJson(vars);
        var data = Encoding.UTF8.GetBytes(json);
        var message = NTopicMessageSendMessage.Default(_topicPlayer1, data);
        _client1.Send(message, (INTopicMessageAck ack) => {
            Debug.Log("Sent message to Player 2");
        }, (INError error) => {
            Debug.LogErrorFormat("Player 1 could not send message: '{0}'.", error.Message);
        });
    }

    public void Player2SendChatMessage() {
        var date = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
        var vars = new Dictionary<string, string>
        {
            {"data", "Player 2 says: Current time is " + date}
        };
        var json = JsonUtility.ToJson(vars);
        var data = Encoding.UTF8.GetBytes(json);

        var message = NTopicMessageSendMessage.Default(_topicPlayer2, data);
        _client2.Send(message, (INTopicMessageAck ack) => {
            Debug.Log("Sent message to Player 1");
        }, (INError error) => {
            Debug.LogErrorFormat("Player 2 could not send message: '{0}'.", error.Message);
        });
    }
}
