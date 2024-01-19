using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coPatrol;
    Coroutine _coSearch;
    [SerializeField]
    Vector3Int _destCellPos;

    [SerializeField]
    GameObject _target;
    [SerializeField]
    float _searchRange = 5.0f;
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
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision: true);
        if (path.Count < 2 || (_target != null && path.Count > 10))  //경로가 없거나 플레이어가 있고 경로가 10이상이면
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];   //다음 위치//path[0]은 현재 위치//path[1]은 다음 위치//

        Vector3Int moveCellDir = nextPos - CellPos;
        if (moveCellDir.x > 0)
            Dir = MoveDir.Right;
        else if (moveCellDir.x < 0)
            Dir = MoveDir.Left;
        else if (moveCellDir.y > 0)
            Dir = MoveDir.Up;
        else if (moveCellDir.y < 0)
            Dir = MoveDir.Down;
        else
            Dir = MoveDir.None;

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

}
