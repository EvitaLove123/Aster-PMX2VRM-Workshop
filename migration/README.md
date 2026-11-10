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


## Phase 2 — VRM 1.0 core serializer (implementation started)

Added `src/vrm/three-vrm-exporter.js`:

- Uses Three.js `GLTFExporter` to produce a binary glTF 2.0 container.
- Adds the required `VRMC_vrm` extension with `specVersion`, metadata, and humanoid node references.
- Requires all 15 mandatory VRM 1.0 humanoid bones and rejects missing or ambiguous exported node names.
- Writes a correctly padded GLB JSON chunk and updates the GLB byte length.
- Reads the result back through the Three-vrm loader adapter before returning a Blob.
- Requires a model name, at least one author, and an HTTPS license URL. Defaults to the official VRM 1.0 license URL; license permissions default conservatively.

**Scope boundary:** this is a core serializer scaffold, not yet the complete exporter. It does not yet serialize VRMC_materials_mtoon, VRMC_springBone, expressions, look-at, constraints, or a thumbnail image. It is not wired to the PMX page's production download button. No runtime acceptance test has been run in a real browser yet. Do not use it as a finished converter.

### Phase 2 validation checklist

- [ ] Run [the browser smoke test](../tests/vrm10-export-smoke-test.html) and confirm Three-vrm reads the generated fixture as VRM 1.0.
- [ ] Validate the generated file against the official VRM 1.0 JSON schemas / validator.
- [ ] Confirm GLB chunk lengths and embedded resources.
- [ ] Confirm skinning, morph targets and texture references survive GLTFExporter.
- [ ] Only then connect the exporter to the PMX conversion UI.
