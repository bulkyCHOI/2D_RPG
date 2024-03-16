using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    [SerializeField]
    public GameObject loginPopup;
    public GameObject signupPopup;
    public GameObject checkingPopup;
    public TMP_InputField login_userName;
    public TMP_InputField login_password;
    public TMP_InputField signup_userName;
    public TMP_InputField signup_password;

    public override void Init()
    {
        base.Init();
    }

    public void OnLoginClick()
    {
        Debug.Log("Login Click");
        loginPopup.SetActive(true);
        signupPopup.SetActive(false);
        checkingPopup.SetActive(false);
    }

    public void OnSignupClick()
    {
        Debug.Log("Signup Click");
        loginPopup.SetActive(false);
        signupPopup.SetActive(true);
        checkingPopup.SetActive(false);
    }

    public void OnCheckingClick()
    {
        Debug.Log("Signup Click");
        loginPopup.SetActive(false);
        signupPopup.SetActive(false);
        checkingPopup.SetActive(true);
    }

    public void OnCloseClick()
    {
        Debug.Log("Close Click");
        loginPopup.SetActive(false);
        signupPopup.SetActive(false);
        checkingPopup.SetActive(false);
    }

    public void OnLoginBtnClick()
    {
        Debug.Log("Login Btn Click");
        string userName = login_userName.text;
        string password = login_password.text;

        LoginAccountPakcetReq packet = new LoginAccountPakcetReq()
        {
            AccountName = userName,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginAccountPakcetRes>("account/login", packet, (res) =>
        {
            Debug.Log(res.LoginOk);
            login_userName.text = "";
            login_password.text = "";

            if (res.LoginOk)
            {
                //Managers.Network.ConnectToGame();
                //Managers.Scene.LoadScene(Define.Scene.Game);
                loginPopup.SetActive(false);

                Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;

                UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                popup.SetServers(res.ServerList);
            }
        });
    }

    public void OnSignupBtnClick()
    {
        Debug.Log("Signup Btn Click");
        string userName = signup_userName.text;
        string password = signup_password.text;

        CreateAccountPakcetReq packet = new CreateAccountPakcetReq()
        {
            AccountName = userName,
            Password = password
        };

        Managers.Web.SendPostRequest<CreateAccountPakcetRes>("account/create", packet, (res) =>
        {
            Debug.Log(res.CreateOk);

            signup_userName.text = "";
            signup_password.text = "";

            if (res.CreateOk)
            {
                loginPopup.SetActive(false);
                signupPopup.SetActive(false);
                checkingPopup.SetActive(false);
            }
        });
    }
}
