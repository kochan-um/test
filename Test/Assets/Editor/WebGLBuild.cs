using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebGLBuild
{
    [MenuItem("Tools/Build/WebGL (Vercel)")]
    public static void BuildForVercel()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("WebGL Build", "EditorBuildSettings にシーンが登録されていません。", "OK");
            return;
        }

        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var outDir = Path.Combine(projectRoot, "WebGLBuild");
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        // 推奨設定（必要に応じて調整）
        // Vercel は配信時に自動圧縮されるため、Unity 側は圧縮を無効化
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.threadsSupport = false; // COOP/COEP 設定が不要な安全側

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.RevealInFinder(outDir);
            EditorUtility.DisplayDialog("WebGL Build", $"Success: {report.summary.totalSize / (1024 * 1024)} MB", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("WebGL Build", $"Failed: {report.summary.result}", "OK");
        }
    }
}
