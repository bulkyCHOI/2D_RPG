using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MpBar : MonoBehaviour
{
    [SerializeField]
    Transform _MpBar = null;

    public void SetMpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _MpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
