# Aster PMX2VRM Lite

Android-first, browser-based PMX to VRM 0.x converter.

## Lite workflow

1. Load one PMX file, or a ZIP containing one PMX and its textures.
2. Preview the model in the browser.
3. Auto-map or manually correct the Humanoid bones.
4. Enter model name, author, and version metadata.
5. Export a VRM 0.x file locally in the browser.

## Technology

- Three.js + MMDLoader for PMX parsing and 3D preview
- JSZip for PMX/texture ZIP packages
- A browser-native VRM 0.x serializer under `src/vrm/`
- Three-vrm read-back inspection for the generated file
- No Unity runtime and no UniVRM dependency

## Important limitations

This is an experimental browser converter, not a guarantee of compatibility with every PMX model or VRM app. The current VRM 0.x serializer covers metadata and Humanoid references, with limited first-person / BlendShape support. MToon materialProperties, complete PMX material translation, MMD physics, SpringBone, and collider conversion are not complete. Test exported files in the target app before relying on them.

The PMX preview is not identical to every MMD renderer. Some toon shading, IK, physics, and legacy texture formats may differ. Preserve your original PMX and texture files.

## Run on GitHub Pages

Publish the `lite` branch from **Settings → Pages → Deploy from a branch → lite / root**, or use a separate Pages site. Open the published page in Android Chrome.

## Third-party libraries

Three.js, JSZip, and three-vrm are loaded through the import map / local source modules. See their upstream licenses before redistributing.
