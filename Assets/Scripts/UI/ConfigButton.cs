using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigButton : MainFeatureButton
{
    public override void OnButtonClicked()
    {
        base.OnButtonClicked();
        FindObjectOfType<SceneLoader>().LoadScene(4);
    }
}
