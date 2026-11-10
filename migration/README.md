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


## Phase 3 — VRM 0.x core serializer (implementation scaffold)

Added `src/vrm/three-vrm0-exporter.js`:

- Writes the legacy monolithic `extensions.VRM` extension with `specVersion: "0.0"`.
- Writes VRM 0.x metadata using legacy property names and permission enums.
- Maps the required legacy humanoid bones to glTF node indices and rejects missing or ambiguous names.
- Writes a first-person bone reference and default offset.
- Preserves the base GLB binary chunk and reads the result back through Three-vrm.
- Serializes BlendShapeMaster groups when supplied: maps a Three.js mesh plus morph target name/index to glTF mesh and target indices, and validates bind weights and preset names.

**Scope boundary:** this is only the VRM 0.x core scaffold. It does not yet translate PMX material settings into MToon materialProperties, map GLTF morph target indices into BlendShape groups, convert PMX hair/cloth physics into secondaryAnimation, or embed a thumbnail. It is not wired to the production PMX download button. The smoke test is source-only and has not been run in a real browser.

### VRM 0.x validation checklist

- [ ] Run [the browser smoke test](../tests/vrm0-export-smoke-test.html) and confirm Three-vrm identifies the result as VRM 0.x.
- [ ] Validate the GLB against the official VRM 0.0 JSON schemas.
- [ ] Verify legacy humanoid requirements, T-pose, -Z facing and bone transforms.
- [ ] Add tested materialProperties / MToon conversion.
- [ ] Add tested BlendShapeMaster mapping and thumbnail embedding.
- [ ] Add tested secondaryAnimation bone groups and collider groups.
- [ ] Connect to PMX conversion only after real fixture and target-app validation.
