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
        Transform dropped = eventData.pointerDrag.transform;    //�巡���� ������
        Transform droppedParent = dropped.GetComponent<UI_EventDrag>().parentAfterDrag;   //�巡���� �������� �θ�
        dropped.SetParent(transform.parent);   //�巡���� �������� ������ �θ�� �������´�.
        

        //Debug.Log($"dropped: {dropped.parent.gameObject.name}");
        //Debug.Log($"transform: {transform.parent.gameObject.name}");

        //������ ������ �ƴϸ� OnDrop�� ȣ����� �ʴ´�.
        //if (dropped.GetComponent<UI_EventHandler>() == null) //���� ����� ��ġ�� UI_EventHandler�� ���ٸ� ����� ����Ѵ�.
        //{
        //    transform.SetParent(parentAfterDrag);
        //    return;
        //}

        ////����� �������� �θ�� ����� �������� �θ� �ٲ۴�.
        //Transform prev_transform = transform;
        //dropped.SetParent(prev_transform.parent);
        Debug.Log($"����: {droppedParent.gameObject.name}");       //����
        //transform.SetParent(dropped.parent);

        Debug.Log($"���: {transform.gameObject.name}");   //����
        //Debug.Log($"���: {drop.parent.gameObject.name}");   //����

        //slot�� �ٲ���ٰ� ������ �˷��ش�.
        if (dropped.GetChild(0).gameObject.activeSelf == true) //�巡���ϴ� ���Կ� �������� ��������
        {
            C_ItemSlotChange c_ItemSlotChange = new C_ItemSlotChange();
            int slot1 = int.Parse(droppedParent.name.Split('_')[1]);
            int slot2 = int.Parse(transform.name.Split('_')[1]);

            c_ItemSlotChange.Slot1 = slot2;
            c_ItemSlotChange.Item1DbId = Managers.Inventory.Find(i => i.Slot == slot1).itemDbId;

            if (transform.GetChild(0).GetChild(0).gameObject.activeSelf == true && slot1 != slot2) //����� ���Կ� �������� �������
            {
                c_ItemSlotChange.Slot2 = slot1;
                c_ItemSlotChange.Item2DbId = Managers.Inventory.Find(i => i.Slot == slot2).itemDbId;
            }   //else�� item2�� null�� �д�.

            Managers.Network.Send(c_ItemSlotChange);
        }

        droppedParent = transform;  //�巡���� �������� �θ� ����� �������� �θ�� �ٲ۴�.
    }
}
