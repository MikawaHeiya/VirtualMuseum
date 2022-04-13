using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequestDialog : MonoBehaviour
{
    public Text content;
    public string Content
    { 
        get { return content.text; } 
        set { content.text = value; }
    }

    public event System.Action PositiveButtonClicked;
    public event System.Action NegativeButtonClicked;

    public void OnPositiveButtonClicked()
    {
        PositiveButtonClicked?.Invoke();
        uIAnimation.PlayExitAnimation(SelfDestroy);
    }

    public void OnNegativeButtonClicked()
    {
        NegativeButtonClicked?.Invoke();
        uIAnimation.PlayExitAnimation(SelfDestroy);
    }

    public void SelfDestroy()
    {
        Destroy(gameObject);
    }

    private UIAnimation uIAnimation;

    private void Start()
    {
        uIAnimation = GetComponent<UIAnimation>();
    }
}
