using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
    public UI_Stat StatUI { get; private set; }
    public UI_Inventory InvenUI { get; private set; }
    public UI_Action ActionUI { get; private set; }
    public UI_Level LevelUI { get; private set; }
    public UI_Vendor VendorUI { get; private set; }
    public UI_Enchant EnchantUI { get; private set; }


    public override void Init()
    {
        base.Init();

        StatUI = GetComponentInChildren<UI_Stat>();
        InvenUI = GetComponentInChildren<UI_Inventory>();
        ActionUI = GetComponentInChildren<UI_Action>();
        LevelUI = GetComponentInChildren<UI_Level>();
        VendorUI = GetComponentInChildren<UI_Vendor>();
        EnchantUI = GetComponentInChildren<UI_Enchant>();

        StatUI.gameObject.SetActive(false);
        InvenUI.gameObject.SetActive(false);
        VendorUI.gameObject.SetActive(false);
        EnchantUI.gameObject.SetActive(false);
    }
}
