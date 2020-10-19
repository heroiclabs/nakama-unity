# Change Log
All notable changes to this project are documented below.

The format is based on [keep a changelog](http://keepachangelog.com/) and this project uses [semantic versioning](http://semver.org/).

## [2.7.0] - 2020-10-19
### Changed
Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.7.0.
Added namespace to JavaScript web socket adapter internals.

## [2.6.0] - 2020-09-21
### Changed
Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.6.0.

## [2.5.0] - 2020-08-14
### Added
- Add support for the Unity Package Manager. See the README for usage.

### Changed
- Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.5.0.
- Update minimum required Unity version to 2018.4.26f1 LTS. While older releases may work YMMV.

## [2.4.0] - 2020-05-04 :star:
### Added
- Add new scene as an example for WebGL basics. Thanks @humbertodias.

### Changed
- Add a default error handler to the socket adapter to make common errors more visible.
- Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.4.0.

### Fixed
- UnityWebRequest downloadHandler is null on DELETE methods. Thanks @hasbean.

## [2.3.2] - 2019-10-23
### Fixed
- Update interface impl signatures with JS socket adapter.

## [2.3.1] - 2019-09-21
### Changed
- Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.3.1.

## [2.3.0] - 2019-09-02
### Changed
- Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.3.0.

## [2.2.2] - 2019-07-02
### Changed
- Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.2.2.

## [2.2.1] - 2019-06-21
### Added
- New example on how to manage the client/socket/session as a singleton.
- Various improvements to existing code examples.

### Changed
- Update Nakama .NET dependency. See release notes: heroiclabs/nakama-dotnet@2.2.1.

## [2.2.0] - 2019-06-12
### Added
- Support WebGL builds.
- Add new Leaderboard and Tournament API methods.

### Changed
- Use new socket library instead of WebSocketListener.
- Update socket event names to match csharp style guide.
- Update TinyJson dependency.

### Fixed
- Socket logger must not disable the socket events.
- Deserialize data in stream messages correctly.

## [2.1.0] - 2018-08-17
### Added
- Linker instructions to preserve code in dependent DLLs.
- New code snippets for multiplayer and matchmaker examples.

### Changed
- Update dependent DLLs for lowlevel websocket driver and .NET client.

## [2.0.0] - 2018-06-18
### Added
- New documentation on the client.
- Many new features and APIs.
- Support for Nakama 2 release.

### Changed
- Rewrite client with async/await sockets.
- New project structure for simpler Unity builds.

---

## [0.10.2] - 2017-11-27
### Fixed
- Use correct JS transport listener bindings.
- Correctly calculate session expiry client-side.

### Changed
- MatchmakeAddMessage correctly follows C# naming scheme.
- Improve memory allocation profile when using UDP transport.

## [0.10.1] - 2017-11-11
### Fixed
- Build system now includes `BCCrypto.dll` in `.unitypackage`.

## [0.10.0] - 2017-11-06
### Added
- New experimental rUDP socket protocol option.

### Changed
- Use string identifiers instead of byte arrays for compatibility across Lua, JSON, and client representations.

## [0.9.0] - 2017-10-17
### Added
- Advanced Matchmaking with custom filters and user properties.
- Expose Collation ID when client operations result in an error.

## [0.8.0] - 2017-08-01
### Added
- A paging cursor can now be serialized and restored.
- New storage partial update feature.
- New storage list feature.
- A new Unity code example which shows how to dispatch actions on the main thread.
- A session now exposes `.ExpiresAt` and `.Handle` from the token.

### Changed
- Add default builder for notification list and remove messages.
- A group self list operation now return the user's membership state with each group.
- A group leave operation now return a specific error code when the last admin attempts to leave.
- The client interface now uses action delegates instead of event handlers to support a proxy pattern.

## [0.7.0] - 2017-07-18
### Added
- A new Unity example scene which shows how to matchmake users.
- New `NIds` helper class and extension methods to compare byte arrays.
- Add new In-App Notification feature.
- Add new In-App Purchase Validation feature.   

### Changed
- Update client to support the new batch-orientated server protocol.

### Fixed
- Accept SSL certificates.
- Improve handling transport errors.
- Improve fetching global storage records.

## [0.6.1] - 2017-05-30
### Changed
- Remove unnecessary headers from HTTP requests.
- Update user fetch add handle method name to avoid a type cast.

### Fixed
- Accept SSL certificates on Android devices.
- Improve leaderboard list message to handle multiple filters.

## [0.6.0] - 2017-05-29
### Added
- New matchmaking feature.
- Optionally send match data to a subset of match participants.
- Expose a way to toggle `TCP_NODELAY` socket option.
- Send RPC messages to run custom code.
- Fetch users by handle.
- Add friend by handle.
- Filter by IDs in leaderboard list message.
- Storage messages can now set records with public read permission.

### Fixed
- Dispatch callbacks when sending match data.

## [0.5.1] - 2017-03-28
### Added
- Support for fetching groups by name.

## [0.5.0] - 2017-03-19
### Added
- Add support for dynamic leaderboards.
- Add error codes for error messages in server protocol.

### Changed
- Use preprocessor directive to skip WebGL specific code with other build profiles.
- Update session token parse code for user's handle.
- Update user presence protocol message to contain user handles.

## [0.4.2] - 2017-02-27
### Added
- Repackage client with Unity 5.4.0 support.

### Changed
- Setup logger in client transport.

## [0.4.1] - 2017-02-26
### Fixed
- Add '.jslib' files to Unity package builds.

## [0.4.0] - 2017-02-26
### Added
- Add WebGL support.

### Changed
- Update the package structure generated by the build system for simpler Asset Store submissions.

## [0.3.0] - 2017-02-18
### Added
- Add new impl of realtime match entities.

### Changed
- Merge match entities into single `INMatch`.

### Fixed
- Incoming realtime messages do not need collation.
- Add event handlers to `INClient` interface.

## [0.2.0] - 2017-02-12
### Added
- Add new impl and test cases for storage, friends, and groups.
- Add new impl for realtime and chat messages.

### Changed
- Do not close the connection on logout. It will be closed by the server.
- Update client usages for friend messages due to changes in server protocol.

### Fixed
- Fix various small test cases caused by changes in the server.

## [0.1.0] - 2017-01-14
### Added
- Initial public release.
