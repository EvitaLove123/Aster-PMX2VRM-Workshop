# Standalone VRM 0.x Exporter — No UniVRM

## Status

Architecture branch only. This document is a development plan, not a working exporter. The current `main` branch and its deployed page remain unchanged.

## Non-negotiable constraints

- No UniVRM, UniGLTF, Unity runtime, or Unity WebGL build.
- No fabricated `UniGLTF` metadata or claims that output was made by UniVRM.
- Target VRM 0.x first. Do not expose VRM 1.0 export in this branch.
- Keep PMX parsing and preview separate from export so preview can be disposed before export.
- Preserve the source PMX scene and provide a clear failure report rather than silently dropping unsupported content.
- No model upload to a server; processing remains in the browser.

## Proposed pipeline

1. Parse PMX and textures with the existing Three.js MMDLoader path.
2. Inspect source model and validate that all required humanoid roles map to unique, valid bones.
3. Build a dedicated export scene without an unnecessary full-scene clone. Release preview-only renderer resources before heavy export work where safe.
4. Convert geometry, skinning, materials, and morph targets to glTF-compatible data.
5. Export a GLB with Three.js GLTFExporter.
6. Post-process the GLB JSON chunk to add a standards-conforming VRM 0.x extension:
   - `extensionsUsed: ["VRM"]`
   - `extensions.VRM.meta`
   - `extensions.VRM.humanoid.humanBones`
   - `extensions.VRM.firstPerson`
   - `extensions.VRM.blendShapeMaster`
   - `extensions.VRM.secondaryAnimation`
   - `extensions.VRM.materialProperties`
7. Rebuild the GLB container with correct JSON/BIN chunk lengths and 4-byte alignment.
8. Run structural checks before offering a download. Mark unimplemented sections honestly and never fabricate values.

## Implementation gates

### Gate A — minimal valid VRM 0.x
- One skinned mesh and a minimal valid humanoid.
- Real node indices derived from the exported glTF; never infer indices from PMX bone array positions.
- Valid VRM extension structure and GLB container.
- Verify with an independent VRM 0.x parser/validator.

### Gate B — model fidelity
- Preserve all meshes, textures, material assignments, skin weights, and supported morph targets.
- Map MMD morphs to VRM blendshape groups only when source morph semantics can be identified.
- Convert MMD toon materials conservatively; report unsupported shader features.

### Gate C — secondary motion and compatibility
- Convert PMX rigid bodies/joints only where a correct VRM 0.x SpringBone equivalent can be produced.
- Validate Secondary Animation collider groups and bone groups.
- Test output in independent viewers and target Android apps.

## Memory and Android constraints

- Avoid simultaneous PMX buffer, cloned scene, GLB buffer, base64 strings, and duplicate copies.
- Prefer Blob/ArrayBuffer transfer and chunked processing where supported; do not base64-encode large GLB payloads.
- Add preflight estimates and a clear warning for high-memory models.
- Catch allocation/export errors where possible; browsers may terminate a tab before JavaScript can catch an out-of-memory failure.
- Test with small, medium, and large PMX fixtures on Android Chrome. A 21.85 MB PMX is not itself a reliable predictor of peak memory.

## Current known blocker

The current `main` page sends GLB data through a Base64 `postMessage` bridge to a separate UniVRM Unity runtime. The current README and page both describe the Unity/UniVRM path. This branch must remove that path only after the standalone exporter passes Gate A; until then, do not claim the export button works.
