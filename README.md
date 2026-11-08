# Aster PMX2VRM Workshop

Android-first browser project for inspecting MMD PMX models and preparing a future PMX to VRM 0.x workflow.

> **Current status:** The repository now includes an early PMX import and interactive 3D preview prototype. It is not a complete PMX-to-VRM converter. Browser integration and representative-model testing are still required.

## Project goals

- PMX input for the first development phase
- English-only website interface
- Android Chrome-friendly layout
- Preserve original source bone names and show English Humanoid labels separately
- Automatic Humanoid mapping with manual correction
- Safe, reversible A-pose to T-pose preparation
- VRM 0.x export with structural validation

## Current implementation

- `index.html` — mobile-friendly interface with PMX/ZIP import, 3D preview, and an early core-body Humanoid mapping editor
- Three.js scene, camera, lighting, grid, and touch-friendly OrbitControls
- MMDLoader integration for parsing PMX data in the browser
- Optional selection of external texture files, matched by filename
- Interactive model preview and basic mesh, material, and bone counts
- Optional skeleton helper overlay
- First Humanoid mapping editor for 19 core body roles, with automatic suggestions and manual dropdown correction

**Testing status:** The code has been committed, but it has not yet been verified against the user's PMX files or on Android Chrome. Some MMD material, toon, IK, physics, and texture-path behavior may differ between models. Treat this as an experimental preview until tested.

## Planned development phases

1. Test PMX parsing and texture loading with representative models on Android Chrome.
2. Improve texture path resolution and report missing texture references.
3. Inspect skeleton hierarchy, source bone names, bind pose, model scale, and mesh deformation.
4. Improve Japanese/English/MMD finger-bone alias coverage and validate parent-child hierarchy.
5. Analyze arm pose and implement safe, reversible A-pose to T-pose adjustments.
6. Convert materials, textures, facial expressions, and blendshape data.
7. Convert supported secondary animation data, including spring bones and colliders.
8. Implement VRM 0.x export and structural validation.
9. Test exported files with independent VRM viewers, validators, and representative models.

## Export validation checklist

- Required VRM metadata and Humanoid bone assignments
- Valid skeleton hierarchy and bind pose
- Valid skin weights and mesh deformation
- Texture references and material settings
- Facial expression and blendshape references
- Secondary animation and collider references
- Consistent model orientation, scale, and floor placement
- No missing mesh data or broken references

A successful structural check does not guarantee that every application will interpret every model identically.

## Known limitations

- The current prototype does not export VRM files.
- Humanoid mapping and pose editing are not implemented.
- ZIP import currently expects exactly one PMX file per archive. Texture matching is best-effort by archive path and filename.
- Full MMD toon shading, IK animation, physics, and all PMX material extensions are not guaranteed in the preview.
- The page loads Three.js modules from a CDN, so an internet connection is required.

## Model integrity

MMD rigs may contain IK, append transforms, unusual local axes, and model-specific skin weights. Never apply a universal 180-degree bone rotation or overwrite source weights without validation. Keep the original model data recoverable during editing.

## GitHub Pages

1. Open **Settings → Pages**.
2. Under **Build and deployment**, choose **Deploy from a branch**.
3. Select branch `main` and folder `/ (root)`.
4. Save and wait for the deployment to finish.
5. Open the Pages URL shown in the Pages settings.

## Third-party library

The preview uses Three.js and its MMDLoader example module via jsDelivr CDN. Review the upstream Three.js license and example-module terms before bundling or redistributing dependencies.

## License

See [LICENSE](LICENSE) for the project license.