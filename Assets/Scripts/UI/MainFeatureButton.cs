using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainFeatureButton : MonoBehaviour
{
    public MainUIController mainUIController;

    public UnityEngine.UI.Image buttonBackground;
    public UnityEngine.UI.Image buttonIcon;

    public UnityEngine.UI.Text buttonText;
    public string displayText;

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
        buttonBackground.color = new Color(135 / 255f, 206 / 255f, 235 / 255f, 255 / 255f);
        buttonIcon.color = new Color(1f, 1f, 1f, 1f);

        buttonText.text = displayText;
    }

    public void OnButtonUnFocused()
    {
        buttonBackground.color = new Color(1f, 1f, 1f, 1f);
        buttonIcon.color = new Color(19 / 255f, 19 / 255f, 19 / 255f, 1f);

        buttonText.text = "";
    }

    public virtual void OnButtonClicked() 
    {
        mainUIController.HighlightButton = GetComponent<ButtonSetElment>().buttonIndex;
        Debug.Log(displayText + " clicked");
    }
}
