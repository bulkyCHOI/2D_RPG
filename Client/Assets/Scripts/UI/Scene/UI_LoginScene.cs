using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LoginScene : UI_Scene
{
    UI_LoginPopup _loginPopupUI;
    public override void Init()
    {
        base.Init();
    }
    
    public void OnLoginClick()
    {
        Debug.Log("Login Click");
        Managers.UI.ShowPopupUI<UI_LoginPopup>();
    }
}
