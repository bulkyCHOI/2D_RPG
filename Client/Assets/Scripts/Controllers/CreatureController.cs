using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    public float _speed = 5.0f;

    public Vector3Int CellPos { get; set; } = Vector3Int.zero;
    protected Animator _animator;
    protected SpriteRenderer _sprite;

    protected CreatureState _state = CreatureState.Idle;
    public CreatureState State 
    {  
        get { return _state; } 
        set
        {
            if (_state == value)
                return;
            _state = value;
            UpdateAnimation();
        }
    }

    protected MoveDir _lastDir = MoveDir.None;    //idle 상태의 방향을 결정하기 위해
    protected MoveDir _dir = MoveDir.None;
    public MoveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
                return;
            
            _dir = value;
            if(value!= MoveDir.None)
                _lastDir = value;
            UpdateAnimation();
        }
    }

    public Vector3Int GetFrontCellPos()
    {
        Vector3Int cellPos = CellPos;

        switch (_lastDir)
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
        else if( _state == CreatureState.Moving)
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
        else if(_state == CreatureState.Skill)
        {
            switch (_lastDir) //키보드를 떼도 마지막 방향으로 공격
            {
                case MoveDir.Up:
                    _animator.Play("ATTACK_BACK");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Down:
                    _animator.Play("ATTACK_FRONT");
                    _sprite.flipX = false; //원래상태
                    break;
                case MoveDir.Left:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = true; //좌우반전
                    break;
                case MoveDir.Right:
                    _animator.Play("ATTACK_RIGHT");
                    _sprite.flipX = false; //원래상태
                    break;
            }
        }
        else
        {

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
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
    }

    protected virtual void UpdateController()
    {
        switch (State)  //각 State별로 먼저 분기를 쳐서 어떤것들을 실행할지 계위를 올려준다.
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

    //이동가능한 상태일때, 실제 좌표를 이동시켜줌
    protected virtual void UpdateIdle()
    {
        if (_dir != MoveDir.None)
        {
            Vector3Int destPos = CellPos;
            switch (_dir)
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

            State = CreatureState.Moving;   //막혀도 애니메이션은 재생되어야 하니까
            if (Managers.Map.CanGo(destPos))
            {
                if (Managers.Object.Find(destPos) == null)
                {
                    CellPos = destPos;
                }
            }
        }
    }
    //스르륵 움직이게 해주는 부분
    protected virtual void UpdateMoving()
    {
        Vector3 destPos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 moveDir = destPos - transform.position;

        //도착 여부 체크
        float dist = moveDir.magnitude;
        if (dist < _speed * Time.deltaTime) //남은 이동 벡터의 크기가 한틱보다 작으면 도착했다고 봄
        {
            transform.position = destPos;
            //State = CreatureState.Idle; //이렇게 두면 계속 Idle로 가면서 끊겨보임
            //예외적으로 애니메이션을 직접 컨트롤
            _state = CreatureState.Idle;
            if (_dir == MoveDir.None)
                UpdateAnimation();
        }
        else //아직 도착전이니 이동시키자
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
    }

    protected virtual void UpdateSkill()
    {

    }
    protected virtual void UpdateDead()
    {

    }
}
