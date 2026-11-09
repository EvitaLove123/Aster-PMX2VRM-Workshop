#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Aster.PMX2VRM.Editor
{
    public static class AsterWebGLBuild
    {
        [MenuItem("Aster/Build UniVRM WebGL Runtime")]
        public static void BuildWebGL()
        {
            // Use gzip with Unity's JavaScript fallback because static GitHub Pages hosting
            // may not send the Content-Encoding header Unity's compressed player expects.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            const string output = "Build/WebGL";
            Directory.CreateDirectory(output);

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("Aster UniVRM WebGL build failed: " + report.summary.result);
                return;
            }

            Debug.Log("Aster UniVRM WebGL build completed: " + report.summary.totalSize + " bytes");
        }
    }
}
#endif
