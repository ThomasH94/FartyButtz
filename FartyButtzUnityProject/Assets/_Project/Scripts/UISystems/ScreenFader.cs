using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public Animator screenFadeAnimator;

    public void FadeIn()
    {
        screenFadeAnimator.SetTrigger("FadeIn");
    }

    public void FadeOut()
    {
        screenFadeAnimator.SetTrigger("FadeOut");
    }
}
