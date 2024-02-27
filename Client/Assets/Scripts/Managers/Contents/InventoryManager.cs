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

    public Item Find(Func<Item, bool> condition)    //Func<Item, bool> ��������Ʈ�� ����Ͽ� ������ �޾Ƶ��̴� Find �޼���
    {
        foreach (Item item in Items.Values)    //foreach���� ����Ͽ� _items�� ��� ��Ҹ� ��ȸ
        {
            if (condition.Invoke(item)) //condition ��������Ʈ�� ȣ���Ͽ� ������ �˻�
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
        //items���� item.id�� ã�Ƽ� count�� �������ش�.
        Items[item.itemDbId].Count += item.Count;
    }
}
