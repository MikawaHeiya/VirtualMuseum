using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using UnityEngine;

public struct LoginResult
{
    public bool success;
    public string passport;
}

public class LoginButton : MainFeatureButton
{
    public GameObject loginDialogPrefab;
    public GameObject requestDialogPrefab;
    public GameObject canvas;
    public GameObject icon;
    public GameObject spinner;
    public UnityEngine.UI.Text title;
    public UnityEngine.UI.Text detail;

    private bool isLogging;
    private bool IsLogging
    {
        get { return isLogging; }
        set 
        { 
            isLogging = value;
            icon.SetActive(!isLogging);
            spinner.SetActive(isLogging);
        }
    }
    private HttpClient httpClient = new HttpClient();

    private void Start()
    {
        if (!string.IsNullOrEmpty(ConfigController.Config.Email) &&
            !string.IsNullOrEmpty(ConfigController.Config.Passport))
        {
            title.text = "登出";
            detail.text = ConfigController.Config.Email;
        }
    }

    public override void OnButtonClicked()
    {
        base.OnButtonClicked();
        if (!string.IsNullOrEmpty(ConfigController.Config.Email) &&
            !string.IsNullOrEmpty(ConfigController.Config.Passport))
        {
            Logout();
        }
        else
        {
            Login();
        }
    }

    private bool IsLegalEmail(string email)
    {
        return Regex.IsMatch(email, @"^[\w-]+@[\w-]+\.(com|net|org|edu|mil|tv|biz|info)$");
    }

    private void DialogForLoginFailed(string message)
    {
        var dialog = Instantiate(requestDialogPrefab, canvas.transform);
        dialog.GetComponent<RequestDialog>().Content = message;
    }

    private async void Logout()
    {
        IsLogging = true;

        var config = ConfigController.Config;
        config.Email = string.Empty;
        config.Passport = string.Empty;
        ConfigController.Config = config;
        await ConfigController.WriteConfig();

        title.text = "登录";
        detail.text = "登录以使用联网功能";

        IsLogging = false;
    }

    private void Login()
    {
        if (IsLogging)
        {
            return;
        }
        IsLogging = true;

        var dialog = Instantiate(loginDialogPrefab, canvas.transform).GetComponent<LoginDialog>();
        dialog.SendVerifyCodeButtonClicked += async () =>
        {
            var email = dialog.MailInputContent;

            if (!IsLegalEmail(email))
            {
                DialogForLoginFailed("非法的邮箱格式。");
                return;
            }

            dialog.StartSendVerifyCodeButtonCooldown(60);

            var response = await httpClient.GetAsync(
                $"http://101.42.253.148:8080/VirtualMuseum/SendVerifyCode?mail={email}");
            var result = await response.Content.ReadAsStringAsync();

            if (result == "false")
            {
                DialogForLoginFailed("发送验证码时失败。");
            }
        };

        dialog.LoginButtonClicked += async () =>
        {
            var email = dialog.MailInputContent;
            var verifyCode = dialog.VerifyCodeInputContent;
            if (!IsLegalEmail(email))
            {
                DialogForLoginFailed("非法的邮箱格式。");
                return;
            }
            else if (string.IsNullOrEmpty(verifyCode))
            {
                DialogForLoginFailed("验证码不能为空。");
                return;
            }
            
            var response = await httpClient.GetAsync(
                $"http://101.42.253.148:8080/VirtualMuseum/LoginWithVerifyCode?mail={email}&verifyCode={verifyCode}");
            var resultJSON = await response.Content.ReadAsStringAsync();

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResult>(resultJSON);
            if (result.success)
            {
                var config = ConfigController.Config;
                config.Email = email;
                config.Passport = result.passport;
                ConfigController.Config = config;
                await ConfigController.WriteConfig();

                title.text = "登出";
                detail.text = email;

                IsLogging = false;
                dialog.Exit();
            }
            else
            {
                DialogForLoginFailed("验证失败。");
            }
        };

        dialog.CancelButtonCLicked += () =>
        {
            IsLogging = false;
        };
    }
}
