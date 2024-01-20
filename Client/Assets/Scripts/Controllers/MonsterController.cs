using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coPatrol;
    Coroutine _coSearch;
    Coroutine _coSkill;

    [SerializeField]
    Vector3Int _destCellPos;

    [SerializeField]
    GameObject _target;
    [SerializeField]
    float _searchRange = 10.0f;
    [SerializeField]
    float _skillRange = 1.0f;
    [SerializeField]
    bool _rangedSkill = false;

    //start, update는 CreatureController에 있으므로 실행이 된다.
    public override CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;
            
            base.State = value;

            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }
            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void Init()
    {
        base.Init();
        State = CreatureState.Idle;
        Dir = MoveDir.None;

        _speed = 3.0f;
        _rangedSkill = Random.Range(0, 2) == 0 ? true : false;
        if(_rangedSkill)
            _skillRange = 3.0f;
        else
            _skillRange = 1.0f;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if(_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }
        if (_coSearch == null)
        {
            _coSearch = StartCoroutine("CoSearch");
        }
    }


    protected override void MoveToNexPos()
    {
        Vector3Int destPos = _destCellPos;  
        if(_target != null) 
        {
            destPos = _target.GetComponent<CreatureController>().CellPos;  //플레이어의 위치를 destPos로

            Vector3Int dir = destPos - CellPos;
            if(dir.magnitude <= _skillRange && (dir.x == 0 || dir.y == 0))  //거리가 _skillRange보다 작고 x,y중 하나가 0이면 => 일직선이면
            {
                Dir = GetDirFromVec(dir);//방향을 바꾸고

                State = CreatureState.Skill;
                if (_rangedSkill)
                    _coSkill = StartCoroutine("CoStartShootArrow");
                else
                    _coSkill = StartCoroutine("CoStartPunch");
                return;
            }
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);
        if (path.Count < 2 || (_target != null && path.Count > 20))  //경로가 없거나 플레이어가 있고 경로가 20이상이면
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];   //다음 위치//path[0]은 현재 위치//path[1]은 다음 위치//

        Vector3Int moveCellDir = nextPos - CellPos;
        Dir = GetDirFromVec(moveCellDir);
        
        if (Managers.Map.CanGo(nextPos) && Managers.Object.Find(nextPos) == null)
        {
            CellPos = nextPos;
        }
        else
        {
            State = CreatureState.Idle;
        }
    }

    IEnumerator CoPatrol()
    {
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for(int i = 0; i < 10; i++) 
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange, 0);

            if(Managers.Map.CanGo(randPos) && Managers.Object.Find(randPos) == null)
            {
                _destCellPos = randPos;
                State = CreatureState.Moving;
                yield break; //아예 끝내는 것
            }
        }

        State = CreatureState.Idle;
    }

    
    IEnumerator CoSearch()  //플레이어를 찾는 코루틴
    {
        while(true)
        {
            yield return new WaitForSeconds(1.0f);

            if (_target != null)
                continue;

            _target = Managers.Object.Find((go) => 
            {
                //플레이어가 아니면 null을 반환하므로 null이 아니면 플레이어임
                PlayerController pc = go.GetComponent<PlayerController>();
                if (pc == null)
                    return false;

                Vector3Int dir = pc.CellPos - CellPos;  //플레이어와 몬스터의 방향
                if (dir.magnitude > _searchRange) //거리가 _searchRange보다 크면
                    return false;
                
                return true;
            });
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
        State = CreatureState.Moving;
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
        State = CreatureState.Moving;
        _coSkill = null;
    }

}
