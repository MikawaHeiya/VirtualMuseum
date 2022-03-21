using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSetElment : MonoBehaviour
{
    public int buttonIndex;

    public event System.Action ButtonFocused;
    public event System.Action ButtonUnFocused;

    public void OnButtonFocused()
    {
        ButtonFocused?.Invoke();
    }

    public void OnButtonUnFocused()
    {
        ButtonUnFocused?.Invoke();
    }
}
