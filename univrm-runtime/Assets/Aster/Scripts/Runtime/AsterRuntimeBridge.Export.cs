using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UniGLTF;
using UnityEngine;
using UniGLTF.Extensions.VRMC_vrm;

namespace Aster.PMX2VRM
{
    public sealed partial class AsterRuntimeBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int AsterDownloadFile(string fileName, byte[] bytes, int length);
#endif
        private VRM.VRMMetaObject _exportMeta0;

        [Serializable]
        private sealed class ExportMetadata
        {
            public string name;
            public string version;
            public string author;
            public string contact;
            public string reference;
            public string allowedUser;
            public string license;
            public string commercial;
            public string violent;
            public string sexual;
            public string other;
        }

        [UnityEngine.Scripting.Preserve]
        public void ExportVrm0(string metadataJson)
        {
            try
            {
                var root = RequireHumanoidRoot();
                var meta = JsonUtility.FromJson<ExportMetadata>(metadataJson ?? "{}") ?? new ExportMetadata();
                if (_exportMeta0 != null) Destroy(_exportMeta0);
                _exportMeta0 = ScriptableObject.CreateInstance<VRM.VRMMetaObject>();
                _exportMeta0.Title = ValueOr(meta.name, root.name);
                _exportMeta0.Version = ValueOr(meta.version, "1.0");
                _exportMeta0.Author = ValueOr(meta.author, "Unknown");
                _exportMeta0.ContactInformation = meta.contact ?? "";
                _exportMeta0.Reference = meta.reference ?? "";
                _exportMeta0.OtherPermissionUrl = meta.other ?? "";
                _exportMeta0.OtherLicenseUrl = meta.other ?? "";
                _exportMeta0.ExporterVersion = "UniVRM 0.131.4";
                _exportMeta0.AllowedUser = ParseEnum(meta.allowedUser, VRM.AllowedUser.Everyone);
                _exportMeta0.LicenseType = ParseEnum(meta.license, VRM.LicenseType.Redistribution_Prohibited);
                _exportMeta0.CommercialUssage = ParseEnum(meta.commercial, VRM.UssageLicense.Disallow);
                _exportMeta0.ViolentUssage = ParseEnum(meta.violent, VRM.UssageLicense.Disallow);
                _exportMeta0.SexualUssage = ParseEnum(meta.sexual, VRM.UssageLicense.Disallow);

                var component = root.GetComponent<VRM.VRMMeta>();
                if (component == null) component = root.AddComponent<VRM.VRMMeta>();
                component.Meta = _exportMeta0;

                var settings = new GltfExportSettings { InverseAxis = Axes.Z };
                var exportingData = VRM.VRMExporter.Export(settings, root, new RuntimeTextureSerializer());
                DeliverVrm(SafeFileName(ValueOr(meta.name, root.name)) + "_VRM0.vrm", exportingData.ToGlbBytes(), "VRM", "vrm0-exported",
                    "Official UniVRM 0.x exporter completed.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                PostStatus("error", "VRM 0.x export failed: " + ex.Message);
            }
        }

        [UnityEngine.Scripting.Preserve]
        public void ExportVrm1(string metadataJson)
        {
            try
            {
                var root = RequireHumanoidRoot();
                var data = JsonUtility.FromJson<ExportMetadata>(metadataJson ?? "{}") ?? new ExportMetadata();
                var meta = new UniVRM10.VRM10ObjectMeta
                {
                    Name = ValueOr(data.name, root.name),
                    Version = ValueOr(data.version, "1.0"),
                    Authors = new List<string> { ValueOr(data.author, "Unknown") },
                    ContactInformation = data.contact ?? "",
                    References = string.IsNullOrWhiteSpace(data.reference) ? new List<string>() : new List<string> { data.reference },
                    ThirdPartyLicenses = data.other ?? "",
                    OtherLicenseUrl = data.other ?? "",
                    AvatarPermission = MapAvatarPermission(data.allowedUser),
                    ViolentUsage = string.Equals(data.violent, "Allow", StringComparison.OrdinalIgnoreCase),
                    SexualUsage = string.Equals(data.sexual, "Allow", StringComparison.OrdinalIgnoreCase),
                    CommercialUsage = string.Equals(data.commercial, "Allow", StringComparison.OrdinalIgnoreCase)
                        ? CommercialUsageType.corporation : CommercialUsageType.personalNonProfit,
                    CreditNotation = CreditNotationType.required,
                    Redistribution = !string.Equals(data.license, "Redistribution_Prohibited", StringComparison.OrdinalIgnoreCase),
                    Modification = string.Equals(data.license, "Redistribution_Prohibited", StringComparison.OrdinalIgnoreCase)
                        ? ModificationType.prohibited : ModificationType.allowModificationRedistribution
                };
                var bytes = UniVRM10.Vrm10Exporter.Export(new GltfExportSettings(), root, null,
                    new RuntimeTextureSerializer(), meta);
                DeliverVrm(SafeFileName(ValueOr(data.name, root.name)) + "_VRM1.vrm", bytes, "VRMC_vrm", "vrm1-exported",
                    "Official UniVRM 1.0 exporter completed.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                PostStatus("error", "VRM 1.0 export failed: " + ex.Message);
            }
        }

        private GameObject RequireHumanoidRoot()
        {
            if (!_humanoidReady || _loadedGltf == null || _loadedGltf.Root == null)
                throw new Exception("Import a GLB and complete a valid Unity Humanoid mapping first.");
            var animator = _loadedGltf.Root.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new Exception("A valid Unity Humanoid Avatar is required before VRM export.");
            return _loadedGltf.Root;
        }

        private void DeliverVrm(string fileName, byte[] bytes, string expectedExtension, string state, string message)
        {
            ValidateVrmGlb(bytes, expectedExtension);
#if UNITY_WEBGL && !UNITY_EDITOR
            if (AsterDownloadFile(fileName, bytes, bytes.Length) == 0)
                throw new Exception("The browser rejected the generated VRM download.");
#else
            Debug.Log("Generated " + fileName + " (" + bytes.Length + " bytes).");
#endif
            PostStatus(state, message + " File size: " + bytes.Length + " bytes.");
        }

        private static void ValidateVrmGlb(byte[] bytes, string expectedExtension)
        {
            if (bytes == null || bytes.Length < 20)
                throw new Exception("UniVRM returned an empty or truncated VRM file.");
            if (bytes[0] != 0x67 || bytes[1] != 0x6C || bytes[2] != 0x54 || bytes[3] != 0x46)
                throw new Exception("Export output does not have the GLB magic header.");
            if (BitConverter.ToUInt32(bytes, 4) != 2)
                throw new Exception("Export output is not a glTF 2.0 binary container.");
            if (BitConverter.ToUInt32(bytes, 8) != (uint)bytes.Length)
                throw new Exception("GLB declared length does not match the exported file size.");

            var jsonLength = BitConverter.ToUInt32(bytes, 12);
            if (BitConverter.ToUInt32(bytes, 16) != 0x4E4F534A || jsonLength > (uint)(bytes.Length - 20))
                throw new Exception("GLB JSON chunk header is invalid.");
            var json = Encoding.UTF8.GetString(bytes, 20, (int)jsonLength);
            var extensionKey = "\"" + expectedExtension + "\"";
            if (json.IndexOf(extensionKey, StringComparison.Ordinal) < 0)
                throw new Exception("GLB does not contain the expected VRM extension: " + expectedExtension);
        }

        private static string SafeFileName(string value)
        {
            foreach (var invalid in new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' })
                value = value.Replace(invalid, '_');
            return value.Trim();
        }

        private static string ValueOr(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            return Enum.TryParse(value, true, out T parsed) ? parsed : fallback;
        }

        private static AvatarPermissionType MapAvatarPermission(string value)
        {
            if (string.Equals(value, "OnlyAuthor", StringComparison.OrdinalIgnoreCase))
                return AvatarPermissionType.onlyAuthor;
            if (string.Equals(value, "ExplicitlyLicensedPerson", StringComparison.OrdinalIgnoreCase))
                return AvatarPermissionType.onlySeparatelyLicensedPerson;
            return AvatarPermissionType.everyone;
        }
    }
}
