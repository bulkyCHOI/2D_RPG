using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventDrop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("=====OnDrop=====");
        Transform dropped = eventData.pointerDrag.transform;    //드래그한 아이템
        Transform droppedParent = dropped.GetComponent<UI_EventDrag>().parentAfterDrag;   //드래그한 아이템의 부모
        dropped.SetParent(transform.parent);   //드래그한 아이템을 원래의 부모로 돌려놓는다.
        

        //Debug.Log($"dropped: {dropped.parent.gameObject.name}");
        //Debug.Log($"transform: {transform.parent.gameObject.name}");

        //아이템 슬롯이 아니면 OnDrop이 호출되지 않는다.
        //if (dropped.GetComponent<UI_EventHandler>() == null) //만약 드랍된 위치에 UI_EventHandler가 없다면 드랍을 취소한다.
        //{
        //    transform.SetParent(parentAfterDrag);
        //    return;
        //}

        ////드랍된 아이템의 부모와 드랍한 아이템의 부모를 바꾼다.
        //Transform prev_transform = transform;
        //dropped.SetParent(prev_transform.parent);
        Debug.Log($"시작: {droppedParent.gameObject.name}");       //종료
        //transform.SetParent(dropped.parent);

        Debug.Log($"드랍: {transform.gameObject.name}");   //시작
        //Debug.Log($"드랍: {drop.parent.gameObject.name}");   //시작

        //slot이 바뀌었다고 서버에 알려준다.
        if (dropped.GetChild(0).gameObject.activeSelf == true) //드래그하는 슬롯에 아이템이 있을때만
        {
            C_ItemSlotChange c_ItemSlotChange = new C_ItemSlotChange();
            int slot1 = int.Parse(droppedParent.name.Split('_')[1]);
            int slot2 = int.Parse(transform.name.Split('_')[1]);

            c_ItemSlotChange.Slot1 = slot2;
            c_ItemSlotChange.Item1DbId = Managers.Inventory.Find(i => i.Slot == slot1).itemDbId;

            if (transform.GetChild(0).GetChild(0).gameObject.activeSelf == true && slot1 != slot2) //드랍된 슬롯에 아이템이 있을경우
            {
                c_ItemSlotChange.Slot2 = slot1;
                c_ItemSlotChange.Item2DbId = Managers.Inventory.Find(i => i.Slot == slot2).itemDbId;
            }   //else는 item2를 null로 둔다.

            Managers.Network.Send(c_ItemSlotChange);
        }

        droppedParent = transform;  //드래그한 아이템의 부모를 드랍한 아이템의 부모로 바꾼다.
    }
}
