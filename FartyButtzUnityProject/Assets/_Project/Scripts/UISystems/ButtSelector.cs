using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtSelector : MonoBehaviour
{
    [SerializeField] private List<ButtData> allButtz = new List<ButtData>();
    // THIS IS ALL TEMPORARY AND SHOULD BE SET WITH EVENTS
    public SpriteRenderer selectedButtSprite;
    public GameObject buttSelectionCanvas;

    public void SetButt(ButtData newButt)
    {
        selectedButtSprite.sprite = newButt.buttSprite;
        CloseButtSelector();
    }


    private void PopulateButtList()
    {
        for (int i = 0; i < allButtz.Count; i++)
        {
            if (allButtz[i].isUnlocked)
            {
                Debug.Log("Added " + allButtz[i].name + " to the butt list");
            }
        }
    }

    // TEMP! TODO: Make this happen when a butt is selected in a cleaner way
    private void CloseButtSelector()
    {
        buttSelectionCanvas.SetActive(false);
    }
}