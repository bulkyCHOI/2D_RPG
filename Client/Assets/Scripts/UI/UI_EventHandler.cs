using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        //transform.SetParent(transform.parent);
        //transform.SetAsLastSibling();
        gameObject.GetComponent<Image>().raycastTarget = false; //raycastTarget을 false로 해야 OnDrop할때 드래그중인 아이템이 아니라 밑에 선택한 아이템이 detect된다.
        
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
        //drag한 아이템을 원래의 부모로 돌려놓는다.
        Debug.Log("EndDrag");
        transform.position = parentAfterDrag.position;
        //transform.SetParent(parentAfterDrag); //동일한 부모이기 때문에 이동이 정렬이 없다.

        //Transform dropped = eventData.pointerDrag.transform;  //여기서는 드래그 완료된 포인트의 아이템이 아닌 드래그중인 아이템이 된다.
        //Debug.Log($"dropped: {dropped.parent.gameObject.name}");
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
        gameObject.GetComponent<Image>().raycastTarget = true;

        if (OnEndDragHandler != null)
            OnEndDragHandler.Invoke(eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop");
        Transform dropped = eventData.pointerDrag.transform;
        //Debug.Log($"dropped: {dropped.parent.gameObject.name}");
        //Debug.Log($"transform: {transform.parent.gameObject.name}");
        
        //아이템 슬롯이 아니면 OnDrop이 호출되지 않는다.
        //if (dropped.GetComponent<UI_EventHandler>() == null) //만약 드랍된 위치에 UI_EventHandler가 없다면 드랍을 취소한다.
        //{
        //    transform.SetParent(parentAfterDrag);
        //    return;
        //}

        ////드랍된 아이템의 부모와 드랍한 아이템의 부모를 바꾼다.
        Transform prev_transform = transform;
        //dropped.SetParent(prev_transform.parent);
        Debug.Log($"시작: {dropped.parent.gameObject.name}");       //종료
        //transform.SetParent(dropped.parent);
        Debug.Log($"드랍: {transform.parent.gameObject.name}");   //시작



        //slot이 바뀌었다고 서버에 알려준다.
        C_ItemSlotChange c_ItemSlotChange = new C_ItemSlotChange();
        int slot1 = int.Parse(dropped.parent.name.Split('_')[1]);
        int slot2 = int.Parse(transform.parent.name.Split('_')[1]);

        c_ItemSlotChange.Slot1 = slot2;
        c_ItemSlotChange.Item1DbId = Managers.Inventory.Find(i => i.Slot == slot1).itemDbId;

        if (transform.GetChild(0).gameObject.activeSelf == true) //드랍된 슬롯에 아이템이 있을경우
        {
            c_ItemSlotChange.Slot2 = slot1;
            c_ItemSlotChange.Item2DbId = Managers.Inventory.Find(i => i.Slot == slot2).itemDbId;
        }   //else는 item2를 null로 둔다.
        
        Managers.Network.Send(c_ItemSlotChange);

        if (OnDragHandler != null)
            OnDragHandler.Invoke(eventData);
    }
}
