using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EventDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    public CanvasGroup canvasGroup;

    //초기화
    private void Awake()
    {
        Debug.Log("Awake");
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BeginDrag");
        parentAfterDrag = transform.parent;
        //Debug.Log(parentAfterDrag.position);
        transform.SetParent(transform.parent.parent);
        transform.SetAsLastSibling();   //드래그하는 아이템을 맨 위로 올린다.
        //gameObject.GetComponent<Image>().raycastTarget = false; //raycastTarget을 false로 해야 OnDrop할때 드래그중인 아이템이 아니라 밑에 선택한 아이템이 detect된다. >> 안됨
        canvasGroup.blocksRaycasts = false; //blocksRaycasts를 false로 해야 OnDrop할때 드래그중인 아이템이 아니라 밑에 선택한 아이템이 detect된다.

    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");
        //Debug.Log(transform.GetChild(0).gameObject.name);
        //Debug.Log(transform.GetChild(0).gameObject.activeSelf);

        //하위 게임오브젝트의 ItemIcon이 활성화 된 경우만 드래그가 되도록 한다.
        if (transform.GetChild(0).gameObject.activeSelf == true)
            transform.position = Input.mousePosition;

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("=====OnEndDrag=====");

        //drag한 아이템을 원래의 부모로 돌려놓는다.
        //gameObject.GetComponent<Image>().raycastTarget = true;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(parentAfterDrag); //동일한 부모이기 때문에 이동이 정렬이 없다.
        //원래 위치로 돌아갈 필요가 없다면
        //transform.position = parentAfterDrag.position;

        //Debug.Log($"transform: {transform.parent.gameObject.name}");

        //transform.SetParent(parentAfterDrag);
        //transform.SetAsLastSibling();
        //if(dropped.GetChild(0).gameObject.activeSelf == false)
        //    transform.SetParent(parentAfterDrag);
        //else
        //{
        //    transform.SetParent(dropped.parent);
        //    dropped.SetParent(parentAfterDrag);
        //}

    }
}
