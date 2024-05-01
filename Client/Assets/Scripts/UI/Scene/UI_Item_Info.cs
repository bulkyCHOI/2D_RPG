using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Item_Info : UI_Base
{
    [SerializeField]
    Image _icon = null;
    [SerializeField]
    Image _background = null;
    [SerializeField]
    Image _frame = null;
    [SerializeField]
    Text _text = null;
    [SerializeField]
    TMP_Text _itemType = null;
    [SerializeField]
    TMP_Text _itemName = null;
    [SerializeField]
    TMP_Text _itemInfo = null;

    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public int Grade { get; private set; }
    public bool Equipped { get; private set; }

    public override void Init()
    {
    }

    public void SetItem(Item item)
    {
        if (item == null)
        {
            ItemDbId = 0;
            TemplateId = 0;
            Count = 0;
            Grade = 0;
            Equipped = false;

            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
            _background.color = new Color(0, 0, 0, 0);
            _text.text = "";
        }
        else
        {
            ItemDbId = item.itemDbId;
            TemplateId = item.TemplateId;
            Count = item.Count;
            Grade = item.Grade;
            Equipped = item.Equipped;

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            _icon.sprite = icon;

            switch (Grade)
            {
                case 0:
                    _background.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                    break;
                case 1:
                    _background.color = new Color(0.1f, 0.7f, 0.1f, 0.8f);
                    break;
                case 2:
                    _background.color = new Color(0f, 0f, 1f, 0.8f);
                    break;
                case 3:
                    _background.color = new Color(1f, 0f, 1f, 0.8f);
                    break;
                case 4:
                    _background.color = new Color(0.9f, 0.5f, 0.1f, 0.8f);
                    break;
                case 5:
                    _background.color = new Color(1f, 0f, 0f, 0.8f);
                    break;
            }

            _icon.gameObject.SetActive(true);
            _frame.gameObject.SetActive(Equipped);
            if (itemData.itemType == ItemType.Consumable)
                _text.text = Count.ToString();
            else
                //enchant가 있다면 표시하기
                _text.text = item.Enchant > 0 ? $"+{item.Enchant}" : "";

            //인벤토리에서 아이템 정보 가져오기

            string description = null;
            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    Weapon weapon = (Weapon)item;
                    int enchantDamage = (int)(weapon.Damage * (item.Enchant * 0.5 + 1));
                    description = $"Type: {item.ItemType}";
                    description += $"\nDamage: {enchantDamage} (+ {enchantDamage-weapon.Damage})";
                    break;
                case ItemType.Armor:
                    Armor armor = (Armor)item;
                    int enchantDefence = (int)(armor.Defence * (item.Enchant * 0.5 + 1));
                    description = $"Type: {armor.ArmorType}";
                    description += $"\nDefence: {enchantDefence} (+ {enchantDefence-armor.Defence})";
                    break;
                case ItemType.Consumable:
                    Consumable consumable = (Consumable)item;
                    description = $"Type: {consumable.ConsumableType}";
                    switch (consumable.ConsumableType)
                    {
                        case ConsumableType.HpPortion:
                            description += $"\nHP회복: {consumable.RecoveryAmount}";
                            break;
                        case ConsumableType.MpPortion:
                            description += $"\nMP회복: {consumable.RecoveryAmount}";
                            break;
                    }
                    break;
            }
            switch(itemData.grade)
            {
                case 0:
                    description += "\nGrade: Common";
                    break;
                case 1:
                    description += "\nGrade: Uncommon";
                    break;
                case 2:
                    description += "\nGrade: Rare";
                    break;
                case 3:
                    description += "\nGrade: Epic";
                    break;
                case 4:
                    description += "\nGrade: Legendary";
                    break;
                case 5:
                    description += "\nGrade: Mythic";
                    break;
            }
            description += $"\nPrice: {itemData.price.ToString("N0")}";


            _itemName.text = itemData.name;
            _itemType.text = itemData.itemType.ToString();
            _itemInfo.text = description;
        }
    }

    //public void RemoveItem()
    //{
    //    _icon.gameObject.SetActive(false);
    //    _frame.gameObject.SetActive(false);
    //    _background.color = new Color(0,0,0,0.5f);
    //    _text.text = "";
    //}
    public void OnClickItemInfo()
    {
        gameObject.SetActive(false);
    }
}
