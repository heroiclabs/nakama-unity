using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Nakama;
using System.Text;
using System;

public class UserChat : MonoBehaviour {

	private readonly string EMAIL = "email@example.com";
	private readonly string PASSWORD = "SOME_VERY_SECURE_PASSWORD";

	private INClient client1;
	private INClient client2;
	private INSession session1;
	private INSession session2;

	private bool client1Connected = false;
	private bool client2Connected = false;
	private INTopicId topicPlayer1 = null;
	private INTopicId topicPlayer2 = null;

	private bool RegisterButtonPlayer1Enable = true;
	private bool LoginButtonPlayer1Enable = true;
	private bool ConnectPlayer1Enable = false;
	private bool RegisterButtonPlayer2Enable = true;
	private bool LoginButtonPlayer2Enable = true;
	private bool ConnectPlayer2Enable = false;
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

		client1.OnTopicMessage = (INTopicMessage message) => {
			outputText = message.Data;
		};
		client1.OnTopicPresence = (INTopicPresence presences) => {
			foreach (var presence in presences.Join) {
				Debug.LogFormat("Presence update received by Player 1: User handle '{0}' joined the topic.", presence.Handle);
			}
			foreach (var presence in presences.Leave) {
				Debug.LogFormat("Presence update received by Player 1: User handle '{0}' left the topic.", presence.Handle);
			}
		};

		client2.OnTopicMessage = (INTopicMessage message) => {
			outputText = message.Data;
		};
		client2.OnTopicPresence = (INTopicPresence presences) => {
			foreach (var presence in presences.Join) {
				Debug.LogFormat("Presence update received by Player 2: User handle '{0}' joined the topic.", presence.Handle);
			}
			foreach (var presence in presences.Leave) {
				Debug.LogFormat("Presence update received by Player 2: User handle '{0}' left the topic.", presence.Handle);
			}
		};
	}

	void Update () {
		RegisterButtonPlayer1.interactable = RegisterButtonPlayer1Enable;
		LoginButtonPlayer1.interactable = LoginButtonPlayer1Enable;
		ConnectPlayer1.interactable = ConnectPlayer1Enable;

		RegisterButtonPlayer2.interactable = RegisterButtonPlayer2Enable;
		LoginButtonPlayer2.interactable = LoginButtonPlayer2Enable;
		ConnectPlayer2.interactable = ConnectPlayer2Enable;

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
		client1Connected = true;

		ConnectPlayer1Enable = false;
	}

	public void Player2Connect() {
		client2.Connect (session2);
		Debug.Log("Player 2 connected successfully");
		client2Connected = true;

		ConnectPlayer2Enable = false;

		JoinTopicPlayer1Enable = true;
	}

	public void Player1JoinTopic() {
		NTopicJoinMessage topicJoin = new NTopicJoinMessage.Builder ().TopicDirectMessage (session2.Id).Build ();
		client1.Send (topicJoin, (INResultSet<INTopic> topics) => {
			foreach (var presence in topics.Results[0].Presences) {
				Debug.LogFormat("Presence initial state received by Player 1: User handle '{0}' is in the topic.", presence.Handle);
			}
			topicPlayer1 = topics.Results[0].Topic;
			JoinTopicPlayer1Enable = false;
			JoinTopicPlayer2Enable = true;
		}, (INError error) => {
			Debug.LogErrorFormat ("Player 1 could not join topic: '{0}'.", error.Message);
		});
	}

	public void Player2JoinTopic() {
		NTopicJoinMessage topicJoin = new NTopicJoinMessage.Builder ().TopicDirectMessage (session1.Id).Build ();
		client2.Send (topicJoin, (INResultSet<INTopic> topics) => {
			foreach (var presence in topics.Results[0].Presences) {
				Debug.LogFormat("Presence initial state received by Player 2: User handle '{0}' is in the topic.", presence.Handle);
			}
			topicPlayer2 = topics.Results[0].Topic;
			Player2SendChatMessage ();
			JoinTopicPlayer2Enable = false;

			SendChatPlayer1Enable = true;
			SendChatPlayer2Enable = true;
		}, (INError error) => {
			Debug.LogErrorFormat ("Player 2 could not join topic: '{0}'.", error.Message);
		});
	}

	public void Player1SendChatMessage() {
		string chatMessage = "{\"data\": \"Player 1 says: Current time is " + DateTime.Now.ToString("yyyy-MM-dd\\THH:mm:ss\\Z") + "\"}";
		NTopicMessageSendMessage msg = NTopicMessageSendMessage.Default(topicPlayer1, chatMessage);
		client1.Send(msg, (INTopicMessageAck ack) => {
			Debug.Log ("Sent message to Player 2");
		}, (INError error) =>
		{
			Debug.LogErrorFormat ("Player 1 could not send message: '{0}'.", error.Message);
		});
	}

	public void Player2SendChatMessage() {
		string chatMessage = "{\"data\": \"Player 2 says: Current time is " + DateTime.Now.ToString ("yyyy-MM-dd\\THH:mm:ss\\Z") + "\"}";
		NTopicMessageSendMessage msg = NTopicMessageSendMessage.Default (topicPlayer2, chatMessage);
		client2.Send (msg, (INTopicMessageAck ack) => {
			Debug.Log ("Sent message to Player 1");
		}, (INError error) => {
			Debug.LogErrorFormat ("Player 2 could not send message: '{0}'.", error.Message);
		});
	}
}
