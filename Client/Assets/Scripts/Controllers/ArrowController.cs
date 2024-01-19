using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
        switch (_lastDir)
        { 
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0,0,0);
                break;
            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
        base.Init();

    }

    protected override void UpdateAnimation()
    {

    }

    protected override void UpdateIdle()
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
            if (Managers.Map.CanGo(destPos))    //갈수있는지 검사
            {
                GameObject go = Managers.Object.Find(destPos);
                if (go == null) //아무것도 없다면
                {
                    CellPos = destPos;  //이동
                }
                else    //object가 있다면 피격됬다고 생각하고
                {
                    CreatureController cc = go.GetComponent<CreatureController>();
                    if(cc!=null)
                        cc.OnDamaged();
                    Managers.Resource.Destroy(gameObject);  //소멸: 근데왜 이렇게 파괴하지? >> Managers에서 통합적으로 관리
                }

            }
            else //뭔가에 막히면(벽, 돌, 등)
            {
                Managers.Resource.Destroy(gameObject);  //소멸: 근데왜 이렇게 파괴하지?
            }
        }
    }
}
