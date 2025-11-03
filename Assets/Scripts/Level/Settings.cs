using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public bool fullscreenTog, vsyncTog;
    public Image fullscreenButton, vsyncButton;

    private void Start()
    {
        fullscreenTog = Screen.fullScreen;
        if (fullscreenTog == false)
        {
            fullscreenButton.color = Color.gray;
        } else
        {
            fullscreenButton.color = Color.white;
        }

        if (QualitySettings.vSyncCount == 0)
        {
            vsyncTog = false;
            vsyncButton.color = Color.gray;
        }
        else
        {
            vsyncTog = true;
            vsyncButton.color = Color.white;
        }
    }
    public void ToggleFullscreen()
    {
        if (fullscreenTog == true)
        {
            fullscreenTog = false;
            fullscreenButton.color = Color.gray;
        } else
        {
            fullscreenTog = true;
            fullscreenButton.color = Color.white;
        }
    }

    public void ToggleVSync()
    {
        if (vsyncTog == true)
        {
            vsyncTog = false;
            vsyncButton.color = Color.gray;
        }
        else
        {
            vsyncTog = true;
            vsyncButton.color = Color.white;
        }
    }
    public void ApplyGraphics()
    {
        Screen.fullScreen = fullscreenTog;

        if (vsyncTog == true) { 
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }
    }
}
