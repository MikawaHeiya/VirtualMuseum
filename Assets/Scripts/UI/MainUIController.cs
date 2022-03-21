using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIController : MonoBehaviour
{
    public List<ButtonSetElment> buttons;
    public int defaultHighlightButton;

    public InputUtil inputUtil;

    private int currentHighlightButton = 0;
    public int HighlightButton
    {
        get { return currentHighlightButton; }
        set
        {
            buttons[currentHighlightButton].OnButtonUnFocused();
            currentHighlightButton = value >= buttons.Count ? 0 : value < 0 ? 0 : value;
            buttons[currentHighlightButton].OnButtonFocused();
        }
    }

    private void Start()
    {
        currentHighlightButton = defaultHighlightButton;
        HighlightButton = defaultHighlightButton;
    }

    private void Update()
    {
        if (inputUtil.NextInputEntered)
        {
            ++HighlightButton;
        }
        else if (inputUtil.ClickInputEntened)
        {
            buttons[HighlightButton].GetComponent<ButtonClick>().OnButtonClicked();
        }
    }
}
