## Release Instructions

The current release infrastructure is built into the project's [build.gradle](https://github.com/heroiclabs/nakama-unity/blob/master/build.gradle). You'll need both Unity and `gradle` installed to generate release packages which can be uploaded to GitHub.

### New releases

To generate a new release via the Unity toolchain run `gradle unityPackage`. You can find the auto-generated Unity project in `"build/release/${version}/"`.

```
build/release/${version}/
├── Assets
│   ├── Nakama
│   └── Nakama.meta
├── Library
│   ├── AnnotationManager
│   ├── AssetImportState
│   ├── ...etc
├── Nakama.unitypackage
└── ProjectSettings
    ├── AudioManager.asset
    ├── ClusterInputManager.asset
    ├── ...etc
```

### Full release workflow

The development team use these steps to build and upload a release.

1. Update the `CHANGELOG.md`.

   Make sure to add the relevant `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, and `Security` sections as suggested by [keep a changelog](http://keepachangelog.com).

2. Update version in `build.gradle` and commit. i.e. `version = '0.3.0'` should become `version = '0.4.0'`.

   ```
   git add build.gradle CHANGELOG.md
   git commit -m "Nakama Unity 0.4.0 release."
   ```

3. Tag the release.

   __Note__ In source control good semver suggests a `"v"` prefix on a version. It helps group release tags.

   ```
   git tag -a v0.4.0 -m "v0.4.0"
   git push origin v0.4.0
   ```

4. Login and create a [new draft release](https://github.com/heroiclabs/nakama-unity/releases/new) on GitHub. Repeat the changelog in the release description. Then publish the release.

5. Add new `## [Unreleased]` section to start of `CHANGELOG.md`. Increment version in `build.gradle` and commit. i.e. `version = '0.4.0'` should now become `version = '0.5.0'`.

   ```
   git add build.gradle CHANGELOG.md
   git commit -m "Set new development version."
   git push origin master
   ```
