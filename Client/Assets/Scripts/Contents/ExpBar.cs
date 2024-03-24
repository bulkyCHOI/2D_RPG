using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpBar : MonoBehaviour
{
    [SerializeField]
    Transform _ExpBar = null;

    public void SetExpBar(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        _ExpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
