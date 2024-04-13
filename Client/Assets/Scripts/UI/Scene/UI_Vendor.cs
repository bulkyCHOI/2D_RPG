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
        RefreshUI();
    }

    public void RefreshUI(List<VendorItemInfo> items = null)
    {
        if (Items.Count == 0)   //init이 안된경우 0이므로 크래시 발생
            return;

        for (int i = 0; i < 30; i++)
        {
            Items[i].SetItem(null);
            //Items[i].RemoveItem();
        }
        
        if (items == null)
            return;
        items.Sort((left, right) => { return left.Slot - right.Slot; }); //sorting
        foreach (VendorItemInfo item in items)
        {
            if (item.Slot < 0 || 30 <= item.Slot)
                continue;

            Items[item.Slot].SetItem(item);
        }

        //구조를 바꿔 아이템 리스트로 하는게 아니고 슬롯단위로 없으면 비활성화 시키는 RemoveItem()써야한다.

    }
}
