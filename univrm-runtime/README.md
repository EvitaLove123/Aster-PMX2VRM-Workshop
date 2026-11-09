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
- [ ] Connect the Workshop UI to a deployed WebGL runtime
- [ ] Test GLB import and AvatarBuilder with real PMX-derived models
- [ ] Apply VRM 0.x metadata, expressions, materials and secondary animation
- [ ] Export VRM 0.x through the official VRM exporter
- [ ] Apply VRM 1.0 metadata, expressions, materials and spring settings
- [ ] Export VRM 1.0 through the official Vrm10Exporter
- [ ] Return the generated VRM file to the Workshop and verify it

**Important:** The GLB import and Unity Humanoid conversion source code is now present,
but it has not yet been compiled or tested in the Unity Editor/WebGL build. The
Workshop does not yet call this runtime. VRM 0.x and VRM 1.0 export are not implemented
yet, and the HTML export buttons must remain disabled.

## Browser bridge protocol

The WebGL player creates `AsterRuntimeBridge` automatically at startup. The
Workshop should send data to the Unity instance using `SendMessage`:

1. `BeginGlb(fileName)`
2. One or more `AppendGlbChunk(base64Chunk)` calls.
3. `FinishGlbImport(mappingJson)`

The JSON mapping payload has this shape:

```json
{
  "mappings": [
    { "slot": "hips", "boneName": "Hips" },
    { "slot": "spine", "boneName": "Spine" },
    { "slot": "leftThumbProximal", "boneName": "ThumbProximal_L" }
  ]
}
```

The `slot` value must match a Unity `HumanBodyBones` enum name (case-insensitive).
The `boneName` must exactly match one unique node name in the imported GLB. Missing,
duplicate, or ambiguous assignments are rejected instead of guessed.

## Open in Unity

1. Install Unity 2022.3 LTS with the WebGL Build Support module.
2. Open this `univrm-runtime` folder as a Unity project.
3. Let Unity resolve the pinned UPM packages.
4. Open **Aster → Build UniVRM WebGL Runtime**.

The Unity runtime bridge is created automatically when the player starts.

## Android/cloud workflow

The Unity Editor and WebGL build cannot run inside Chrome on Android. A cloud CI
runner can build this project, but it needs a valid Unity Editor installation and
Unity license activation configured securely by the repository owner. Never commit
Unity license files, passwords, or activation credentials.

## Official references

- https://github.com/vrm-c/UniVRM
- https://github.com/vrm-c/UniVRM/releases/tag/v0.131.4
- https://github.com/vrm-c/UniVRM/blob/v0.131.4/Packages/UniGLTF/Runtime/UniGLTF/IO/GltfUtility.cs
