using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserButton : MonoBehaviour
{
    public MainUIController mainUIController;
    public MainSceneApplication mainSceneApplication;

    public GameObject loginedContent;
    public GameObject loginedHighlight;
    public GameObject logoutedContent;
    public GameObject logoutedHighlight;

    private bool UserLogined { get { return mainSceneApplication.User != null; } }

    private void Awake()
    {
        var button = GetComponent<ButtonSetElment>();
        button.ButtonFocused += OnButtonFocused;
        button.ButtonUnFocused += OnButtonUnFocused;
    }

    private void Start()
    {
        var click = GetComponent<ButtonClick>();
        click.ButtonClicked += OnButtonClicked;

        mainSceneApplication.UserLogined += (UserInfo user) =>
        {
            logoutedContent.SetActive(false);
            logoutedHighlight.SetActive(false);
            loginedContent.SetActive(true);
            loginedHighlight.SetActive(true);
            loginedContent.GetComponent<UnityEngine.UI.Text>().text = user.mail;
        };

        mainSceneApplication.UserLogouted += () =>
        {
            loginedContent.SetActive(false);
            loginedHighlight.SetActive(false);
            logoutedContent.SetActive(true);
            logoutedHighlight.SetActive(true);
        };
    }

    public void OnButtonClicked()
    {
        mainUIController.HighlightButton = GetComponent<ButtonSetElment>().buttonIndex;

        if (UserLogined)
        {
            var dialog = Instantiate(
                mainSceneApplication.requestDialogPrefab, 
                mainSceneApplication.canvas.transform).GetComponent<RequestDialog>();
            dialog.Content = $"是否要退出{mainSceneApplication.User.mail}的登录？";
            dialog.PositiveButtonClicked += mainSceneApplication.Logout;
        }
        else
        {
            mainSceneApplication.InstantiateLoginDialog();
        }
    }

    private void OnButtonFocused()
    {
        if (UserLogined)
        {
            loginedHighlight.SetActive(true);
        }
        else
        {
            logoutedHighlight.SetActive(true);
        }
    }

    private void OnButtonUnFocused()
    {
        if (UserLogined)
        {
            loginedHighlight.SetActive(false);
        }
        else
        {
            logoutedHighlight.SetActive(false);
        }
    }
}
