using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainFeatureButton : MonoBehaviour
{
    public MainUIController mainUIController;
    
    public UnityEngine.UI.Image buttonBackground;

    private void Awake()
    {
        var button = GetComponent<ButtonSetElment>();
        button.ButtonFocused += OnButtonFocused;
        button.ButtonUnFocused += OnButtonUnFocused;

        var click = GetComponent<ButtonClick>();
        click.ButtonClicked += OnButtonClicked;
    }

    public void OnButtonFocused()
    {
        buttonBackground.enabled = true;
    }

    public void OnButtonUnFocused()
    {
        buttonBackground.enabled = false;
    }

    public virtual void OnButtonClicked() 
    {
        mainUIController.HighlightButton = GetComponent<ButtonSetElment>().buttonIndex;
    }
}
