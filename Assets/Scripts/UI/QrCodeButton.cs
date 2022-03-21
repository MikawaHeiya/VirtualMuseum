using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QrCodeButton : MainFeatureButton
{
    public override void OnButtonClicked()
    {
        base.OnButtonClicked();
        FindObjectOfType<SceneLoader>().LoadScene(1);
    }
}
