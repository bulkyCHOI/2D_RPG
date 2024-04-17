using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectServerPopup_Item : UI_Base
{
    public ServerInfo ServerInfo { get; set; }

    enum Buttons
    {
        SelectServerButton
    }

    enum Texts
    {
        ServerNameText
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        GetButton((int)Buttons.SelectServerButton).gameObject.BindEvent(OnClickButton);
    }

    public void RefreshUI()
    {
        if(ServerInfo == null)
            return;
        GetTMPText((int)Texts.ServerNameText).text = ServerInfo.ServerName;
    }
    void OnClickButton(PointerEventData evt)
    {
        //Managers.Network.ConnectToGame(ServerInfo);이미 접속이 되어 잇다.
        //원래대로 라면 account서버에 접속이 되어 있던것이니, 접속을 끊고 다시 접속해야한다.
        Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.UI.ClosePopupUI();

        C_Login loginPacket = new C_Login();

        loginPacket.UniqueId = Managers.Network.AccountName;   //아이디를 로그인때의 ID로 대체
        Managers.Network.Send(loginPacket);
    }
}

   