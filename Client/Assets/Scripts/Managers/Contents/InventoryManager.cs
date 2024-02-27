using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

    public void Add(Item item)
    {
        Items.Add(item.itemDbId, item);
    }

    public Item Get(int id)
    {
        Item item = null;
        Items.TryGetValue(id, out item);
        return item;
    }

    public Item Find(Func<Item, bool> condition)    //Func<Item, bool> 델리게이트를 사용하여 조건을 받아들이는 Find 메서드
    {
        foreach (Item item in Items.Values)    //foreach문을 사용하여 _items의 모든 요소를 순회
        {
            if (condition.Invoke(item)) //condition 델리게이트를 호출하여 조건을 검사
                return item;
        }
        return null;
    }

    public void Clear()
    {
        Items.Clear();
    }

    public void AddItemCount(Item item)
    {
        //items에서 item.id로 찾아서 count를 변경해준다.
        Items[item.itemDbId].Count += item.Count;
    }
}
