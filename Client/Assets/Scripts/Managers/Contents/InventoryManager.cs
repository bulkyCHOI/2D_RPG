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

    public void EditItemCount(Item item)
    {
        //items���� item.id�� ã�Ƽ� count�� �������ش�.
        Items[item.itemDbId].Count = item.Count;
    }

    public void EditItemSlot(Item item)
    {
        //items���� item.id�� ã�Ƽ� slot�� �������ش�.
        Items[item.itemDbId].Slot = item.Slot;
    }

    public void SwitchItemSlot(Item item1, Item item2)
    {
        //item1�� item2�� slot�� ���� �ٲ��ش�.
        EditItemSlot(item1);
        EditItemSlot(item2);
    }

    public void Remove(Item item)
    {
        Items.Remove(item.itemDbId);
    }
}
