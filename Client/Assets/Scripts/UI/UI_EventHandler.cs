using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnBeginDragHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnEndDragHandler = null;
    public Action<PointerEventData> OnDropHandler = null;

    [HideInInspector] public Transform parentAfterDrag;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (OnClickHandler != null)
			OnClickHandler.Invoke(eventData);
	}

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BeginDrag");
        parentAfterDrag = transform.parent;
        //Debug.Log(parentAfterDrag.position);
        transform.SetParent(transform.parent);
        transform.SetAsLastSibling();
        
        if (OnBeginDragHandler != null)
            OnBeginDragHandler.Invoke(eventData);
    }
	public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");
        //하위 게임오브젝트의 ItemIcon이 활성화 된 경우만 드래그가 되도록 한다.
        //Debug.Log(transform.GetChild(0).gameObject.name);
        //Debug.Log(transform.GetChild(0).gameObject.activeSelf);
        if (transform.GetChild(0).gameObject.activeSelf == true)
            transform.position = Input.mousePosition;
		
        if (OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
	}
	public void OnEndDrag(PointerEventData eventData)
	{
        //transform.position = parentAfterDrag.position;
        transform.SetParent(parentAfterDrag);
        Debug.Log("EndDrag");
        
        if (OnEndDragHandler != null)
            OnEndDragHandler.Invoke(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop");
        Transform dropped = eventData.pointerDrag.transform;
        //드랍된 아이템의 부모와 드랍한 아이템의 부모를 바꾼다.
        transform.SetParent(dropped.parent);
        dropped.SetParent(parentAfterDrag);
        //parentAfterDrag = transform;
        //dropped.transform.SetParent(transform);
        //dropped.transform.position = transform.position;
        
        if(OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
    }
}
