Setup
--

There are three examples with the Nakama Unity client.
(1) UserChat shows how two users can connect to a chat channel and exchange messages within one Unity app.
(2) Matchmake shows how a user can add themselves to the matchmaker pool and connect with another user in a realtime multiplayer match. You should run a Unity app build and editor build to try it out as two users.
(3) UnityMainThreadDispatch shows a code example on how to dispatch all socket events on the Unity main thread.

Nakama server setup
--

For quick instructions on how to run Nakama server. Run a console and start the server "nakama --name nk1 --log.verbose --log.stdout".

You'll need to have installed Nakama server. For full instructions on how to setup the server have a look at the docs:
https://heroiclabs.com/docs/

UserChat
--

You'll need to attach the "UserChat.cs" script to the GameObject in the "NakamaScriptObject" scene. When this is complete you can build and run the scene and click to register two users, connect to the server, and join a direct message chat channel to send messages.

Matchmake
--

You'll need to attach the "Matchmake.cs" script to the GameObject in the "Matchmake.unity" scene. When this is complete you can build and run the scene and also run the scene within the editor. The two clients will try to connect and can click to matchmake and once both clients have been placed into a match you can send messages between each other.
