using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _gameSceneUI;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        //TODO: 여기에 계정생성 코드 잠시 추가
        Managers.Web.BaseUrl = "http://localhost:5000/api";
        WebPacket.SendCreateAccount("ppaccomy", "1234");



        Managers.Map.LoadMap(1);

        Screen.SetResolution(1280, 1024, false);

        _gameSceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();

        
    }

    public override void Clear()
    {
        
    }
}
