using UnityEngine;

public static class BuildUtils
{
    public static bool IsEditor =>
        Application.isEditor;

    public static bool IsDevelopmentBuild =>
        Debug.isDebugBuild;

    public static bool IsReleaseBuild =>
#if RELEASE_BUILD
        true;
#else
        !Application.isEditor && !Debug.isDebugBuild;
#endif

    public static bool IsDevBuild =>
#if DEV_BUILD || UNITY_EDITOR
        true;
#else
        Debug.isDebugBuild;
#endif

    public static string BuildLabel =>
#if UNITY_EDITOR
        "EDITOR";
#elif DEV_BUILD
        "DEV";
#else
        "RELEASE";
#endif
}