This is Heroic Labs' UnityEngine monorepository that contains libraries for accessing two different backend services, Nakama and Satori.

The clients are built on the [.NET client](https://github.com/heroiclabs/nakama-dotnet) with extensions for Unity Engine. They require the .NET 4.6 scripting runtime version to be set in the editor.

# Nakama

[Nakama](https://github.com/heroiclabs/nakama) is an open-source server designed to power modern games and apps. Features include user accounts, chat, social, matchmaker, realtime multiplayer, and much [more](https://heroiclabs.com).

Full documentation is online - https://heroiclabs.com/docs/unity-client-guide

## Getting Started

You'll need to setup the server and database before you can connect with the client. The simplest way is to use Docker but have a look at the [server documentation](https://github.com/heroiclabs/nakama#getting-started) for other options.

### Installing the SDK

1. Install and run the servers. Follow these [instructions](https://heroiclabs.com/docs/install-docker-quickstart).

2. Install the Unity SDK. You have three options for this.

   1. To use an official release, you may download either the .unitypackage or .tar from the [releases page](https://github.com/heroiclabs/nakama-unity/releases) and import it into your project. If you chose the .tar option, you can import it from a dropdown in the Unity Package Manager window.

   2. Alternatively, if you'd like to checkout a specific release or commit from Github and are using Unity 2019.4.1 or later, you can add the following to the `manifest.json` file in your project's `Packages` folder:

      ```json
          "com.heroiclabs.nakama-unity": "https://github.com/heroiclabs/nakama-unity.git?path=/Packages/Nakama#<commit | tag>"
      ```

   3. Your final option is to download prebuilt binaries from the [Asset Store](https://assetstore.unity.com/packages/tools/network/nakama-81338).

3. Use the connection credentials to build a client object.

    ```csharp
    using Nakama;
    const string scheme = "http";
    const string host = "127.0.0.1";
    const int port = 7350;
    const string serverKey = "defaultkey";
    var client = new Client(scheme, host, port, serverKey, UnityWebRequestAdapter.Instance);
    ```

## Usage

The client object has many methods to execute various features in the server or open realtime socket connections with the server.

### Authenticate

There's a variety of ways to [authenticate](https://heroiclabs.com/docs/authentication) with the server. Authentication can create a user if they don't already exist with those credentials. It's also easy to authenticate with a social profile from Google Play Games, Facebook, Game Center, etc.

```csharp
var deviceId = SystemInfo.deviceUniqueIdentifier;
var session = await client.AuthenticateDeviceAsync(deviceId);
Debug.Log(session);
```

### Sessions

When authenticated the server responds with an auth token (JWT) which contains useful properties and gets deserialized into a `Session` object.

```csharp
Debug.Log(session.AuthToken); // raw JWT token
Debug.LogFormat("Session user id: '{0}'", session.UserId);
Debug.LogFormat("Session user username: '{0}'", session.Username);
Debug.LogFormat("Session has expired: {0}", session.IsExpired);
Debug.LogFormat("Session expires at: {0}", session.ExpireTime); // in seconds.
```

It is recommended to store the auth token from the session and check at startup if it has expired. If the token has expired you must reauthenticate. The expiry time of the token can be changed as a setting in the server.

```csharp
const string prefKeyName = "nakama.session";
ISession session;
var authToken = PlayerPrefs.GetString(prefKeyName);
if (string.IsNullOrEmpty(authToken) || (session = Session.Restore(authToken)).IsExpired)
{
    Debug.Log("Session has expired. Must reauthenticate!");
};
Debug.Log(session);
```

### Requests

The client includes lots of builtin APIs for various features of the game server. These can be accessed with the async methods. It can also call custom logic as RPC functions on the server. These can also be executed with a socket object.

All requests are sent with a session object which authorizes the client.

```csharp
var account = await client.GetAccountAsync(session);
Debug.LogFormat("User id: '{0}'", account.User.Id);
Debug.LogFormat("User username: '{0}'", account.User.Username);
Debug.LogFormat("Account virtual wallet: '{0}'", account.Wallet);
```

Requests can be supplied with a retry configurations in cases of transient network or server errors.

A single configuration can be used to control all request retry behavior:

```csharp
var retryConfiguration = new RetryConfiguration(baseDelay: 1, maxRetries: 5, delegate { System.Console.Writeline("about to retry."); });

client.GlobalRetryConfiguration = retryConfiguration;
var account = await client.GetAccountAsync(session);
```

Or, the configuration can be supplied on a per-request basis:

```csharp

var retryConfiguration = new RetryConfiguration(baseDelay: 1, maxRetries: 5, delegate { System.Console.Writeline("about to retry."); });

var account = await client.GetAccountAsync(session, retryConfiguration);

```
Per-request retry configurations override the global retry
configuration.

Requests also can be supplied with a cancellation token if you need to cancel them mid-flight:

```csharp
var canceller = new CancellationTokenSource();
var account = await client.GetAccountAsync(session, retryConfiguration: null, canceller);

await Task.Delay(25);

canceller.Cancel(); // will raise a TaskCanceledException
```

### Socket

The client can create one or more sockets with the server. Each socket can have it's own event listeners registered for responses received from the server.

```csharp
var socket = client.NewSocket();
socket.Connected += () => Debug.Log("Socket connected.");
socket.Closed += () => Debug.Log("Socket closed.");
await socket.ConnectAsync(session);
```

If you'd like socket handlers to execute outside Unity's main thread, pass the `useMainThread: false` argument:

```csharp
var socket = client.NewSocket(useMainThread: false);
```

### Errors

You can capture errors when you use `await` scaffolding with Tasks in C#.

```csharp
try
{
    var account = await client.GetAccountAsync(session);
    Debug.LogFormat("User id: '{0}'", account.User.Id);
}
catch (ApiResponseException e)
{
    Debug.LogFormat("{0}", e);
}
```

### Error Callbacks

You can avoid the use of `await` where exceptions will need to be caught and use `Task.ContinueWith(...)` as a callback style with standard C# if you prefer.

```csharp
client.GetAccountAsync(session).ContinueWith(t =>
{
    if (t.IsFaulted || t.IsCanceled)
    {
        Debug.LogFormat("{0}", t.Exception);
        return;
    }
    var account = t.Result;
    Debug.LogFormat("User id: '{0}'", account.User.Id);
});
```

# Satori

Satori is a liveops server for games that powers actionable analytics, A/B testing and remote configuration. Use the Satori Unity Client to coomunicate with Satori from within your Unity game.

Full documentation is online - https://heroiclabs.com/docs/satori/client-libraries/unity

## Getting Started

Create a client object that accepts the API you were given as a Satori customer.

```csharp
using Satori;

const string scheme = "https";
const string host = "127.0.0.1"; // add your host here
const int port = 443;
const string apiKey = "apiKey"; // add the api key that was given to you as a Satori customer.

var client = new Client(scheme, host, port, apiKey);
```

Then authenticate with the server to obtain your session.


```csharp
// Authenticate with the Satori server.
try
{
    session = await client.AuthenticateAsync(id);
    Debug.Log("Authenticated successfully.");
}
catch(ApiResponseException ex)
{
    Debug.LogFormat("Error authenticating: {0}", ex.Message);
}
```

Using the client you can get any experiments or feature flags, the user belongs to.

```csharp
var experiments = await client.GetExperimentsAsync(session);
var flag = await client.GetFlagAsync(session, "FlagName");
```

You can also send arbitrary events to the server:

```csharp

await client.EventAsync(session, new Event("gameLaunched", DateTime.UtcNow));

```

This is only a subset of the Satori client API, so please see the documentation link listed earlier for the full API.

# Unity WebGL

For both Nakama and Satori WebGL builds you should make sure the `IHttpAdapter` passed into the client is a `UnityWebRequestAdapter`.

```csharp
var client = new Client("defaultkey", UnityWebRequestAdapter.Instance);
```

For Nakama, use the `NewSocket()` extension method to create the socket OR manually set the right `ISocketAdapter` per platform.

```csharp
var socket = client.NewSocket();

// or
#if UNITY_WEBGL && !UNITY_EDITOR
    ISocketAdapter adapter = new JsWebSocketAdapter();
#else
    ISocketAdapter adapter = new WebSocketAdapter();
#endif
var socket = Socket.From(client, adapter);
```

When testing our example WebGL scene before 2021.1, be sure to go into the Build Settings and set the C++ Compiler Configuration to Release instead
of Debug due to an outstanding issue in Unity WebGL builds: https://issuetracker.unity3d.com/issues/webgl-build-throws-threads-are-not-enabled-for-this-platform-error-when-programs-built-using-debug-c-plus-plus-compiler-configuration

# Contribute

The development roadmap is managed as GitHub issues and pull requests are welcome. If you're interested to enhance the code please open an issue to discuss the changes or drop in and discuss it in the [community forum](https://forum.heroiclabs.com).

This project can be opened in Unity to create a ".unitypackage".

# License

This project is licensed under the [Apache-2 License](https://github.com/heroiclabs/nakama-unity/blob/master/LICENSE).
