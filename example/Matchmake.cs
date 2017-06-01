using Nakama;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Matchmake : MonoBehaviour {
	
	private const string ServerHost = "127.0.0.1";
	private const int ServerPort = 7350;
	private const string ServerKey = "defaultkey";
	private const bool ServerSsl = false;

	// number of players participating in the match.
	private const int MatchPlayerSize = 2;
	// A custom Opcode to distinguish different message types
	private const int MatchOpCode = 16;

	// Nakama client
	private readonly INClient _client;
	// the current player's login ID.
	private readonly string _customUserId;

	// Matchmaking matchmakeTicket - used to cancel matchmaking request.
	private INMatchmakeTicket _matchmakeTicket;
	// Ongoing match - used to send data
	private INMatch _match;

	// Send Match Data button
	private Button _matchSendDataButton;
	// State of the send match data button
	private bool _matchSendDataButtonEnable; //false

	public Matchmake() {
		// create a new User ID for demo purposes.
		_customUserId = "nakama_demo_" + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

		// create new client
		_client = new NClient.Builder(ServerKey)
			.Host(ServerHost)
			.Port(ServerPort)
			.SSL(ServerSsl)
			.Build();
	}

	void Start() {
		_matchSendDataButton = GameObject.Find("MatchSendDataButton").GetComponent<Button>();

		RegisterAndConnect();
		SetupMatchmakeListener();
		SetupMatchPresenceListener();
		SetupMatchDataListener();
	}
	
	// Update is called once per frame
	void Update() {
		_matchSendDataButton.interactable = _matchSendDataButtonEnable;
		// You must not do any network calls on the Update loop.
	}

	void OnApplicationQuit() {
		// if no matchmaking active just disconnect.
		if (_matchmakeTicket == null) {
			// let's gracefully disconnect from the server.
			_client.Disconnect();
			return;
		}

		// if matchmaking active stop matchmaking then disconnect.
		var message = NMatchmakeRemoveMessage.Default(_matchmakeTicket);
		_client.Send(message, done => {
			// The user is now removed from the matchmaker pool.
			Debug.Log("Matchmaking stopped.");

			// let's gracefully disconnect from the server.
			_client.Disconnect();
		}, error => {
			Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);

			// let's gracefully disconnect from the server.
			_client.Disconnect();
		});
	}

	private void RegisterAndConnect() {
		// let's add a disconnect handler so we know when we are disconnected from the server.
		_client.OnDisconnect += (sender, e) => {
			Debug.Log("Disconnected from the server.");
		};

		// this can be DeviceID, Facebook, Google, GameCenter or a custom ID.
		// For more information, please visit: https://heroiclabs.com/docs/development/user/
		var request = NAuthenticateMessage.Custom(_customUserId);
		_client.Register(request, session => {
			Debug.LogFormat("Registered user.");
			// we suggest that you cache the Session object on device storage
			// so you don't have to login each time the game starts
			// For demo purposes - we'll ignore that.

			_client.Connect(session, connected => {
				Debug.Log("Socket connected.");
			});
		}, error => {
			Debug.LogErrorFormat("ID register '{0}' failed: {1}", _customUserId, error);
		});
	}

	private void SetupMatchmakeListener() {
		// Attach an event handler for when the user is matched with other users.
		// This only needs to be done once.
		// For more information, please visit: https://heroiclabs.com/docs/development/matchmaker/
		_client.OnMatchmakeMatched += (source, args) => {
			// set the local cache to null now that we are matchmaked.
			_matchmakeTicket = null;
			Debug.Log("Matchmaker found an opponent.");			

			// The match token is used to join a multiplayer match.
			// Let's accept the match and join it.
			var message = NMatchJoinMessage.Default(args.Matched.Token);
			_client.Send(message, match => {
				Debug.Log("Successfully joined match.");

				_match = match;
				// let's enable the button now that we've joined a match.
				_matchSendDataButtonEnable = true;
			}, error => {
				Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);
			});
		};
	}

	private void SetupMatchPresenceListener() {
		// For more information, please visit: https://heroiclabs.com/docs/development/realtime-multiplayer/
		_client.OnMatchPresence += (src, args) => {
			// `args.MatchPresence.Id` to get the `byte[]` match ID this update relates to.
			// `args.MatchPresence.Join` and `args.MatchPresence.Leave` for lists of
			// presences that have joined/left since the last update for this match.
			foreach (var presence in args.MatchPresence.Join) {
				Debug.LogFormat("User handle '{0}' joined the match.", presence.Handle);
			}
			foreach (var presence in args.MatchPresence.Leave) {
				Debug.LogFormat("User handle '{0}' left the match.", presence.Handle);
			}
		};
	}

	private void SetupMatchDataListener() {
		_client.OnMatchData += (src, args) => {
			// `args.MatchData.Id` to get the `byte[]` match ID this data relates to.
			// `args.MatchData.Presence` is the sender of this data.
			// `args.MatchData.OpCode` and `args.MatchData.Data` are the custom
			// fields set by the sender.

			if (args.Data.OpCode == MatchOpCode) {
				Debug.LogFormat("Received match data from {0}: {1}", args.Data.Presence.Handle, Encoding.UTF8.GetString(args.Data.Data));
			} else {
				Debug.LogFormat("Received data but didn't match expected OpsCode.");
			}
		};
	}

	// This is invoked by clicking on the scene UI button.
	public void DoMatchmake() {
		// Add self to the matchmaker pool.
		var message = NMatchmakeAddMessage.Default(MatchPlayerSize);
		_client.Send(message, ticket => {
			// Store the matchmakeTicket so it can be used to cancel later.
			Debug.Log("Added to the pool by Matchmaker.");
			_matchmakeTicket = ticket;
		}, error => {
			Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);
		});
	}

	// This is invoked by clicking on the scene UI button.
	public void SendMatchData() {
		var date = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
		var vars = new Dictionary<string, string>
		{
			{"time", date},
			{"message", "Hello World"}
		};
		var json = JsonUtility.ToJson(vars);
		var data = Encoding.UTF8.GetBytes(json);

		// `matchId` is the `byte[]` ID of the match to send to.
		// `opCode` is any desired integer.
		// `data` is any `byte[]` of data to send.
		var message = NMatchDataSendMessage.Default(_match.Id, MatchOpCode, data);
		_client.Send(message, complete => {
			Debug.Log("Successfully sent data to match.");
		}, error => {
			Debug.LogErrorFormat("Send error: code '{1}' with '{0}'.", error.Message, error.Code);
		});
	}
}
