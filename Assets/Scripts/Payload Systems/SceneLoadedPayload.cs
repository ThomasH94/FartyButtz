public struct SceneLoadedPayload
{
	public string SceneName { get; }

	public SceneLoadedPayload(string sceneName)
	{
		SceneName = sceneName;
	}
}
