# Aster UniVRM Runtime

This is a separate Unity runtime project for the Aster PMX2VRM Workshop.

## Pinned dependencies

- Unity: 2022.3 LTS
- UniVRM: v0.131.4
- UniGLTF: v0.131.4
- VRM 0.x package: v0.131.4
- VRM 1.0 package: v0.131.4

The three official UPM packages are pinned to the same UniVRM release tag in
`Packages/manifest.json`. UniVRM v0.131.0 and later use the `Packages` paths.

## Current implementation status

- [x] Unity project skeleton
- [x] Official UniVRM UPM dependencies pinned to v0.131.4
- [x] Browser ↔ Unity WebGL message bridge skeleton
- [x] Unity Editor menu for a WebGL build
- [ ] Add and configure the runtime scene
- [ ] Import Workshop GLB at runtime
- [ ] Validate and map Unity Humanoid bones
- [ ] Apply VRM metadata, materials, expressions and spring settings
- [ ] Export VRM 0.x and VRM 1.0 through official UniVRM runtime APIs
- [ ] Return the generated VRM file to the Workshop

**Important:** This repository folder is the integration foundation, not yet a working
VRM converter. The bridge currently reports receipt/status only; it does not create
a VRM file. The export buttons in the HTML Workshop must remain disabled until the
runtime exporter is implemented and tested.

## Open in Unity

1. Install Unity 2022.3 LTS with the WebGL Build Support module.
2. Open this `univrm-runtime` folder as a Unity project.
3. Let Unity resolve the pinned UPM packages.
4. Create/configure the runtime scene and add an active GameObject named
   `AsterRuntimeBridge` with the `AsterRuntimeBridge` component.
5. Use **Aster → Build UniVRM WebGL Runtime**.

## Android/cloud workflow

The Unity Editor and WebGL build cannot run inside Chrome on Android. A cloud CI
runner can build this project, but it must have a valid Unity Editor installation
and Unity license activation configured securely by the repository owner. Never
commit Unity license files, passwords, or activation credentials.

## Official references

- https://github.com/vrm-c/UniVRM
- https://github.com/vrm-c/UniVRM/releases/tag/v0.131.4
