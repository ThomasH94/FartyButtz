public struct SceneUnloadedPayload
{
	public string SceneName { get; }

	public SceneUnloadedPayload(string sceneName)
	{
		SceneName = sceneName;
	}
}
