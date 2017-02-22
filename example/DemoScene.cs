using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Nakama;
using System.Text;
using System;

public class DemoScene : MonoBehaviour {

	private readonly string EMAIL = "email@example.com";
	private readonly string PASSWORD = "SOME_VERY_SECURE_PASSWORD";

	private INClient client1;
	private INClient client2;
	private INSession session1;
	private INSession session2;

	private bool client1Connected = false;
	private bool client2Connected = false;
	private byte[] idPlayer1 = null;
	private byte[] idPlayer2 = null;
	private INTopicId topicPlayer1 = null;
	private INTopicId topicPlayer2 = null;

	private bool RegisterButtonPlayer1Enable = true;
	private bool LoginButtonPlayer1Enable = true;
	private bool ConnectPlayer1Enable = false;
	private bool RegisterButtonPlayer2Enable = true;
	private bool LoginButtonPlayer2Enable = true;
	private bool ConnectPlayer2Enable = false;
	private bool SelfPlayer1Enable = false;
	private bool SelfPlayer2Enable = false;
	private bool JoinTopicPlayer1Enable = false;
	private bool JoinTopicPlayer2Enable = false;
	private bool SendChatPlayer1Enable = false;
	private bool SendChatPlayer2Enable = false;

	private string outputText = "";

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

		client1 = new NClient.Builder("defaultkey")
			.Host("127.0.0.1")
			.Port(7350)
			.SSL(false)
			.Build();

		client2 = new NClient.Builder("defaultkey")
			.Host("127.0.0.1")
			.Port(7350)
			.SSL(false)
			.Build();

		client1.OnTopicMessage += (object sender, NTopicMessageEventArgs e) => {
			outputText = Encoding.UTF8.GetString(e.Message.Data);
		};

		client2.OnTopicMessage += (object sender, NTopicMessageEventArgs e) => {
			outputText = Encoding.UTF8.GetString(e.Message.Data);
		};
	}
		
	void Update () {
		RegisterButtonPlayer1.interactable = RegisterButtonPlayer1Enable;
		LoginButtonPlayer1.interactable = LoginButtonPlayer1Enable;
		ConnectPlayer1.interactable = ConnectPlayer1Enable;
		RegisterButtonPlayer2.interactable = RegisterButtonPlayer2Enable;
		LoginButtonPlayer2.interactable = LoginButtonPlayer2Enable;
		ConnectPlayer2.interactable = ConnectPlayer2Enable;
		SelfPlayer1.interactable = SelfPlayer1Enable;
		SelfPlayer2.interactable = SelfPlayer2Enable;
		JoinTopicPlayer1.interactable = JoinTopicPlayer1Enable;
		JoinTopicPlayer2.interactable = JoinTopicPlayer2Enable;
		SendChatPlayer1.interactable = SendChatPlayer1Enable;
		SendChatPlayer2.interactable = SendChatPlayer2Enable;

		OutputTextField.text = outputText;
	}

	void OnApplicationQuit() {
		if (client1Connected) {
			client1.Disconnect ();
		}

		if (client2Connected) {
			client2.Disconnect ();
		}
	}

	public void Player1RegisterClick() {
		string id = SystemInfo.deviceUniqueIdentifier;
		var request = NAuthenticateMessage.Device(id);
		client1.Register(request, (INSession session) => {
			Debug.Log("Player 1 Registration successful");
			LoginButtonPlayer1Enable = true;
			RegisterButtonPlayer1Enable = false;
		}, (INError error) => {
			Debug.LogErrorFormat("ID register '{0}' failed: {1}", id, error);
		});
	}

	public void Player1LoginClick() {
		string id = SystemInfo.deviceUniqueIdentifier;
		var request = NAuthenticateMessage.Device(id);
		client1.Login(request, (INSession session) => {
			Debug.Log("Player 1 Logged in successfully");
			this.session1 = session;
			RegisterButtonPlayer1Enable = false;
			LoginButtonPlayer1Enable = false;
			ConnectPlayer1Enable = true;
		}, (INError error) => {
			Debug.LogErrorFormat("ID login '{0}' failed: {1}", id, error);
		});
	}

	public void Player2RegisterClick() {
		var request = NAuthenticateMessage.Email(EMAIL, PASSWORD);
		client2.Register(request, (INSession session) => {
			Debug.Log("Player 2 Registration successful");
			LoginButtonPlayer2Enable = true;
			RegisterButtonPlayer2Enable = false;
		}, (INError error) => {
			Debug.LogErrorFormat("Email register '{0}' failed: {1}", EMAIL, error);
		});
	}

	public void Player2LoginClick() {
		var request = NAuthenticateMessage.Email(EMAIL, PASSWORD);
		client2.Login(request, (INSession session) => {
			Debug.Log("Player 2 Logged in successfully");
			this.session2 = session;
			RegisterButtonPlayer2Enable = false;
			LoginButtonPlayer2Enable = false;
			ConnectPlayer2Enable = true;
		}, (INError error) => {
			Debug.LogErrorFormat("Email login '{0}' failed: {1}", EMAIL, error);
		});
	}

	public void Player1Connect() {
		client1.Connect (session1);
		Debug.Log("Player 1 connected successfully");
		ConnectPlayer1Enable = false;
		SelfPlayer1Enable = true;
		client1Connected = true;
	}

	public void Player2Connect() {
		client2.Connect (session2);
		Debug.Log("Player 2 connected successfully");
		ConnectPlayer2Enable = false;
		SelfPlayer2Enable = true;
		client1Connected = true;
	}

	public void Player1GetSelf() {
		client1.Send(NSelfFetchMessage.Default(), (INSelf result) => {
			Debug.LogFormat ("Player 1 handle: '{0}'.", result.Handle);
			idPlayer1 = result.Id;
			JoinTopicPlayer1Enable = true;
		}, (INError error) =>
		{
			Debug.LogErrorFormat ("Could not retrieve player 1 self: '{0}'.", error.Message);
		});
	}

	public void Player2GetSelf() {
		client2.Send(NSelfFetchMessage.Default(), (INSelf result) => {
			Debug.LogFormat ("Player 2 handle: '{0}'.", result.Handle);
			idPlayer2 = result.Id;
			JoinTopicPlayer2Enable = true;
		}, (INError error) =>
		{
			Debug.LogErrorFormat ("Could not retrieve player 2 self: '{0}'.", error.Message);
		});
	}

	public void Player1JoinTopic() {
		NTopicJoinMessage topicJoin = new NTopicJoinMessage.Builder ().TopicDirectMessage (idPlayer2).Build ();
		client1.Send (topicJoin, (INTopic topic) => {
			topicPlayer1 = topic.Topic;
			JoinTopicPlayer1Enable = false;
			SendChatPlayer1Enable = true;
		}, (INError error) => {
			Debug.LogErrorFormat ("Player 1 could not join topic: '{0}'.", error.Message);
		});
	}

	public void Player2JoinTopic() {
		NTopicJoinMessage topicJoin = new NTopicJoinMessage.Builder ().TopicDirectMessage (idPlayer1).Build ();
		client2.Send (topicJoin, (INTopic topic) => {
			topicPlayer2 = topic.Topic;
			Player2SendChatMessage ();
			JoinTopicPlayer2Enable = false;
			SendChatPlayer2Enable = true;
		}, (INError error) => {
			Debug.LogErrorFormat ("Player 2 could not join topic: '{0}'.", error.Message);
		});
	}

	public void Player1SendChatMessage() {
		string chatMessage = "{\"data\": \"Player 1 says: Current time is " + DateTime.Now.ToString("yyyy-MM-dd\\THH:mm:ss\\Z") + "\"}";
		NTopicMessageSendMessage msg = NTopicMessageSendMessage.Default(topicPlayer1, Encoding.UTF8.GetBytes(chatMessage));
		client1.Send(msg, (INTopicMessageAck ack) => {
			Debug.Log ("Sent message to Player 2");
		}, (INError error) =>
		{
			Debug.LogErrorFormat ("Player 1 could not send message: '{0}'.", error.Message);
		});
	}

	public void Player2SendChatMessage() {
		string chatMessage = "{\"data\": \"Player 2 says: Current time is " + DateTime.Now.ToString ("yyyy-MM-dd\\THH:mm:ss\\Z") + "\"}";
		NTopicMessageSendMessage msg = NTopicMessageSendMessage.Default (topicPlayer2, Encoding.UTF8.GetBytes (chatMessage));
		client2.Send (msg, (INTopicMessageAck ack) => {
			Debug.Log ("Sent message to Player 1");
		}, (INError error) => {
			Debug.LogErrorFormat ("Player 2 could not send message: '{0}'.", error.Message);
		});
	}
}
