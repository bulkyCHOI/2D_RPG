using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager
{
    public string BaseUrl { get; set; } = "http://localhost:5000/api";

    public void SendPostRequest<T>(string uri, object obj, Action<T> res)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(uri, UnityWebRequest.kHttpVerbPOST, obj, res));
    }

    IEnumerator CoSendWebRequest<T>(string uri, string method, object obj, Action<T> res)
    {
        string sendUrl = $"{BaseUrl}/{uri}";

        byte[] jsonBytes = null;
        if (obj != null)
        {
            string json = JsonUtility.ToJson(obj);
            jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        }

        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.uploadHandler.contentType = "application/json";

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                T resObj = JsonUtility.FromJson<T>(uwr.downloadHandler.text);
                res.Invoke(resObj);
            }
        }
    }
}
