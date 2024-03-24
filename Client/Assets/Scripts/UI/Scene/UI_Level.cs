using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Level : UI_Base
{
    [SerializeField]
    public TMP_Text _level;
    public TMP_Text _exp;
    public GameObject _expBar;
    public int _totalExp;
    public int _currentExp;

    public override void Init()
    {
        //RefreshUI();
    }

    public void RefreshUI()
    {
        _totalExp = Managers.Data.StatDict;
        _currentExp = Managers.Object.MyPlayer.Stat.TotalExp;
        _level.text = $"{Managers.Object.MyPlayer.Stat.Level.ToString()}";
        _exp.text = $"{Managers.Object.MyPlayer.Stat.TotalExp}";

    }
}
