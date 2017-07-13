Nakama Unity
============

> Unity client for Nakama server.

Nakama is an open-source distributed server for social and realtime games. For more information have a look at the [server documentation](https://heroiclabs.com/docs/); and for a quick list of build targets run `gradle tasks`.

The [client guide](https://heroiclabs.com/docs/clients/unity/) is available on the server documentation. The [client reference documentation](http://heroiclabs.github.io/nakama-unity/) is deployed on gh-pages.

If you encounter any issues with the server you can generate diagnostics for us with `nakama doctor`. Send these to support@heroiclabs.com or [open an issue](https://github.com/heroiclabs/nakama/issues). If you experience any issues with the client, it can be useful to enable trace (`.Trace(true)`) to produce logs and [open an issue](https://github.com/heroiclabs/nakama-unity/issues).

### Start client

You can download the client directly from the [Asset Store](https://www.assetstore.unity3d.com/en/#!/content/81338).

Have a look at the [quickstart guide](https://heroiclabs.com/docs/clients/unity/) for a full introduction on how to get started with the client in development and/or production.

To create a client and connect to the Nakama server with the default settings:

```csharp
using UnityEngine;
using System.Collections;
using Nakama;

public class NewBehaviourScript : MonoBehaviour {
  void Start() {
    INClient client = NClient.Default("defaultkey");
  }

  void Update() {
  }
}
```

### Build Settings for Android

Nakama Unity Client requires the full .Net 2.0 runtime. In Unity Build Settings window, change API Compatibility Level to .Net 2.0.

In addition to above, you'll need to change Stripping Level to either "Disabled" or "Strip Assemblies".

### Build Settings for iOS

Nakama Unity Client requires the full .Net 2.0 runtime. In Unity Build Settings window, change API Compatibility Level to .Net 2.0.

### Contribute

To build the codebase you will need to install these dependencies:

* __unity__ The Unity editor/compiler toolchain.
* __gradle__ A JVM-based build tool.
* __protobuf__ A toolchain used to create custom protocols.
* __doxygen__ A tool for generating documentation from annotated source.

You can then install code dependencies:

```
$> gradle nugetRestore
```

For development use the compile and test run cycle:

```
$> gradle nunit
```

To package a release use:

```
$> gradle unityPackage
```

#### Credits

The generated documentation theme is based on the work of [@Velron](https://github.com/Velron/doxygen-bootstrapped/).
