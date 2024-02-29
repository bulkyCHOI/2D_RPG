using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Inventory : UI_Base
{
    public List<UI_Inventory_Item> Items { get; } = new List<UI_Inventory_Item>();
    public override void Init()
    {
        Items.Clear();

        GameObject grid = transform.Find("ItemGrid").gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for(int i = 0; i < 30; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
            UI_Inventory_Item item = go.GetOrAddComponent<UI_Inventory_Item>();
            Items.Add(item);
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (Items.Count == 0)   //init이 안된경우 0이므로 크래시 발생
            return;
        List<Item> items = Managers.Inventory.Items.Values.ToList();    //InventoryManager의 Items를 List로 변환
        items.Sort((left, right) => { return left.Slot - right.Slot; }); //sorting

        for (int i = 0; i < 30; i++)
        {
            Items[i].SetItem(null);
            //Items[i].RemoveItem();
        }
        foreach(Item item in items)
        {
            if (item.Slot < 0 || 30 <= item.Slot)
                continue;

            Items[item.Slot].SetItem(item);  
        }

        //구조를 바꿔 아이템 리스트로 하는게 아니고 슬롯단위로 없으면 비활성화 시키는 RemoveItem()써야한다.

    }
}
