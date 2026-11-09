using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace Aster.PMX2VRM
{
    /// <summary>
    /// Receives a GLB preview and explicit Humanoid role-to-node-name mappings
    /// from the browser, imports it through UniGLTF, and builds a Unity Humanoid Avatar.
    /// No VRM export is reported as successful by this stage.
    /// </summary>
    public sealed partial class AsterRuntimeBridge : MonoBehaviour
    {
        public static AsterRuntimeBridge Instance { get; private set; }

        private readonly StringBuilder _glbBase64 = new StringBuilder();
        private RuntimeGltfInstance _loadedGltf;
        private Avatar _builtAvatar;
        private string _pendingFileName = "AsterModel.glb";

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AsterPostMessage(string json);
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBridge()
        {
            if (Instance != null) return;
            new GameObject("AsterRuntimeBridge").AddComponent<AsterRuntimeBridge>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            gameObject.name = "AsterRuntimeBridge";
            DontDestroyOnLoad(gameObject);
        }

        public void BeginGlb(string fileName)
        {
            _glbBase64.Length = 0;
            _pendingFileName = string.IsNullOrWhiteSpace(fileName) ? "AsterModel.glb" : fileName;
            PostStatus("receiving", "Receiving GLB data from Workshop.");
        }

        public void AppendGlbChunk(string chunk)
        {
            if (!string.IsNullOrEmpty(chunk)) _glbBase64.Append(chunk);
        }

        public async void FinishGlbImport(string mappingJson)
        {
            try
            {
                if (_glbBase64.Length == 0) throw new Exception("No GLB data was received.");
                var bytes = Convert.FromBase64String(_glbBase64.ToString());
                _glbBase64.Length = 0;
                PostStatus("importing", "Importing GLB with official UniGLTF runtime API.");

                if (_loadedGltf != null)
                {
                    Destroy(_loadedGltf.gameObject);
                    _loadedGltf = null;
                }
                if (_builtAvatar != null)
                {
                    Destroy(_builtAvatar);
                    _builtAvatar = null;
                }

                _loadedGltf = await GltfUtility.LoadBytesAsync(
                    _pendingFileName,
                    bytes,
                    awaitCaller: new RuntimeOnlyAwaitCaller());

                if (_loadedGltf == null || _loadedGltf.Root == null)
                    throw new Exception("UniGLTF returned an empty runtime model.");

                _loadedGltf.ShowMeshes();
                _loadedGltf.EnableUpdateWhenOffscreen();

                var request = JsonUtility.FromJson<MappingRequest>(mappingJson ?? "{}");
                var result = BuildHumanoidAvatar(_loadedGltf.Root, request);
                _builtAvatar = result;
                var animator = _loadedGltf.Root.GetComponent<Animator>();
                if (animator == null) animator = _loadedGltf.Root.AddComponent<Animator>();
                animator.avatar = _builtAvatar;

                PostStatus("humanoid-ready",
                    "GLB imported. Unity Humanoid Avatar built and validated. VRM export is the next stage.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                PostStatus("error", ex.Message);
            }
        }

        private static Avatar BuildHumanoidAvatar(GameObject root, MappingRequest request)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (request == null || request.mappings == null)
                throw new Exception("Humanoid mapping data is missing.");

            var transforms = root.GetComponentsInChildren<Transform>(true);
            var byName = transforms.GroupBy(x => x.name)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            var human = new List<HumanBone>();
            var usedTransforms = new HashSet<Transform>();

            foreach (var item in request.mappings)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.slot) ||
                    string.IsNullOrWhiteSpace(item.boneName)) continue;

                if (!Enum.TryParse(item.slot, true, out HumanBodyBones bodyBone) ||
                    bodyBone == HumanBodyBones.LastBone) continue;

                if (!byName.TryGetValue(item.boneName, out var matches) || matches.Count == 0)
                    throw new Exception("Mapped bone was not found in imported GLB: " + item.boneName);
                if (matches.Count > 1)
                    throw new Exception("Mapped bone name is ambiguous in GLB: " + item.boneName);

                var boneTransform = matches[0];
                if (!usedTransforms.Add(boneTransform))
                    throw new Exception("One source bone is assigned to multiple Humanoid roles: " + item.boneName);

                human.Add(new HumanBone
                {
                    humanName = HumanTrait.BoneName[(int)bodyBone],
                    boneName = boneTransform.name,
                    limit = new HumanLimit { useDefaultValues = true }
                });
            }

            var required = new[]
            {
                HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Head,
                HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
                HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
                HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot
            };
            var mappedHumanNames = new HashSet<string>(human.Select(x => x.humanName));
            var missing = required.Where(x => !mappedHumanNames.Contains(HumanTrait.BoneName[(int)x])).ToArray();
            if (missing.Length > 0)
                throw new Exception("Required Humanoid roles are missing: " +
                    string.Join(", ", missing.Select(x => x.ToString())));

            var skeleton = transforms.Select(t => new SkeletonBone
            {
                name = t.name,
                position = t.localPosition,
                rotation = t.localRotation,
                scale = t.localScale
            }).ToArray();

            var description = new HumanDescription
            {
                human = human.ToArray(),
                skeleton = skeleton,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false
            };

            var avatar = AvatarBuilder.BuildHumanAvatar(root, description);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
            {
                if (avatar != null) UnityEngine.Object.Destroy(avatar);
                throw new Exception("Unity rejected the Humanoid skeleton. Check bone hierarchy, required mappings, and T-pose orientation.");
            }
            avatar.name = root.name + "_HumanoidAvatar";
            return avatar;
        }

        public void PostStatus(string state, string message)
        {
            var payload = JsonUtility.ToJson(new BridgeStatus
            {
                source = "aster-univrm",
                state = state,
                message = message,
                univrmVersion = "0.131.4"
            });
#if UNITY_WEBGL && !UNITY_EDITOR
            AsterPostMessage(payload);
#else
            Debug.Log(payload);
#endif
        }

        [Serializable] private sealed class MappingRequest { public HumanBoneMap[] mappings; }
        [Serializable] private sealed class HumanBoneMap { public string slot; public string boneName; }
        [Serializable] private sealed class BridgeStatus
        {
            public string source;
            public string state;
            public string message;
            public string univrmVersion;
        }
    }
}
