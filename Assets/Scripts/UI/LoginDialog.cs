using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginDialog : MonoBehaviour
{
    public Text mailText;
    public Text verifyCodeText;
    public Text sendVerifyCodeButtonText;

    private bool isSendVerifyCodeCooldown = false;

    public string MailInputContent
    { get { return mailText.text; } }

    public string VerifyCodeInputContent
    { get { return verifyCodeText.text; } }

    public event System.Action SendVerifyCodeButtonClicked;
    public event System.Action LoginButtonClicked;
    public event System.Action CancelButtonCLicked;

    public void StartSendVerifyCodeButtonCooldown(int cooldown)
    {
        StartCoroutine(SendVerifyCodeButtonCooldown(cooldown));
    }

    private IEnumerator SendVerifyCodeButtonCooldown(int cooldown)
    {
        isSendVerifyCodeCooldown = true;

        while (cooldown-- > 0)
        {
            sendVerifyCodeButtonText.text = $"{cooldown}s";
            yield return new WaitForSeconds(1f);
        }

        isSendVerifyCodeCooldown = false;
        sendVerifyCodeButtonText.text = "·¢ËÍ";
        yield break;
    }

    public void OnSendVerifyCodeButtonClicked()
    {
        if (!isSendVerifyCodeCooldown)
        {
            SendVerifyCodeButtonClicked?.Invoke();
        }
    }

    public void OnLoginButtonClicked()
    {
        LoginButtonClicked?.Invoke();
    }

    public void OnCancelButtonClicked()
    {
        CancelButtonCLicked?.Invoke();
        uiAnimation.PlayExitAnimation(SelfDestroy);
    }

    public void Exit()
    {
        uiAnimation.PlayExitAnimation(SelfDestroy);
    }

    public void SelfDestroy()
    {
        Destroy(gameObject);
    }

    private UIAnimation uiAnimation;

    private void Start()
    {
        uiAnimation = GetComponent<UIAnimation>();
    }
}
