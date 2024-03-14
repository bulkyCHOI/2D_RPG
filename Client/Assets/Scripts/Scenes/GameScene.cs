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

        Managers.Map.LoadMap(1);

        Screen.SetResolution(1280, 800, false);

        _gameSceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {
        
    }
}
