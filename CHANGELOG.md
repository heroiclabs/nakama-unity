# Change Log
All notable changes to this project are documented below.

The format is based on [keep a changelog](http://keepachangelog.com/) and this project uses [semantic versioning](http://semver.org/).

## [Unreleased]

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
