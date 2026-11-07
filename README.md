# Aster PMX2VRM Workshop

Android-first project scaffold for a browser-based PMX to VRM 0.x workflow.

> **Current status:** Prototype scaffold only. This repository does not yet provide a complete PMX-to-VRM converter. Do not treat the current interface as a production exporter.

## Project goals

- PMX-only input for the first development phase
- English-only website interface
- Preserve original source bone names and show English Humanoid labels separately
- Automatic Humanoid mapping with manual correction
- Automatic A-pose to T-pose preparation with manual adjustment
- VRM 0.x export
- Android Chrome and GitHub Pages support

## Technical targets

- VRM 0.x format
- UniVRM 0.106.0-compatible metadata
- AlwaysUnlit and MToon material support
- Preserve textures, materials, and model appearance
- Preserve facial expressions and blendshape data where conversion is supported
- Support secondary animation data, including spring bones and colliders, where conversion is supported

These are development targets, not verified compatibility claims. Output files must pass structural validation and be tested with independent VRM viewers and tools.

## Current repository contents

- `index.html` — mobile-friendly English project interface
- `README.md` — project scope and development status

## Planned development phases

1. Validate PMX parsing and texture loading on Android Chrome.
2. Inspect skeleton hierarchy, source bone names, bind pose, and model scale.
3. Implement automatic Humanoid mapping, including finger bones, and a manual mapping editor.
4. Analyze arm pose and implement safe, reversible A-pose to T-pose adjustments.
5. Convert materials, textures, facial expressions, and blendshape data.
6. Convert supported secondary animation data, including spring bones and colliders.
7. Implement VRM 0.x export and structural validation.
8. Test exported files with independent VRM viewers, validators, and representative models.

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

## Safety and model integrity

MMD rigs may contain IK, append transforms, unusual local axes, and model-specific skin weights. Never apply a universal 180-degree bone rotation or overwrite source weights without validation. Keep the original model data recoverable during editing.

## GitHub Pages

1. Open **Settings → Pages**.
2. Under **Build and deployment**, choose **Deploy from a branch**.
3. Select branch `main` and folder `/ (root)`.
4. Save and wait for the deployment to finish.
5. Open the Pages URL shown in the Pages settings.

## Development notes

The project is designed for Android-first browser use. Third-party libraries and their licenses must be reviewed before bundling or redistributing them.

## License

See [LICENSE](LICENSE) for the project license.