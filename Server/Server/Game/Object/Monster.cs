using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public Monster()
        {
            ObjectType = GameObjectType.Monster;

            Stat.Level = 1;
            Stat.Hp = 100;
            Stat.MaxHp = 100;
            Stat.Speed = 3.0f;

            State = CreatureState.Idle;
        }

        //FSM (Finite State Machine)
        public override void Update()
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

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;

        long _nextSearchTime = 0;
        protected virtual void UpdateIdle()
        {
            if(_nextSearchTime > Environment.TickCount64)
                return;
            _nextSearchTime = Environment.TickCount64 + 1000;

            //주변에 플레이어가 있는지 체크
            Player target = Room.FindPlayer(p =>
            {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist;
            });

            if(target == null)
                return;
            
            _target = target;
            State = CreatureState.Moving;
        }

        long _nextMoveTime = 0;
        protected virtual void UpdateMoving()
        {
            if(_nextMoveTime > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTime = Environment.TickCount64 + moveTick;

            if(_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            int dist = (_target.CellPos - CellPos).cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: false);
            if (path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                return;
            }

            //이동
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]); 
            
            //다른 플레이어에게 알림
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
        }

        protected virtual void UpdateSkill()
        {
            //TODO : 스킬 시전
        }

        protected virtual void UpdateDead()
        {
            //TODO : 죽음
        }
    }
}