using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Level : UI_Base
{
    [SerializeField]
    public TMP_Text _level;
    public TMP_Text _exp;
    public Transform _expBar;
    public int _totalExp;
    public int _currentExp;

    public override void Init()
    {
        //RefreshUI();
    }

    public void RefreshUI()
    {
        //level = Managers.Object.MyPlayer.Stat.Level;
        //StatInfo statInfo = null;
        //Managers.Data.StatDict.TryGetValue(level, out statInfo);
        _totalExp = Managers.Object.MyPlayer.Stat.TotalExp;
        _currentExp = Managers.Object.MyPlayer.Stat.CurrentExp;
        _level.text = $"{Managers.Object.MyPlayer.Stat.Level.ToString()}";
        _exp.text = $"{_currentExp}/{_totalExp}";

        SetExpBar((float)_currentExp / _totalExp);
    }

    public void SetExpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _expBar.localScale = new Vector3(ratio, 1, 1);
    }
}
