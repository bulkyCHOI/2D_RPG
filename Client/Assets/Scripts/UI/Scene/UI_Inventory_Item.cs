using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;
    [SerializeField]
    Image _background = null;
    [SerializeField]
    Image _frame = null;
    [SerializeField]
    Text _text = null;

    public int ItemDbId { get; private set; }
    public int TemplateId { get; private set; }
    public int Count { get; private set; }
    public int Grade { get; private set; }
    public bool Equipped { get; private set; }

    public override void Init()
    {
        _icon.gameObject.BindEvent((e) =>
        {
            Debug.Log("아이템 클릭");

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            if(itemData == null)
                return;
            //TODO: 아이템 사용 >> C_USE_ITEM
            //if(itemData.itemType == ItemType.Consumable)
            //    return;
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Vendor vendorUI = gameSceneUI.VendorUI;

            if (vendorUI.gameObject.activeSelf == false)
            {
                C_EquipItem equipPacket = new C_EquipItem();
                equipPacket.ItemDbId = ItemDbId;
                equipPacket.Equipped = !Equipped;
                Managers.Network.Send(equipPacket);
            }
            else
            {
                Debug.Log("판매");
                C_SellItem sellPacket = new C_SellItem();
                sellPacket.ItemDbId = ItemDbId;
                Managers.Network.Send(sellPacket);
                RemoveItem();
            }
        });
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
                _text.text = "";
        }
    }

    public void RemoveItem()
    {
        _icon.gameObject.SetActive(false);
        _frame.gameObject.SetActive(false);
        _background.color = new Color(0,0,0,0.5f);
        _text.text = "";
    }
}
