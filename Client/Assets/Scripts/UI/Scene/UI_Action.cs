using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using static UnityEditor.Progress;

public class UI_Action : UI_Base
{
    enum Images
    {
        ActionSlot1Icon,
        ActionSlot2Icon,
        ActionSlot3Icon,
        ActionSlot4Icon,
    }

    enum Texts
    {
        ActionSlot1Text,
        ActionSlot2Text,
        ActionSlot3Text,
        ActionSlot4Text,
    }

    //bool _isInit = false;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        //_isInit = true;   //�׳� �ݵ�� ����Ǿ�� �ϹǷ�
        //RefreshUI(); //Inventory�� ��������� ���Ŀ� �˼� �����Ƿ� ���⼭�� �������� ���� >> �������� itemlist ��Ŷ�� �޾ƿ��� ����
    }

    public void RefreshUI()
    {
        //if (_isInit == false)
        //    return;

        Get<Image>((int)Images.ActionSlot1Icon).enabled = false;
        Get<Image>((int)Images.ActionSlot2Icon).enabled = false;
        Get<Image>((int)Images.ActionSlot3Icon).enabled = false;
        Get<Image>((int)Images.ActionSlot4Icon).enabled = false;
        Get<Text>((int)Texts.ActionSlot1Text).text = "";
        Get<Text>((int)Texts.ActionSlot2Text).text = "";
        Get<Text>((int)Texts.ActionSlot3Text).text = "";
        Get<Text>((int)Texts.ActionSlot4Text).text = "";

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
                    Get<Image>((int)Images.ActionSlot1Icon).sprite = icon;
                    Get<Image>((int)Images.ActionSlot1Icon).enabled = true;
                    Get<Text>((int)Texts.ActionSlot1Text).text = item.Count.ToString();
                }
                else if (consumable.ConsumableType == ConsumableType.MpPortion)
                {
                    Get<Image>((int)Images.ActionSlot2Icon).sprite = icon;
                    Get<Image>((int)Images.ActionSlot2Icon).enabled = true;
                    Get<Text>((int)Texts.ActionSlot2Text).text = item.Count.ToString();
                }
            }
        }
    }
}
