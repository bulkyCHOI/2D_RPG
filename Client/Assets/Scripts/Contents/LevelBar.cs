using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBar : MonoBehaviour
{
    [SerializeField]
    Transform _ExpBar = null;
    [SerializeField]
    Transform _HpBar = null;
    [SerializeField]
    Transform _MpBar = null;

    public void SetExpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _ExpBar.localScale = new Vector3(ratio, 1, 1);
    }

    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _HpBar.localScale = new Vector3(ratio, 1, 1);
    }

    public void SetMpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _MpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
