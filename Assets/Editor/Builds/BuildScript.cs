using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private static string[] GetScenes()
    {
        return EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
    }

    public static void BuildAndroidDev() =>
        Build(BuildPlatform.Android, BuildType.Development);

    public static void BuildAndroidRelease() =>
        Build(BuildPlatform.Android, BuildType.Release);

    public static void BuildiOSDev() =>
        Build(BuildPlatform.iOS, BuildType.Development);

    public static void BuildiOSRelease() =>
        Build(BuildPlatform.iOS, BuildType.Release);

    public static void BuildFromCommandLine()
    {
        var platformArg = GetArg("-platform");
        var buildTypeArg = GetArg("-buildType");

        if (!Enum.TryParse(platformArg, true, out BuildPlatform platform))
            throw new Exception($"Invalid platform: {platformArg}");

        if (!Enum.TryParse(buildTypeArg, true, out BuildType buildType))
            throw new Exception($"Invalid build type: {buildTypeArg}");

        Build(platform, buildType);
    }

    private static void Build(BuildPlatform platform, BuildType buildType)
    {
        var config = BuildConfig.Create(platform, buildType);

        Debug.Log($"Starting build: {platform} | {buildType}");

        SwitchPlatform(platform);
        ApplyPlatformSettings(platform, buildType);

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = GetBuildPath(config),
            target = GetBuildTarget(platform),
            options = config.Options
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

    private static void ApplyPlatformSettings(BuildPlatform platform, BuildType buildType)
    {
        switch (platform)
        {
            case BuildPlatform.Android:
                EditorUserBuildSettings.buildAppBundle = true; // AAB for Play Store
                PlayerSettings.Android.useCustomKeystore = false; // change later for release
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

    private static string GetBuildPath(BuildConfig config)
    {
        switch (config.Platform)
        {
            case BuildPlatform.Android:
                return $"{config.OutputPath}/{config.BuildName}.aab";

            case BuildPlatform.iOS:
                return $"{config.OutputPath}";
        }

        throw new Exception("Unsupported platform");
    }

    private static BuildTarget GetBuildTarget(BuildPlatform platform)
    {
        return platform switch
        {
            BuildPlatform.Android => BuildTarget.Android,
            BuildPlatform.iOS => BuildTarget.iOS,
            _ => throw new Exception("Invalid platform")
        };
    }

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
}