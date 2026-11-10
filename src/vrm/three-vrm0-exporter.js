/**
 * Aster PMX2VRM — legacy VRM 0.x serializer (phase 3 foundation).
 *
 * Writes the monolithic extensions.VRM payload into a GLB produced by
 * Three.js GLTFExporter. This first increment covers metadata, humanoid and
 * first-person references. BlendShape, MToon/materialProperties and
 * secondaryAnimation are opt-in payloads added in later validated increments.
 */
import { GLTFExporter } from 'three/addons/exporters/GLTFExporter.js';
import { inspectVrm } from './three-vrm-runtime.js';

const REQUIRED_BONES = [
  'hips', 'spine', 'chest', 'neck', 'head',
  'leftUpperLeg', 'leftLowerLeg', 'leftFoot',
  'rightUpperLeg', 'rightLowerLeg', 'rightFoot',
  'leftUpperArm', 'leftLowerArm', 'leftHand',
  'rightUpperArm', 'rightLowerArm', 'rightHand',
];

const BONE_NAMES = new Set([
  ...REQUIRED_BONES,
  'upperChest', 'leftShoulder', 'rightShoulder',
  'leftToes', 'rightToes', 'leftEye', 'rightEye', 'jaw',
  ...['left', 'right'].flatMap((side) =>
    ['Thumb', 'Index', 'Middle', 'Ring', 'Little'].flatMap((finger) =>
      ['Proximal', 'Intermediate', 'Distal'].map((part) => side + finger + part)
    )
  ),
]);

const LICENSES = new Set([
  'Redistribution_Prohibited', 'CC0', 'CC_BY', 'CC_BY_NC',
  'CC_BY_SA', 'CC_BY_NC_SA', 'CC_BY_ND', 'CC_BY_NC_ND', 'Other',
]);

function requiredText(value, label) {
  if (typeof value !== 'string' || !value.trim()) {
    throw new Error(label + ' is required.');
  }
  return value.trim();
}

function makeMeta(input = {}) {
  const licenseName = input.licenseName || 'Redistribution_Prohibited';
  const allowedUserName = input.allowedUserName || 'OnlyAuthor';
  const violentUssageName = input.violentUssageName || 'Disallow';
  const sexualUssageName = input.sexualUssageName || 'Disallow';
  const commercialUssageName = input.commercialUssageName || 'Disallow';

  if (!LICENSES.has(licenseName)) throw new Error('Invalid VRM 0.x licenseName.');
  if (!['OnlyAuthor', 'ExplicitlyLicensedPerson', 'Everyone'].includes(allowedUserName)) {
    throw new Error('Invalid allowedUserName.');
  }
  for (const value of [violentUssageName, sexualUssageName, commercialUssageName]) {
    if (!['Disallow', 'Allow'].includes(value)) throw new Error('VRM 0.x usage permissions must be Allow or Disallow.');
  }
  if (licenseName === 'Other' && !input.otherLicenseUrl) {
    throw new Error('otherLicenseUrl is required when licenseName is Other.');
  }

  const meta = {
    title: requiredText(input.title || input.name, 'Model title'),
    author: requiredText(input.author, 'Model author'),
    allowedUserName,
    violentUssageName,
    sexualUssageName,
    commercialUssageName,
    licenseName,
  };

  for (const key of [
    'version', 'contactInformation', 'reference',
    'otherPermissionUrl', 'otherLicenseUrl',
  ]) {
    if (typeof input[key] === 'string' && input[key].trim()) meta[key] = input[key].trim();
  }
  if (Number.isInteger(input.texture) && input.texture >= 0) meta.texture = input.texture;
  return meta;
}

function readGlb(arrayBuffer) {
  const view = new DataView(arrayBuffer);
  if (view.byteLength < 20 ||
      view.getUint32(0, true) !== 0x46546c67 ||
      view.getUint32(4, true) !== 2) {
    throw new Error('GLTFExporter did not produce a valid GLB 2.0 file.');
  }
  if (view.getUint32(8, true) !== view.byteLength) {
    throw new Error('GLB length header is inconsistent.');
  }

  let offset = 12;
  let json = null;
  const chunks = [];
  while (offset + 8 <= view.byteLength) {
    const length = view.getUint32(offset, true);
    const type = view.getUint32(offset + 4, true);
    const start = offset + 8;
    const end = start + length;
    if (end > view.byteLength) throw new Error('GLB chunk exceeds file bounds.');
    const bytes = new Uint8Array(arrayBuffer, start, length);
    if (type === 0x4e4f534a) {
      if (json) throw new Error('GLB contains multiple JSON chunks.');
      json = JSON.parse(new TextDecoder().decode(bytes).trim());
    } else {
      chunks.push({ type, bytes: new Uint8Array(bytes) });
    }
    offset = end;
  }
  if (!json || !Array.isArray(json.nodes)) throw new Error('GLB JSON or nodes are missing.');
  return { json, chunks };
}

function writeGlb(json, chunks) {
  const jsonBytes = new TextEncoder().encode(JSON.stringify(json));
  const jsonLength = Math.ceil(jsonBytes.length / 4) * 4;
  const paddedJson = new Uint8Array(jsonLength);
  paddedJson.fill(0x20);
  paddedJson.set(jsonBytes);

  const totalLength = 12 + 8 + paddedJson.length +
    chunks.reduce((sum, chunk) => sum + 8 + chunk.bytes.length, 0);
  const output = new ArrayBuffer(totalLength);
  const view = new DataView(output);
  view.setUint32(0, 0x46546c67, true);
  view.setUint32(4, 2, true);
  view.setUint32(8, totalLength, true);

  let offset = 12;
  view.setUint32(offset, paddedJson.length, true);
  view.setUint32(offset + 4, 0x4e4f534a, true);
  new Uint8Array(output, offset + 8, paddedJson.length).set(paddedJson);
  offset += 8 + paddedJson.length;

  for (const chunk of chunks) {
    if (chunk.bytes.length % 4 !== 0) throw new Error('GLB chunk is not 4-byte aligned.');
    view.setUint32(offset, chunk.bytes.length, true);
    view.setUint32(offset + 4, chunk.type, true);
    new Uint8Array(output, offset + 8, chunk.bytes.length).set(chunk.bytes);
    offset += 8 + chunk.bytes.length;
  }
  return output;
}

function mapNodes(scene, gltfJson, mapping) {
  if (!mapping || typeof mapping !== 'object') throw new Error('Humanoid mapping is required.');

  const sceneNames = new Map();
  scene.traverse((object) => {
    if (!object.name) return;
    sceneNames.set(object.name, (sceneNames.get(object.name) || 0) + 1);
  });
  const exportedNames = new Map();
  gltfJson.nodes.forEach((node, index) => {
    if (!node.name) return;
    const values = exportedNames.get(node.name) || [];
    values.push(index);
    exportedNames.set(node.name, values);
  });

  const result = {};
  const byObject = new Map();
  for (const [bone, object] of Object.entries(mapping)) {
    if (!BONE_NAMES.has(bone)) throw new Error('Unknown VRM 0.x humanoid bone: ' + bone);
    if (!object || !object.name) continue;
    if (sceneNames.get(object.name) !== 1) {
      throw new Error('Humanoid node name is missing or duplicated in scene: ' + object.name);
    }
    const indices = exportedNames.get(object.name) || [];
    if (indices.length !== 1) {
      throw new Error('Humanoid node was not exported uniquely: ' + object.name);
    }
    result[bone] = { bone, node: indices[0], useDefaultValues: true };
    byObject.set(object, indices[0]);
  }

  const missing = REQUIRED_BONES.filter((bone) => !result[bone]);
  if (missing.length) throw new Error('Missing required VRM 0.x bones: ' + missing.join(', '));
  return { humanBones: Object.values(result), byObject };
}

function cleanOptionalPayload(payload, label) {
  if (payload == null) return undefined;
  if (typeof payload !== 'object' || Array.isArray(payload)) {
    throw new TypeError(label + ' must be an object.');
  }
  return payload;
}

/**
 * @param {import('three').Object3D} scene Root containing the complete avatar.
 * @param {object} options
 * @param {Record<string, import('three').Object3D>} options.humanoid VRM bone name -> Three.js Bone.
 * @param {object} options.meta Legacy VRM 0.x metadata.
 * @param {object} [options.firstPerson] Optional first-person settings.
 * @param {object} [options.blendShapeMaster] Future-compatible raw legacy payload.
 * @param {object} [options.secondaryAnimation] Future-compatible raw legacy payload.
 * @param {Array<object>} [options.materialProperties] Future-compatible legacy material payload.
 */
export async function exportVrm0(scene, options = {}) {
  if (!scene || typeof scene.traverse !== 'function') {
    throw new TypeError('A Three.js avatar scene is required.');
  }
  const meta = makeMeta(options.meta);
  scene.updateMatrixWorld(true);

  const exporter = new GLTFExporter();
  const baseGlb = await exporter.parseAsync(scene, {
    binary: true,
    onlyVisible: false,
    trs: false,
    includeCustomExtensions: true,
    maxTextureSize: options.maxTextureSize || Infinity,
  });
  if (!(baseGlb instanceof ArrayBuffer)) {
    throw new Error('GLTFExporter did not return binary GLB data.');
  }

  const { json, chunks } = readGlb(baseGlb);
  const humanoid = mapNodes(scene, json, options.humanoid);
  const head = options.humanoid.head;
  const headNode = humanoid.byObject.get(head);
  const firstPersonInput = options.firstPerson || {};
  const firstPersonBone = firstPersonInput.firstPersonBone
    ? humanoid.byObject.get(firstPersonInput.firstPersonBone)
    : headNode;
  if (!Number.isInteger(firstPersonBone)) {
    throw new Error('VRM 0.x firstPersonBone must resolve to an exported humanoid node.');
  }

  const firstPerson = {
    firstPersonBone,
    firstPersonBoneOffset: {
      x: Number(firstPersonInput.offset?.x) || 0,
      y: Number(firstPersonInput.offset?.y) || 0,
      z: Number(firstPersonInput.offset?.z) || 0,
    },
    meshAnnotations: Array.isArray(firstPersonInput.meshAnnotations)
      ? firstPersonInput.meshAnnotations
      : [],
    lookAtTypeName: firstPersonInput.lookAtTypeName === 'BlendShape' ? 'BlendShape' : 'Bone',
  };

  const vrm = {
    specVersion: '0.0',
    exporterVersion: 'Aster PMX2VRM Browser',
    meta,
    humanoid: {
      humanBones: humanoid.humanBones,
      armStretch: 0.05,
      legStretch: 0.05,
      upperArmTwist: 0.5,
      lowerArmTwist: 0.5,
      upperLegTwist: 0.5,
      lowerLegTwist: 0.5,
      feetSpacing: 0,
      hasTranslationDoF: false,
    },
    firstPerson,
  };

  const blendShapeMaster = cleanOptionalPayload(options.blendShapeMaster, 'blendShapeMaster');
  const secondaryAnimation = cleanOptionalPayload(options.secondaryAnimation, 'secondaryAnimation');
  if (blendShapeMaster) vrm.blendShapeMaster = blendShapeMaster;
  if (secondaryAnimation) vrm.secondaryAnimation = secondaryAnimation;
  if (Array.isArray(options.materialProperties)) vrm.materialProperties = options.materialProperties;

  json.extensions = json.extensions || {};
  json.extensions.VRM = vrm;
  json.extensionsUsed = Array.isArray(json.extensionsUsed) ? json.extensionsUsed : [];
  if (!json.extensionsUsed.includes('VRM')) json.extensionsUsed.push('VRM');

  const arrayBuffer = writeGlb(json, chunks);
  const checked = await inspectVrm(arrayBuffer);
  if (checked.report.format !== 'VRM 0.x') {
    throw new Error('Three-vrm read-back validation did not identify VRM 0.x.');
  }

  return {
    blob: new Blob([arrayBuffer], { type: 'model/gltf-binary' }),
    arrayBuffer,
    report: {
      ...checked.report,
      nodes: json.nodes.length,
      materials: (json.materials || []).length,
      meshes: (json.meshes || []).length,
      skins: (json.skins || []).length,
      extensions: json.extensionsUsed,
      humanoidBones: humanoid.humanBones.length,
      note: 'VRM 0.x core: metadata, humanoid and first-person are serialized. BlendShape, MToon material mapping and secondary animation need dedicated tested serializers.',
    },
  };
}
