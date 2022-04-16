using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideSceneElement : MonoBehaviour
{
    public int index = 0;
    private GuideSceneApplication sceneApplication;

    private void Start()
    {
        sceneApplication = FindObjectOfType<GuideSceneApplication>();
    }

    public void NextWidget()
    {
        sceneApplication.WidgetIndex = index + 1;
    }

    public void CancelButtonClicked()
    {
        sceneApplication.CancelButtonClicked();
    }
}
