using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Popup_Message : UI_Base
{
    [SerializeField]
    public GameObject errorMsg1Popup;
    public GameObject errorMsg2Popup;
    public GameObject alramMsg1Popup;
    public GameObject alramMsg2Popup;


    public override void Init()
    {
    }

    //게임오브젝트를 n초 후에 비활성화
    public void SetActiveFalse(GameObject obj, string text, float time)
    {
        obj.SetActive(true);
        obj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        StartCoroutine(ActiveFalse(obj, time));
    }

    public void SetActiveFalse(GameObject obj, float time)
    {
        StartCoroutine(ActiveFalse(obj, time));
    }

    IEnumerator ActiveFalse(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
