using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationButton : MainFeatureButton
{
    public override void OnButtonClicked()
    {
        base.OnButtonClicked();
        FindObjectOfType<MainSceneApplication>().InstantiateInfoDialog();
    }
}
