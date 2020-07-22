using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class will be responsible for loading scenes including with scene fades, additive scene loading, and scene unloading
/// </summary>
public class SceneController : MonoBehaviour
{
    public Animator screenFadeAnimator;
    // Start is called before the first frame update
    void Start()
    {
        
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
        SceneManager.LoadSceneAsync("Playground");
    }
}