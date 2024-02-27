using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Action : UI_Base
{
    enum Images
    {
        Action_Slot1,
        Action_Slot2,
        Action_Slot3,
        Action_Slot4,
    }

    bool _isInit = false;

    public override void Init()
    {
        Bind<Image>(typeof(Images));

        _isInit = true;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_isInit == false)
            return;

        Get<Image>((int)Images.Action_Slot1).enabled = false;
        Get<Image>((int)Images.Action_Slot2).enabled = false;
        Get<Image>((int)Images.Action_Slot3).enabled = false;
        Get<Image>((int)Images.Action_Slot4).enabled = false;

        foreach (Item item in Managers.Inventory.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);

            if(itemData.itemType == ItemType.Consumable)
            {
                Consumable consumable = (Consumable)item;
                if (consumable.ConsumableType == ConsumableType.HpPortion)
                {
                    Get<Image>((int)Images.Action_Slot1).sprite = icon;
                    Get<Image>((int)Images.Action_Slot1).enabled = true;
                }
                else if (consumable.ConsumableType == ConsumableType.MpPortion)
                {
                    Get<Image>((int)Images.Action_Slot2).sprite = icon;
                    Get<Image>((int)Images.Action_Slot2).enabled = true;
                }
            }
        }
    }
}
