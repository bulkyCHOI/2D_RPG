using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Progress;

public class Item   //Item 정보를 메모리에 가지고 있는 클래스
{
    public ItemInfo Info { get; } = new ItemInfo();
    public int itemDbId
    {
        get { return Info.ItemDbId; }
        set { Info.ItemDbId = value; }
    }
    public int TemplateId
    {
        get { return Info.TemplateId; }
        set { Info.TemplateId = value; }
    }
    public int Count
    {
        get { return Info.Count; }
        set { Info.Count = value; }
    }
    public int Slot
    {
        get { return Info.Slot; }
        set { Info.Slot = value; }
    }
    public int Grade
    {
        get { return Info.Grade; }
        set { Info.Grade = value; }
    }
    public bool Equipped
    {
        get { return Info.Equipped; }
        set { Info.Equipped = value; }
    }


    public ItemType ItemType { get; private set; }
    public bool IsStackable { get; protected set; }

    public Item(ItemType itemType)
    {
        ItemType = itemType;
    }

    public static Item MakeItem(ItemInfo itemInfo)
    {
        Item item = null;
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);
        if (itemData == null)
            return null;
        switch (itemData.itemType)
        {
            case ItemType.Weapon:
                item = new Weapon(itemInfo.TemplateId);
                break;
            case ItemType.Armor:
                item = new Armor(itemInfo.TemplateId);
                break;
            case ItemType.Consumable:
                item = new Consumable(itemInfo.TemplateId);
                break;
        }
        if (item != null)
        {
            item.itemDbId = itemInfo.ItemDbId;
            item.Count = itemInfo.Count;
            item.Slot = itemInfo.Slot;
            item.Equipped = itemInfo.Equipped;
            item.Grade = itemInfo.Grade;
        }
        return item;
    }
}

public class Weapon : Item
{
    public WeaponType WeaponType { get; private set; }
    public int Damage { get; private set; }

    public Weapon(int templateId) : base(ItemType.Weapon)
    {
        Init(templateId);
    }

    void Init(int templateId)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
        if (itemData.itemType != ItemType.Weapon)
            return;

        WeaponData data = (WeaponData)itemData;
        {
            TemplateId = data.id;
            Count = 1;
            WeaponType = data.weaponType;
            Damage = data.damage;
            IsStackable = false;
        }
    }
}

public class Armor : Item
{
    public ArmorType ArmorType { get; private set; }
    public int Defence { get; private set; }

    public Armor(int templateId) : base(ItemType.Armor)
    {
        Init(templateId);
    }

    void Init(int templateId)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
        if (itemData.itemType != ItemType.Armor)
            return;

        ArmorData data = (ArmorData)itemData;
        {
            TemplateId = data.id;
            Count = 1;
            ArmorType = data.armorType;
            Defence = data.defence;
            IsStackable = false;
        }
    }
}

public class Consumable : Item
{
    public ConsumableType ConsumableType { get; private set; }
    public int MaxCount { get; private set; }

    public Consumable(int templateId) : base(ItemType.Consumable)
    {
        Init(templateId);
    }

    void Init(int templateId)
    {
        ItemData itemData = null;
        Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
        if (itemData.itemType != ItemType.Consumable)
            return;

        ConsumableData data = (ConsumableData)itemData;
        {
            TemplateId = data.id;
            Count = 1;
            ConsumableType = data.consumableType;
            MaxCount = data.maxCount;
            IsStackable = (data.maxCount > 1);
        }
    }
}
