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
    public TMP_Text _hp;
    public Transform _hpBar;
    public TMP_Text _mp;
    public Transform _mpBar;

    public override void Init()
    {
        //RefreshUI();
    }

    public void RefreshUI()
    {
        //level = Managers.Object.MyPlayer.Stat.Level;
        //StatInfo statInfo = null;
        //Managers.Data.StatDict.TryGetValue(level, out statInfo);
        _level.text = $"{Managers.Object.MyPlayer.Stat.Level.ToString()}";
        _exp.text = $"{Managers.Object.MyPlayer.Stat.CurrentExp}/{Managers.Object.MyPlayer.Stat.TotalExp}";

        _hp.text = $"{Managers.Object.MyPlayer.Stat.Hp}/{Managers.Object.MyPlayer.Stat.MaxHp}";
        _mp.text = $"{Managers.Object.MyPlayer.Stat.Mp}/{Managers.Object.MyPlayer.Stat.MaxMp}";

        SetExpBar((float)Managers.Object.MyPlayer.Stat.CurrentExp/Managers.Object.MyPlayer.Stat.TotalExp);
        SetHpBar((float)Managers.Object.MyPlayer.Stat.Hp / Managers.Object.MyPlayer.Stat.MaxHp);
        SetMpBar((float)Managers.Object.MyPlayer.Stat.Mp / Managers.Object.MyPlayer.Stat.MaxMp);
    }

    public void SetExpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _expBar.localScale = new Vector3(ratio, 1, 1);
    }

    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _hpBar.localScale = new Vector3(ratio, 1, 1);
    }

    public void SetMpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _mpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
