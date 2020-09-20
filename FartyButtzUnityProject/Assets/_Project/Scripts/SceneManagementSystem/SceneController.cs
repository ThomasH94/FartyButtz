using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class will be responsible for loading scenes including with scene fades, additive scene loading, and scene unloading
/// </summary>
public class SceneController : MonoBehaviour
{
    public Animator screenFadeAnimator;
    
    private void Awake()
    {
        // Create singleton reference with Don't Destroy On Load
    }

    [ContextMenu("Load Scene Routine")]
    public void LoadSceneWithFadeWrapper()
    {
        screenFadeAnimator.SetTrigger("FadeOut");
        StartCoroutine(LoadSceneWithFadeRoutine());
    }
    
    private IEnumerator LoadSceneWithFadeRoutine()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("Sandbox");
    }

    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // TODO: Bring up a confirmation, then make this a coroutine that fades out
    public void QuitGame()
    {
        Application.Quit();
    }
}