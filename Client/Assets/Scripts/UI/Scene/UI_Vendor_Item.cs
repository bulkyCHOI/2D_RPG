using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Vendor_Item : UI_Base
{
    [SerializeField]
    Image _icon = null;
    [SerializeField]
    Image _background = null;
    [SerializeField]
    Image _frame = null;
    [SerializeField]
    Text _text = null;

    public int TemplateId { get; private set; }
    public int Slot { get; private set; }
    public int Price { get; private set; }
    public int Grade { get; private set; }

    public override void Init()
    {
        _icon.gameObject.BindEvent((e) =>
        {
            Debug.Log($"아이템 구매: {TemplateId}({Price})");

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            if(itemData == null)
                return;
            
            //아이템 구매
            C_BuyItem buyPacket = new C_BuyItem();
            buyPacket.ItemId = TemplateId;
            Managers.Network.Send(buyPacket);

            //if(itemData.itemType == ItemType.Consumable)
            //    return;

            //C_EquipItem equipPacket = new C_EquipItem();
            //equipPacket.ItemDbId = ItemDbId;
            //equipPacket.Equipped = !Equipped;
            //Managers.Network.Send(equipPacket);

        });
    }

    public void SetItem(VendorItemInfo item)
    {
        if (item == null)
        {
            TemplateId = 0;
            Slot = 0;
            Price = 0;
            Grade = 0;
            
            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
            _background.color = new Color(0, 0, 0, 0.5f);
        }
        else
        {
            TemplateId = item.ItemId    ;
            Slot = item.Slot;
            Price = item.Price;

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);
            Grade = itemData.grade;

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
        }
    }

    public void RemoveItem()
    {
        _icon.gameObject.SetActive(false);
        _frame.gameObject.SetActive(false);
        _background.color = new Color(0, 0, 0, 0.5f);
        _text.text = "";
    }
}
