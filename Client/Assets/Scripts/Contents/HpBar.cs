using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField]
    Transform _HpBar = null;

    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _HpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
