# Aster PMX2VRM Workshop

Android-first browser project for inspecting MMD PMX models and preparing a PMX-to-VRM workflow.

> **Current status:** The browser prototype imports PMX/ZIP files, previews the model, and can generate a GLB preview. A separate Unity project now contains the official UniVRM runtime import, Humanoid conversion, and VRM 0.x/1.0 exporter call sites. The Unity/WebGL project has not yet been compiled or validated, so generated VRM files are not verified yet.

## Project goals

- PMX input and ZIP texture package support
- English-only, Android Chrome-friendly interface
- Preserve source bone names and display English Humanoid role labels separately
- Side-aware automatic body and finger mapping with manual correction
- Official UniVRM runtime for GLB import and VRM 0.x / 1.0 export
- Preserve model materials, textures, expressions, and supported secondary animation wherever conversion is validated

## Browser implementation

- `index.html` — PMX/ZIP import, touch-friendly 3D preview, skeleton overlay, metadata editor, Humanoid mapping, and GLB preview export
- Three.js MMDLoader integration for PMX parsing
- ZIP texture extraction and best-effort texture path matching
- 49 Humanoid mapping roles: core body plus 30 finger roles, with manual dropdown correction
- GLB preview export from the loaded PMX model
- Browser message bridge to the separate Unity runtime
- Export buttons wait for the official runtime and a validated Unity Humanoid Avatar

## Official UniVRM runtime

The separate `univrm-runtime/` Unity project is pinned to:

- Unity 2022.3 LTS
- Official UniVRM / UniGLTF v0.131.4
- VRM 0.x package `com.vrmc.univrm`
- VRM 1.0 package `com.vrmc.vrm`

Runtime source includes:

- GLB import through official `UniGLTF.GltfUtility.LoadBytesAsync`
- Explicit source-node to `HumanBodyBones` mapping
- Unity `AvatarBuilder.BuildHumanAvatar` and `isValid` / `isHuman` validation
- Runtime calls to official `VRM.VRMExporter.Export` and `UniVRM10.Vrm10Exporter.Export`
- VRM metadata translation for the current browser metadata fields
- WebGL message and browser file-download bridges

**Important:** These are source implementations, not a successful Unity build. They still need a cloud WebGL build, Android Chrome runtime test, representative PMX-derived GLB tests, and independent VRM validation. MMD expressions, full material conversion, SpringBone, and collider conversion are not complete.

## Build and deploy the WebGL runtime from Android

The repository includes `.github/workflows/build-univrm-webgl.yml` using GameCI.

1. Add the Unity license as an Actions secret named `UNITY_LICENSE`, plus `UNITY_EMAIL` and `UNITY_PASSWORD`, in **Repository Settings → Secrets and variables → Actions**.
2. Set **Repository Settings → Pages → Build and deployment → Source** to **GitHub Actions**.
3. Open the **Actions** tab.
4. Run **Build and deploy Aster UniVRM WebGL runtime**.

Never commit Unity license files, passwords, or activation credentials. GameCI uses the three repository secrets to reactivate Unity in its cloud runner. The workflow will not build Unity until a valid license secret is available.

## Model integrity and known limitations

MMD rigs may contain IK, append transforms, unusual local axes, and model-specific skin weights. Never apply a universal 180-degree bone rotation or overwrite source weights without validation. Keep the original model data recoverable during editing.

The browser preview is experimental. MMD toon shading, IK animation playback, physics, and all PMX material extensions are not guaranteed. ZIP import currently expects exactly one PMX file per archive.

## GitHub Pages

The Pages deployment is now managed by the GitHub Actions workflow above. Do not switch it back to branch-based deployment; the workflow publishes both the Workshop page and the generated Unity WebGL runtime.

## Third-party libraries

The browser preview uses Three.js and its MMDLoader / GLTFExporter modules via jsDelivr CDN, plus JSZip. Review the upstream licenses and example-module terms before bundling or redistributing dependencies.

## License

See [LICENSE](LICENSE) for the project license.
