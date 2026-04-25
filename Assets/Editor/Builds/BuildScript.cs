using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    // ------------------------
    // Scenes
    // ------------------------

    private static string[] GetScenes()
    {
        return EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
    }

    // ------------------------
    // Build entry points
    // ------------------------

    public static void BuildAndroidDev() =>
        Build(BuildPlatform.Android, BuildType.Development);

    public static void BuildAndroidRelease() =>
        Build(BuildPlatform.Android, BuildType.Release);

    public static void BuildiOSDev() =>
        Build(BuildPlatform.iOS, BuildType.Development);

    public static void BuildiOSRelease() =>
        Build(BuildPlatform.iOS, BuildType.Release);

    // ------------------------
    // CLI entry point (GitHub Actions)
    // ------------------------

    public static void BuildFromCommandLine()
    {
        var platformArg = GetArg("-platform");
        var buildTypeArg = GetArg("-buildType");
        var buildNumberArg = GetOptionalArg("-buildNumber");

        if (!Enum.TryParse(platformArg, true, out BuildPlatform platform))
            throw new Exception($"Invalid platform: {platformArg}");

        if (!Enum.TryParse(buildTypeArg, true, out BuildType buildType))
            throw new Exception($"Invalid build type: {buildTypeArg}");

        BuildMetadata.SetBuildNumber(buildNumberArg);

        Build(platform, buildType);
    }

    // ------------------------
    // Core build pipeline
    // ------------------------

    private static void Build(BuildPlatform platform, BuildType buildType)
    {
        Debug.Log($"Starting build: {platform} | {buildType}");

        SwitchPlatform(platform);
        ApplyPlatformSettings(platform, buildType);
        ApplyDefines(platform, buildType);

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = GetBuildPath(platform, buildType),
            target = GetBuildTarget(platform),
            options = GetBuildOptions(buildType)
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Build failed: {summary.result}");
            throw new Exception("Build failed");
        }

        Debug.Log($"Build succeeded: {summary.outputPath}");
    }

    // ------------------------
    // Platform switching
    // ------------------------

    private static void SwitchPlatform(BuildPlatform platform)
    {
        var target = GetBuildTarget(platform);
        var group = BuildPipeline.GetBuildTargetGroup(target);

        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            Debug.Log($"Switching platform to {target}");
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        }
    }

    // ------------------------
    // Platform settings
    // ------------------------

    private static void ApplyPlatformSettings(BuildPlatform platform, BuildType buildType)
    {
        switch (platform)
        {
            case BuildPlatform.Android:
                // Dev = APK, Release = AAB
                EditorUserBuildSettings.buildAppBundle = buildType == BuildType.Release;
                PlayerSettings.Android.useCustomKeystore = buildType == BuildType.Release;
                break;

            case BuildPlatform.iOS:
                PlayerSettings.iOS.buildNumber = DateTime.Now.ToString("yyyyMMddHHmm");
                break;
        }

        if (buildType == BuildType.Release)
        {
            PlayerSettings.stripEngineCode = true;
        }
    }

    // ------------------------
    // Build options
    // ------------------------

    private static BuildOptions GetBuildOptions(BuildType buildType)
    {
        return buildType switch
        {
            BuildType.Development =>
                BuildOptions.Development |
                BuildOptions.AllowDebugging |
                BuildOptions.ConnectWithProfiler,

            BuildType.Release =>
                BuildOptions.None,

            _ => BuildOptions.None
        };
    }

    // ------------------------
    // Defines (Unity 6+ correct API)
    // ------------------------

    private static void ApplyDefines(BuildPlatform platform, BuildType buildType)
    {
        NamedBuildTarget target = GetNamedBuildTarget(platform);

        string existing = PlayerSettings.GetScriptingDefineSymbols(target);

        // clean old build defines
        existing = existing
            .Replace("DEV_BUILD", "")
            .Replace("RELEASE_BUILD", "")
            .Replace(";;", ";")
            .Trim(';');

        string buildDefine = buildType switch
        {
            BuildType.Development => "DEV_BUILD",
            BuildType.Release => "RELEASE_BUILD",
            _ => ""
        };

        string final;

        if (string.IsNullOrWhiteSpace(existing))
        {
            final = buildDefine;
        }
        else if (string.IsNullOrWhiteSpace(buildDefine))
        {
            final = existing;
        }
        else
        {
            final = $"{existing};{buildDefine}";
        }

        final = final.Replace(";;", ";").Trim(';');

        PlayerSettings.SetScriptingDefineSymbols(target, final);
    }

    private static NamedBuildTarget GetNamedBuildTarget(BuildPlatform platform)
    {
        return platform switch
        {
            BuildPlatform.Android => NamedBuildTarget.Android,
            BuildPlatform.iOS => NamedBuildTarget.iOS,
            _ => throw new Exception("Unsupported platform")
        };
    }

    // ------------------------
    // Output path
    // ------------------------

    private static string GetBuildPath(BuildPlatform platform, BuildType buildType)
    {
        switch (platform)
        {
            case BuildPlatform.Android:
                string ext = buildType == BuildType.Release ? "aab" : "apk";
                return $"{buildType}/{platform}/{Application.productName}.{ext}";

            case BuildPlatform.iOS:
                return $"{buildType}/{platform}";

            default:
                throw new Exception("Unsupported platform");
        }
    }

    // ------------------------
    // Unity build target mapping
    // ------------------------

    private static BuildTarget GetBuildTarget(BuildPlatform platform)
    {
        return platform switch
        {
            BuildPlatform.Android => BuildTarget.Android,
            BuildPlatform.iOS => BuildTarget.iOS,
            _ => throw new Exception("Invalid platform")
        };
    }

    // ------------------------
    // Args
    // ------------------------

    private static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && i + 1 < args.Length)
                return args[i + 1];
        }

        throw new Exception($"Missing argument: {name}");
    }

    private static string GetOptionalArg(string name)
    {
        var args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && i + 1 < args.Length)
                return args[i + 1];
        }

        return null;
    }
}

// ------------------------
// Runtime build metadata
// ------------------------