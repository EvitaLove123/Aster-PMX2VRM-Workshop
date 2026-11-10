# Three.js / Three-vrm migration

This work is being developed on `migration/three-vrm-browser`. The existing `main` branch remains the rollback point.

## Architecture decision

- Keep Three.js + MMDLoader for PMX/ZIP parsing and preview.
- Use `@pixiv/three-vrm` for VRM runtime loading, humanoid/expression/spring-bone inspection, and round-trip validation.
- Use Three.js `GLTFExporter` for the base glTF 2.0 binary scene.
- Implement VRM extension serialization as a separate, explicit export layer. Do not assume three-vrm exports VRM: its documented API is a GLTFLoader plugin.
- Implement VRM 1.0 and legacy VRM 0.x as separate serializers. Never label a plain GLB as a VRM.
- Keep the Unity/UniVRM project and current UI intact until browser export passes independent validators and target-app tests.

## Phases

1. **Runtime foundation (current):** add the three-vrm loader adapter and use it as the read-back validator for VRM files. No production export buttons are switched yet.
2. **VRM 1.0 exporter:** serialize the base GLB plus required `VRMC_vrm` data, then add MToon, expressions, and SpringBone extensions in validated increments.
3. **VRM 0.x exporter:** implement the legacy `VRM` extension independently; validate its metadata, humanoid, materials, blend shapes, and secondary animation.
4. **PMX conversion integration:** map all existing 49 humanoid roles to glTF node indices, preserve skinning/morph targets/textures, and author metadata/thumbnail.
5. **Compatibility and performance:** validate on Android Chrome, VRM validators, VRM Viewer, VRoid Hub, VRM Posing Mobile, and Virtual Droid where format support allows.
6. **Retirement:** only after acceptance tests pass, remove the Unity iframe, runtime bridge, UBA workflow, and `univrm-runtime/` directory.

## Important technical constraint

Three-vrm is not a drop-in VRM exporter. The official documentation describes it as a loader plugin for Three.js GLTFLoader. The export layer must write the VRM glTF extensions and preserve references to the correct glTF node, mesh, material, and texture indices. A generated file must pass a VRM validator and be read back through three-vrm before it is offered as a download.

## Current status

Phase 1 adapter is added on the migration branch. The production page still uses the existing Unity runtime until later phases are complete.
