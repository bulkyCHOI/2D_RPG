using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    [SerializeField]
    public GameObject loginPopup;
    public GameObject signupPopup;
    public GameObject checkingPopup;
    public GameObject loadingPopup;
    public GameObject errorMsg1Popup;
    public GameObject errorMsg2Popup;
    public GameObject alramMsg1Popup;
    public GameObject alramMsg2Popup;

    public TMP_InputField login_userName;
    public TMP_InputField login_password;
    public TMP_InputField signup_userName;
    public TMP_InputField signup_password;

    public override void Init()
    {
        base.Init();
        Managers.Network.ConnectToServer(NetworkManager.GameServerAddress, 80);
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

        C_LoginAccount loginAccountPkt = new C_LoginAccount();
        loginAccountPkt.AccountId = userName;
        loginAccountPkt.Password = password;
        Managers.Network.AccountName = userName; //나중에 사용을 위해 저장(캐릭터 닉네임)
        Managers.Network.Send(loginAccountPkt);

        //로딩중 표시 필요
    }

    public void OnSignupBtnClick()
    {
        Debug.Log("Signup Btn Click");
        string userName = signup_userName.text;
        string password = signup_password.text;

        C_CreateAccount createAccountPkt = new C_CreateAccount();
        createAccountPkt.AccountId = userName;
        createAccountPkt.Password = password;
        Managers.Network.Send(createAccountPkt);

        //로딩중 표시 필요
    }

    //게임오브젝트를 n초 후에 비활성화
    public void SetActiveFalse(GameObject obj, float time)
    {
        StartCoroutine(ActiveFalse(obj, time));
    }

    public void SetActiveFalse(GameObject obj, string text, float time)
    {
        obj.SetActive(true);
        obj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        StartCoroutine(ActiveFalse(obj, time));
    }


    IEnumerator ActiveFalse(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }

    //public void OnLoginBtnClick()
    //{
    //    Debug.Log("Login Btn Click");
    //    string userName = login_userName.text;
    //    string password = login_password.text;

    //    LoginAccountPakcetReq packet = new LoginAccountPakcetReq()
    //    {
    //        AccountName = userName,
    //        Password = password
    //    };

    //    Managers.Web.SendPostRequest<LoginAccountPakcetRes>("account/login", packet, (res) =>
    //    {
    //        //Debug.Log(res.LoginOk);
    //        login_userName.text = "";
    //        login_password.text = "";

    //        if (res.LoginOk)
    //        {
    //            //Managers.Network.ConnectToGame();
    //            //Managers.Scene.LoadScene(Define.Scene.Game);
    //            loginPopup.SetActive(false);

    //            Managers.Network.AccountId = res.AccountId;
    //            Managers.Network.AccountName = userName;
    //            Managers.Network.Token = res.Token;

    //            UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
    //            popup.SetServers(res.ServerList);
    //        }
    //    });
    //}

    //public void OnSignupBtnClick()
    //{
    //    //Debug.Log("Signup Btn Click");
    //    string userName = signup_userName.text;
    //    string password = signup_password.text;

    //    CreateAccountPakcetReq packet = new CreateAccountPakcetReq()
    //    {
    //        AccountName = userName,
    //        Password = password
    //    };

    //    Managers.Web.SendPostRequest<CreateAccountPakcetRes>("account/create", packet, (res) =>
    //    {
    //        //Debug.Log(res.CreateOk);

    //        signup_userName.text = "";
    //        signup_password.text = "";

    //        if (res.CreateOk)
    //        {
    //            loginPopup.SetActive(false);
    //            signupPopup.SetActive(false);
    //            checkingPopup.SetActive(false);
    //        }
    //    });
    //}
}
