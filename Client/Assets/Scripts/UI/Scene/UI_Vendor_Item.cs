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
    Image _frame = null;
    [SerializeField]
    Text _text = null;

    public int TemplateId { get; private set; }
    public int Slot { get; private set; }
    public int Price { get; private set; }

    public override void Init()
    {
        _icon.gameObject.BindEvent((e) =>
        {
            Debug.Log($"아이템 구매: {TemplateId}({Price})");

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            if(itemData == null)
                return;
            //TODO: 아이템 사용 >> C_USE_ITEM
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

            _icon.gameObject.SetActive(false);
            _frame.gameObject.SetActive(false);
        }
        else
        {
            TemplateId = item.ItemId    ;
            Slot = item.Slot;
            Price = item.Price;

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(TemplateId, out itemData);

            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            _icon.sprite = icon;

            _icon.gameObject.SetActive(true);
        }
    }

    public void RemoveItem()
    {
        _icon.gameObject.SetActive(false);
        _frame.gameObject.SetActive(false);
        _text.text = "";
    }
}
