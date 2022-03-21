using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    public event System.Action ButtonClicked;

    public void OnButtonClicked()
    {
        ButtonClicked?.Invoke();
    }
}
