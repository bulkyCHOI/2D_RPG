using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat : UI_Base
{
    enum Images
    {
        Slot_Helmet,
        Slot_Armor,
        Slot_Pants,
        Slot_Boots,
        Slot_WeaponMelee,
        Slot_WeaponRange,
    }

    enum Texts
    {
        NameText,
        MeleeValueText,
        RangeValueText,
        DefenceValueText,
    }

    bool _isInit = false;
    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        _isInit = true;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if(_isInit == false)
            return;

        //�켱 ��������
        //Get<Image>((int)Images.Slot_Helmet).gameObject.SetActive(false);    //���ӿ�����Ʈ
        Get<Image>((int)Images.Slot_Helmet).enabled = false;    //image ������Ʈ
        Get<Image>((int)Images.Slot_Armor).enabled = false;
        Get<Image>((int)Images.Slot_Pants).enabled = false;
        Get<Image>((int)Images.Slot_Boots).enabled = false;
        Get<Image>((int)Images.Slot_WeaponMelee).enabled = false;
        Get<Image>((int)Images.Slot_WeaponRange).enabled = false;

        //���â�� ������ ������ ������ �����ͼ� Ȱ��ȭ
        foreach (Item item in Managers.Inventory.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            Data.ItemData itemData = null;
            Managers.Data.ItemDict.TryGetValue(item.TemplateId, out itemData);
            Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);
            
            switch (itemData.itemType)
            {
                case ItemType.Weapon:
                    Weapon weapon = (Weapon)item;
                    switch (weapon.WeaponType)
                    {
                        case WeaponType.Melee:
                            Get<Image>((int)Images.Slot_WeaponMelee).enabled = true;
                            Get<Image>((int)Images.Slot_WeaponMelee).sprite = icon;
                            break;
                        case WeaponType.Range:
                            Get<Image>((int)Images.Slot_WeaponRange).enabled = true;
                            Get<Image>((int)Images.Slot_WeaponRange).sprite = icon;
                            break;
                    }
                    break;
                case ItemType.Armor:
                    Armor armor = (Armor)item;
                    switch(armor.ArmorType)
                    {
                        case ArmorType.Helmet:
                            Get<Image>((int)Images.Slot_Helmet).enabled = true;
                            Get<Image>((int)Images.Slot_Helmet).sprite = icon;
                            break;
                        case ArmorType.Armor:
                            Get<Image>((int)Images.Slot_Armor).enabled = true;
                            Get<Image>((int)Images.Slot_Armor).sprite = icon;
                            break;
                        case ArmorType.Pants:
                            Get<Image>((int)Images.Slot_Pants).enabled = true;
                            Get<Image>((int)Images.Slot_Pants).sprite = icon;
                            break;
                        case ArmorType.Boots:
                            Get<Image>((int)Images.Slot_Boots).enabled = true;
                            Get<Image>((int)Images.Slot_Boots).sprite = icon;
                            break;
                    }
                    break;       
            }
        }

        //Text
        MyPlayerController player = Managers.Object.MyPlayer;
        player.RefreshAdditionalStat();

        Get<Text>((int)Texts.NameText).text = player.name;
        int totalMeleeAttack = player.Stat.Attack + player.MeleeDamage;
        int totalRangeAttack = player.Stat.Attack + player.RangeDamage;
        int totalDefence = player.Stat.Defence + player.ArmorDefence;
        Get<Text>((int)Texts.MeleeValueText).text = $"{totalMeleeAttack}(+{player.MeleeDamage})";
        Get<Text>((int)Texts.RangeValueText).text = $"{totalRangeAttack}(+{player.RangeDamage})";
        Get<Text>((int)Texts.DefenceValueText).text = $"{totalDefence}(+{player.ArmorDefence})";
    }
}
