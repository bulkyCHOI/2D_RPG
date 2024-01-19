using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static Define;

public class PlayerController : CreatureController
{
    Coroutine _coSkill;
    bool _rangedSkill = false;

    //start, update는 CreatureController에 있으므로 실행이 된다.
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (_state == CreatureState.Idle)
        {
            switch (_lastDir) //키보드를 떼도 마지막 방향으로 서있기
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_BACK");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Down:
                case MoveDir.None:
                    _animator.Play("IDLE_FRONT");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Left:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = true; //좌우반전
                    break;
                case MoveDir.Right:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = false; //원복
                    break;
            }
        }
        else if (_state == CreatureState.Moving)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = true; //좌우반전
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = false; //원래상태
                    break;
            }
        }
        else if (_state == CreatureState.Skill)
        {
            switch (_lastDir) //키보드를 떼도 마지막 방향으로 공격
            {
                case MoveDir.Up:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Down:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Left:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_LEFT" : "ATTACK_RIGHT");
                    _sprite.flipX = true; //좌우반전
                    break;
                case MoveDir.Right:
                    _animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
                    _sprite.flipX = false; //원래상태
                    break;
            }
        }
        else
        {

        }
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                GetIdleInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }
        base.UpdateController();
    }

    void LateUpdate() //카메라는 보통 LateUpdate
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    //키보드입력 받아서 방향 설정
    void GetDirInput()
    {
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
            Dir = MoveDir.None;
        }
    }
    void GetIdleInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            State = CreatureState.Skill;
            //_coSkill = StartCoroutine("CoStartPunch");
            _coSkill = StartCoroutine("CoStartShootArrow");
        }
    }

    IEnumerator CoStartPunch()
    {
        //피격판정
        GameObject go = Managers.Object.Find(GetFrontCellPos());
        if (go != null)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc != null)
                cc.OnDamaged();
        }
        //대기시간
        _rangedSkill = false;
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Idle;
        _coSkill = null;
    }
    IEnumerator CoStartShootArrow()
    {
        //화살 생성
        GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController ac = go.GetComponent<ArrowController>();
        ac.Dir = _lastDir;
        ac.CellPos = CellPos;

        //대기시간
        _rangedSkill = true;
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
    }
}
