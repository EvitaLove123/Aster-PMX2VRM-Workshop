# Aster UniVRM Runtime

Separate Unity runtime project for the Aster PMX2VRM Workshop.

## Pinned dependencies

- Unity: 2022.3 LTS
- UniVRM: v0.131.4
- UniGLTF: v0.131.4
- VRM 0.x package: v0.131.4
- VRM 1.0 package: v0.131.4

The official UPM packages are pinned to the same UniVRM release tag in
`Packages/manifest.json`.

## Current implementation status

- [x] Unity project scaffold and WebGL build menu
- [x] Official UniVRM/UniGLTF package references pinned to v0.131.4
- [x] Browser-to-Unity WebGL message bridge
- [x] Chunked base64 GLB receive methods (`BeginGlb`, `AppendGlbChunk`, `FinishGlbImport`)
- [x] Runtime GLB import through official `UniGLTF.GltfUtility.LoadBytesAsync`
- [x] Explicit role-to-node mapping into Unity `HumanDescription`
- [x] Unity Humanoid Avatar creation and `isValid` / `isHuman` validation
- [x] Workshop browser bridge for chunked GLB and metadata transfer
- [x] Browser download bridge for generated VRM files
- [x] Basic GLB header and expected VRM-extension check before download
- [x] Runtime call to official VRM 0.x exporter (`VRM.VRMExporter.Export`)
- [x] Runtime call to official VRM 1.0 exporter (`Vrm10Exporter.Export`)
- [x] GitHub Actions workflow scaffold for Unity WebGL build and Pages deployment
- [ ] Configure `UNITY_LICENSE` secret and run a successful cloud build
- [ ] Deploy runtime and test the complete browser-to-Unity round trip
- [ ] Test GLB import and AvatarBuilder with real PMX-derived models
- [ ] Complete expressions, MMD materials, SpringBone, and collider conversion
- [ ] Validate exported VRM 0.x and VRM 1.0 files in independent validators/apps

**Important:** The GLB import, Unity Humanoid conversion, and official VRM 0.x/1.0 exporter call sites are present in source. They have not yet been compiled or tested in Unity/WebGL, so neither export format is considered verified. The browser bridge and export controls are wired, but require a successful deployed WebGL runtime before they can work. Do not treat source implementation as a successful build or a validated VRM.

## Browser bridge protocol

The WebGL player creates `AsterRuntimeBridge` automatically at startup. The
Workshop should send data to the Unity instance using `SendMessage`:

1. `BeginGlb(fileName)`
2. One or more `AppendGlbChunk(base64Chunk)` calls.
3. `FinishGlbImport(mappingJson)`

The JSON payload has this shape. The runtime reads `mappings` for Humanoid construction and `metadata` for export:

```json
{
  "mappings": [
    { "slot": "hips", "boneName": "Hips" },
    { "slot": "spine", "boneName": "Spine" },
    { "slot": "leftThumbProximal", "boneName": "ThumbProximal_L" }
  ],
  "metadata": {
    "name": "Avatar", "version": "1.0", "author": "Creator",
    "allowedUser": "Everyone", "license": "CC_BY",
    "commercial": "Disallow", "violent": "Disallow", "sexual": "Disallow"
  }
}
```

The `slot` value must match a Unity `HumanBodyBones` enum name (case-insensitive). The `boneName` must exactly match one unique node name in the imported GLB. Missing, duplicate, or ambiguous assignments are rejected instead of guessed. Legacy metadata values are translated to each VRM specification; some VRM 1.0 license concepts do not have a one-to-one equivalent in the current UI.

## Open in Unity

1. Install Unity 2022.3 LTS with the WebGL Build Support module.
2. Open this `univrm-runtime` folder as a Unity project.
3. Let Unity resolve the pinned UPM packages.
4. Open **Aster → Build UniVRM WebGL Runtime**.

The Unity runtime bridge is created automatically when the player starts.

## Android/cloud workflow

The Unity Editor and WebGL build cannot run inside Chrome on Android. The repository
contains `.github/workflows/build-univrm-webgl.yml`, which uses GameCI to build the
Unity project and deploys the Workshop plus runtime to GitHub Pages. Before the first
successful run:

1. Add the appropriate Unity license secrets in Settings → Secrets and variables → Actions. For Personal: `UNITY_LICENSE` (contents of an already activated `.ulf` file), `UNITY_EMAIL`, and `UNITY_PASSWORD`. For Professional: `UNITY_SERIAL`, `UNITY_EMAIL`, and `UNITY_PASSWORD`.
2. Set GitHub Pages source to **GitHub Actions** (Settings → Pages).
3. Run the **Build and deploy Aster UniVRM WebGL runtime** workflow from the Actions tab.

**Personal-license limitation:** Unity's current documentation says Unity Hub is the supported activation method for Unity Personal; manual `.alf` activation is not supported. A valid Personal `.ulf` must already be available from a prior Hub activation, or the build needs access to a suitable licensed runner/seat.

The workflow does not contain license credentials. Never commit Unity license files,
passwords, or activation credentials. If any required secret is absent or invalid, the
build cannot activate Unity and will fail before compiling the runtime.

## Official references

- https://github.com/vrm-c/UniVRM
- https://github.com/vrm-c/UniVRM/releases/tag/v0.131.4
- https://github.com/vrm-c/UniVRM/blob/v0.131.4/Packages/UniGLTF/Runtime/UniGLTF/IO/GltfUtility.cs

## Unity Build Automation compatibility

The runtime now exposes a Unity Build Automation pre-export hook:

`Aster.PMX2VRM.Editor.AsterWebGLBuild.PreExport`

For a WebGL target, configure the target's **Project subfolder path** as
`univrm-runtime`, select Unity `2022.3`, and set the **Pre-export method**
to the fully qualified method above. The hook applies the WebGL gzip and
decompression-fallback settings; Unity Build Automation performs the normal
WebGL player build for the configured target. This avoids depending on the
GameCI-specific `buildMethod` setting in the existing GitHub Actions workflow.

Unity's documentation confirms that Build Automation supports WebGL, GitHub
repositories, Unity 2022.3, project subfolders, and pre-export methods. However,
the user's Unity Dashboard access and the licensing/entitlement path have not
been verified. This repository change is preparation only: no cloud build has
been run, and no VRM exporter output has been validated.
