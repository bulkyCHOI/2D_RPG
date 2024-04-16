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

        _loginSceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>();
    }

    public override void Clear()
    {
        
    }

    
}
