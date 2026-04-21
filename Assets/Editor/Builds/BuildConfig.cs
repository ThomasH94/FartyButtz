using UnityEditor;
using UnityEngine;

public enum BuildType
{
    Development,
    Release
}

public enum BuildPlatform
{
    Android,
    iOS
}

public class BuildConfig
{
    public BuildType BuildType;
    public BuildPlatform Platform;

    public string OutputPath;
    public string BuildName;

    public BuildOptions Options;

    public static BuildConfig Create(BuildPlatform platform, BuildType buildType)
    {
        var config = new BuildConfig
        {
            Platform = platform,
            BuildType = buildType
        };

        config.BuildName = $"{Application.productName}_{platform}_{buildType}";
        config.OutputPath = $"Builds/{platform}/{buildType}/";

        config.Options = BuildOptions.None;

        if (buildType == BuildType.Development)
        {
            config.Options |= BuildOptions.Development;
            config.Options |= BuildOptions.AllowDebugging;
            config.Options |= BuildOptions.ConnectWithProfiler;
        }

        return config;
    }
}