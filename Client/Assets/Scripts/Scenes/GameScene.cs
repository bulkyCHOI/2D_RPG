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

        Managers.Map.LoadMap(2);    //초기 마을맵이 2번이다.

        _gameSceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {
        
    }
    
    public void MoveScene() //Scene이동시켜주는 함수
    {

        //Managers.Scene.LoadScene(Define.Scene.Game);
    }
}
