using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoginScene : BaseScene
{
    UI_LoginScene _loginSceneUI;
    
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        Managers.Web.BaseUrl = "http://localhost:5000/api";

        Screen.SetResolution(1280, 800, false);

        _loginSceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>();
    }

    public override void Clear()
    {
        
    }

    
}
