#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Aster.PMX2VRM.Editor
{
    public static class AsterWebGLBuild
    {
        // Unity Build Automation calls this method after scripts compile and
        // before it starts its normal WebGL player build.
        // Configure this method as the UBA Pre-export method:
        // Aster.PMX2VRM.Editor.AsterWebGLBuild.PreExport
        public static void PreExport()
        {
            ConfigureWebGLPlayer();
            Debug.Log("Aster UniVRM: WebGL player settings prepared for Unity Build Automation.");
        }

        [MenuItem("Aster/Build UniVRM WebGL Runtime")]
        public static void BuildWebGL()
        {
            ConfigureWebGLPlayer();

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

        private static void ConfigureWebGLPlayer()
        {
            // GitHub Pages/static hosting may not send Content-Encoding for
            // compressed Unity files, so enable Unity's decompression fallback.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
        }
    }
}
#endif
