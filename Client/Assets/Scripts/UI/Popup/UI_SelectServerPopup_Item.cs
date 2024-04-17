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
        //Managers.Network.ConnectToGame(ServerInfo);�̹� ������ �Ǿ� �մ�.
        //������� ��� account������ ������ �Ǿ� �ִ����̴�, ������ ���� �ٽ� �����ؾ��Ѵ�.
        Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.UI.ClosePopupUI();

        C_Login loginPacket = new C_Login();

        loginPacket.UniqueId = Managers.Network.AccountName;   //���̵� �α��ζ��� ID�� ��ü
        Managers.Network.Send(loginPacket);
    }
}

   