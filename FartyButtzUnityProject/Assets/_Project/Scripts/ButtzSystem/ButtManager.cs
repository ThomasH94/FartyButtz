using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The purpose of this class is to set our Butt based on the users selection and assign that butt to the active butt
/// when entering any play mode
/// </summary>
public class ButtManager : MonoBehaviour
{
    public ButtData ActiveButt { get; private set; }    // Displays the active Butt Data and should only be set within this class
    public ButtData[] AllButtz;
    private int buttIndexer = 0;

    // This method will cycle through all of the buttz and display the active butt
    public void SelectButt(int buttIndex)
    {
        buttIndexer += buttIndex;
        // Do some UI work here to display the Butt..
    }

    // This method will set the current ButtData to the selected ButtData if it's unlocked
    public void SetButt(int currentButtIndex)
    {
        if (AllButtz[currentButtIndex].isUnlocked)
        {
            ActiveButt = AllButtz[currentButtIndex];
        }
    }
}
