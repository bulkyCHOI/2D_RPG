using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using static Define;

public class BaseController : MonoBehaviour
{
    public int Id { get; set; }

    StatInfo _stat = new StatInfo();
    public virtual StatInfo Stat
    {
        get { return _stat; }
        set
        {
            if (_stat.Equals(value))
                return;

            _stat.Level = value.Level;
            _stat.Hp = value.Hp;
            _stat.Mp = value.Mp;
            _stat.MaxHp = value.MaxHp;
            _stat.MaxMp = value.MaxMp;
            _stat.Speed = value.Speed;
            _stat.Str = value.Str;
            _stat.Dex = value.Dex;
            _stat.Int = value.Int;
        }
    }

    public float Speed
    {
        get { return Stat.Speed; }
        set { Stat.Speed = value; }
    }

    public virtual int Hp
    {
        get { return Stat.Hp; }
        set
        {
            Stat.Hp = value;
        }

    }

    protected bool _updated = false;

    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
            {
                return;
            }

            CellPos = new Vector3Int(value.PosX, value.PosY, 0);
            State = value.State;
            Dir = value.MoveDir;
            //_positionInfo = value;	// 이렇게 하면 안됨. 참조가 아니라 값 복사가 되어버림.
            //UpdateAnimation();		
        }
    }

    public void SyncPos()   //위치 동기화(바로 이동)
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = destPos;
    }


    public Vector3Int CellPos
    {
        get
        {
            return new Vector3Int(PosInfo.PosX, PosInfo.PosY, 0);
        }
        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y)
                return;
            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            _updated = true;
        }
    }

    protected Animator _animator;
    protected SpriteRenderer _sprite;

    public virtual CreatureState State
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;
            UpdateAnimation();
            _updated = true;
        }
    }

    public MoveDir Dir
    {
        get { return PosInfo.MoveDir; }
        set
        {
            if (PosInfo.MoveDir == value)
                return;

            PosInfo.MoveDir = value;

            UpdateAnimation();
            _updated = true;
        }
    }

    public MoveDir GetDirFromVec(Vector3Int dir)
    {
        if (dir.x > 0)
            return MoveDir.Right;
        else if (dir.x < 0)
            return MoveDir.Left;
        else if (dir.y > 0)
            return MoveDir.Up;
        else
            return MoveDir.Down;
    }

    public Vector3Int GetFrontCellPos() //앞에 있는 셀의 위치를 반환
    {
        Vector3Int cellPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                cellPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                cellPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                cellPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                cellPos += Vector3Int.right;
                break;
        }

        return cellPos;
    }

    protected virtual void UpdateAnimation()
    {
        if (_animator == null || _sprite == null)
            return;
        if (State == CreatureState.Idle)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("IDLE_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("IDLE_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("IDLE_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Moving)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Skill)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("ATTACK_BACK");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Down:
                    _animator.Play("ATTACK_FRONT");
                    _sprite.flipX = false;
                    break;
                case MoveDir.Left:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = false;
                    break;
            }
        }
        else
        {

        }
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;

        UpdateAnimation();
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
            case CreatureState.Skill:
                UpdateSkill();
                break;
            case CreatureState.Dead:
                UpdateDead();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
    }

    // 스르륵 이동하는 것을 처리
    protected virtual void UpdateMoving()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        // 도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < Speed * Time.deltaTime)
        {
            transform.position = destPos;
            MoveToNextPos();
        }
        else
        {
            transform.position += moveDir.normalized * Speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
    }

    protected virtual void MoveToNextPos()
    {
        //내 플레이어가 아니면 움직이지 않는다.
    }

    protected virtual void UpdateSkill()
    {

    }

    protected virtual void UpdateDead()
    {

    }
}
