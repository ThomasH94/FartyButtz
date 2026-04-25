public static class BuildMetadata
{
    public static string BuildNumber { get; private set; }

    public static void SetBuildNumber(string value)
    {
        BuildNumber = value;
    }
}