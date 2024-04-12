using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Vendor : UI_Base
{
    public List<UI_Vendor_Item> Items { get; } = new List<UI_Vendor_Item>();
    public override void Init()
    {
        Items.Clear();

        GameObject grid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for(int i = 0; i < 30; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Vendor_Item", grid.transform);
            UI_Vendor_Item item = go.GetOrAddComponent<UI_Vendor_Item>();
            Items.Add(item);
        }
        RefreshUI(null);
    }

    public void RefreshUI(List<VendorItemInfo> items)
    {
        if (Items.Count == 0)   //init�� �ȵȰ�� 0�̹Ƿ� ũ���� �߻�
            return;
        
        for (int i = 0; i < 30; i++)
        {
            Items[i].SetItem(null);
            //Items[i].RemoveItem();
        }
        foreach (VendorItemInfo item in items)
        {
            if (item.Slot < 0 || 30 <= item.Slot)
                continue;

            Items[item.Slot].SetItem(item);
        }

        //������ �ٲ� ������ ����Ʈ�� �ϴ°� �ƴϰ� ���Դ����� ������ ��Ȱ��ȭ ��Ű�� RemoveItem()����Ѵ�.

    }
}
