using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneApplication : MonoBehaviour
{
    public GameObject loginDialogPrefab;
    public GameObject requestDialogPrefab;
    public GameObject infoDialog;
    public GameObject canvas;
    public GameObject loading;
    public UserButton userButton;

    public UserInfo User { get; private set; }

    public event System.Action<UserInfo> UserLogined;
    public event System.Action UserLogouted;

    private void Start()
    {
        FindObjectOfType<SceneLoader>().loading = loading;

        loading.SetActive(false);
    }

    public void SendVerifyCodeRequest(string mail)
    {
        Debug.Log("MainSceneApplication.SendVerifyCodeRequest(string): no implementation");
        Debug.Log($"MainSceneApplication.SendVerifyCodeRequest(string): mail: {mail}");
    }

    public void LoginWithVerifyCode(string mail, string verifyCode)
    {
        Debug.Log("MailSceneApplication.LoginWithVerifyCode(string, string): no implementation");
        Debug.Log($"MailSceneApplication.LoginWithVerifyCode(string, string): mail: {mail}, verifyCode: {verifyCode}");

        User = new UserInfo { mail = mail, passport = null };
        UserLogined?.Invoke(User);
    }

    public void LoginWithPassport(string mail, string passport)
    {
        Debug.Log("MailSceneApplication.LoginWithPassport(string, string): no implementation");
        Debug.Log($"MailSceneApplication.LoginWithPassport(string, string): mail: {mail}, passport: {passport}");

        User = new UserInfo { mail = mail, passport = passport };
        UserLogined.Invoke(User);
    }

    public void Logout()
    {
        User = null;
        UserLogouted?.Invoke();
    }

    public void InstantiateLoginDialog()
    {
        var dialog = Instantiate(loginDialogPrefab, canvas.transform).GetComponent<LoginDialog>();
        dialog.SendVerifyCodeButtonClicked += () => { SendVerifyCodeRequest(dialog.MailInputContent); };
        dialog.LoginButtonClicked += 
            () => { LoginWithVerifyCode(dialog.MailInputContent, dialog.VerifyCodeInputContent); };
    }

    public void InstantiateInfoDialog()
    {
        Instantiate(infoDialog, canvas.transform);
    }
}
