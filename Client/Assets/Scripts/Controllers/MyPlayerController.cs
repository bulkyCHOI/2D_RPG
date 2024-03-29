using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;

    public int WeaponDamage { get; private set; }
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
            else if (((Weapon)equipedWeapon).WeaponType == WeaponType.Melee)
                skill.Info.SkillId = 1;
            else if (((Weapon)equipedWeapon).WeaponType == WeaponType.Range)
                skill.Info.SkillId = 2;
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
        WeaponDamage = 0;
        ArmorDefence = 0;

        foreach (Item item in Managers.Inventory.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    WeaponDamage += ((Weapon)item).Damage;
                    break;
                case ItemType.Armor:
                    ArmorDefence += ((Armor)item).Defence;
                    break;
            }
        }
    }
}

