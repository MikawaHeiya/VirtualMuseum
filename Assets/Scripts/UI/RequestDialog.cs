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
        animator.SetTrigger("Exit");
    }

    public void OnNegativeButtonClicked()
    {
        NegativeButtonClicked?.Invoke();
        animator.SetTrigger("Exit");
    }

    public void SelfDestroy()
    {
        Destroy(gameObject);
    }

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
}
