using Sirenix.Serialization;
using TMPro;
using UnityEngine;

public class DebugInfoMenuData : IMenuData
{
}

public class DebugInfoMenu : BaseMenu
{
    [OdinSerialize] private TextMeshProUGUI m_GameVersionTMP;
    [OdinSerialize] private TextMeshProUGUI m_GameInfoTMP;

    private void Awake()
    {
        if (!BuildUtils.IsDevBuild)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void Start()
    {
        InitializeDebugInfo();
    }

    private void InitializeDebugInfo()
    {
        string version = Application.version;

        string build =
            string.IsNullOrEmpty(BuildMetadata.BuildNumber)
                ? "editor"
                : BuildMetadata.BuildNumber;

        m_GameVersionTMP.text = $"v{version} (build {build})";

        m_GameInfoTMP.text =
            $"{Application.platform}\nScene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}";
    }
}