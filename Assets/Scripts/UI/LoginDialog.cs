using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginDialog : MonoBehaviour
{
    public Text mailText;
    public Text verifyCodeText;

    public string MailInputContent
    { get { return mailText.text; } }

    public string VerifyCodeInputContent
    { get { return verifyCodeText.text; } }

    public event System.Action SendVerifyCodeButtonClicked;
    public event System.Action LoginButtonClicked;
    public event System.Action CancelButtonCLicked;

    public void OnSendVerifyCodeButtonClicked()
    {
        SendVerifyCodeButtonClicked?.Invoke();
    }

    public void OnLoginButtonClicked()
    {
        LoginButtonClicked?.Invoke();
        animator.SetTrigger("Exit");
    }

    public void OnCancelButtonClicked()
    {
        CancelButtonCLicked?.Invoke();
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
