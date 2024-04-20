using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;

    public int MeleeDamage { get; private set; }
    public int RangeDamage { get; private set; }
    public int ArmorDefence { get; private set; }

    protected override void Init()
    {
        base.Init();
        RefreshAdditionalStat();
    }
    protected override void UpdateController()
    {
        GetUIKeyInput();

        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }
    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        // 스킬 상태로 갈지 확인
        if (_coInputCooltime == null && Input.GetKey(KeyCode.Space))
        {
            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            //장착된 아이템의 type을 확인하여 스킬 아이디를 설정
            Item equipedWeapon = null;
            equipedWeapon = Managers.Inventory.Find(
                        i => i.ItemType == ItemType.Weapon && i.Equipped);
            if(equipedWeapon == null) //맨손
                skill.Info.SkillId = 1;
            else //if (((Weapon)equipedWeapon).WeaponType == WeaponType.Melee)
                skill.Info.SkillId = 2;
            //else if (((Weapon)equipedWeapon).WeaponType == WeaponType.Range)
            //    skill.Info.SkillId = 2;
            Managers.Network.Send(skill);

            _coInputCooltime = StartCoroutine(CoInputCooltime(0.2f));
        }
        else if (_coInputCooltime == null && Input.GetKey(KeyCode.Q))
        {
            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            //장착된 아이템의 type을 확인하여 스킬 아이디를 설정
            //Item equipedWeapon = null;
            //range weapon 찾기
            //equipedWeapon = Managers.Inventory.Find(
            //            i => i.ItemType == ItemType.Weapon && i.Equipped);
            //if (((Weapon)equipedWeapon).WeaponType == WeaponType.Range)
            skill.Info.SkillId = 3; //일단 그냥 스킬 나감
            Managers.Network.Send(skill);

            _coInputCooltime = StartCoroutine(CoInputCooltime(0.2f));
        }
    }

    Coroutine _coInputCooltime; //쿨타임을 위한 코루틴
    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _coInputCooltime = null;
    }

    // UI 키보드 입력
    void GetUIKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

            if (invenUI.gameObject.activeSelf == false)
            {
                invenUI.gameObject.SetActive(true);
                invenUI.RefreshUI();
            }
            else
            {
                invenUI.gameObject.SetActive(false);
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Stat statUI = gameSceneUI.StatUI;

            if (statUI.gameObject.activeSelf == false)
            {
                statUI.gameObject.SetActive(true);
                statUI.RefreshUI();
            }
            else
            {
                statUI.gameObject.SetActive(false);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Action actionUI = gameSceneUI.ActionUI;
            if (actionUI.item1 != null)
                actionUI.SlotClick(actionUI.item1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Action actionUI = gameSceneUI.ActionUI;
            if (actionUI.item2 != null)
                actionUI.SlotClick(actionUI.item2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Action actionUI = gameSceneUI.ActionUI;
            if (actionUI.item3 != null)
                actionUI.SlotClick(actionUI.item3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Action actionUI = gameSceneUI.ActionUI;
            if (actionUI.item4 != null)
                actionUI.SlotClick(actionUI.item4);
        }
        else if(Input.GetKeyDown(KeyCode.F))
        {
            //UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            //UI_Vendor vendorUI = gameSceneUI.VendorUI;
            //UI_Inventory invenUI = gameSceneUI.InvenUI;

            //if (vendorUI.gameObject.activeSelf == false)
            //{
            //    vendorUI.gameObject.SetActive(true);
            //    invenUI.gameObject.SetActive(true);
            //    vendorUI.RefreshUI();
            //    invenUI.RefreshUI();
            //}
            //else
            //{
            //    vendorUI.gameObject.SetActive(false);
            //    invenUI.gameObject.SetActive(false);
            //}
            //패킷을 만들어서 서버로 보내자
            //캐릭터가 서있는 위치/방향으로 앞에 누가 있는지 확인
            C_VendorInteraction vendorInven = new C_VendorInteraction();
            Managers.Network.Send(vendorInven);

        }
    }
    // 키보드 입력
    void GetDirInput()
    {
        _moveKeyPressed = true;
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }

    protected override void MoveToNextPos()
    {
        if (_moveKeyPressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag(); //아래서 바로 리턴이 되므로 추가
            return;
        }

        Vector3Int destPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.FindCreature(destPos) == null)
            {
                CellPos = destPos;
            }
        }

        CreatureState prevState = State;
        Vector3Int prevCellPos = CellPos;

        CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    public void RefreshAdditionalStat() //공격력/방어력 갱신이 아닌 UI에 표시할 수치를 갱신
    {
        MeleeDamage = 0;
        RangeDamage = 0;
        ArmorDefence = 0;

        foreach (Item item in Managers.Inventory.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    if (((Weapon)item).WeaponType == WeaponType.Melee)
                        MeleeDamage += (int)Math.Round(((Weapon)item).Damage * ((((Weapon)item).Enchant * 0.5) + 1));
                    else if (((Weapon)item).WeaponType == WeaponType.Range)
                        RangeDamage += (int)Math.Round(((Weapon)item).Damage * ((((Weapon)item).Enchant * 0.5) + 1));
                    break;
                case ItemType.Armor:
                    ArmorDefence += (int)Math.Round(((Armor)item).Defence * ((((Armor)item).Enchant * 0.5) + 1));
                    break;
            }
        }
    }

    public override void OnDead(int attackerId)
    {
        base.OnDead(attackerId);

        //나자신이 죽였다면 패스
        if (attackerId == Managers.Object.MyPlayer.Id)
            return;

        //죽엇다는 메시지 출력
        string attackerName = Managers.Object.FindById(attackerId).name;
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.PopupMessage.SetActiveFalse(gameSceneUI.PopupMessage.errorMsg1Popup, $"{attackerName}에게 죽음", 5.0f);
    }
}

