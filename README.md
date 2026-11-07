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

## Compatibility targets

- VRM 0.x
- UniVRM 0.106.0-compatible metadata
- AlwaysUnlit and MToon materials
- VRM Posing Mobile
- Napharmonia
- Virtual Droid

These are targets, not verified compatibility claims. Every export must be tested with the target applications and representative models.

## Current repository contents

- `index.html` — mobile-friendly English project interface
- `README.md` — project scope and development status

## Planned development phases

1. Validate PMX parsing and texture loading on Android Chrome.
2. Inspect skeleton hierarchy, source bone names, and bind pose.
3. Implement automatic Humanoid mapping and a manual mapping editor.
4. Analyze arm pose and implement safe, reversible A-pose to T-pose adjustments.
5. Convert materials, textures, expressions, and secondary animation data.
6. Implement VRM 0.x export and structural validation.
7. Test exported files in VRM Posing Mobile, Napharmonia, and Virtual Droid.

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