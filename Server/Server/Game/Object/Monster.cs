using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.DB;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; private set; }

        public Monster()
        {
            ObjectType = GameObjectType.Monster;
        }

        public void Init(int templateId)
        {
            TemplateId = templateId;

            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(templateId, out monsterData);
            Stat.MergeFrom(monsterData.stat);
            Stat.Hp = Stat.MaxHp;
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

        int _skillRange = 1;
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
                BroadcastMove();
                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: false);
            if (path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            //스킬로 넘어갈지 체크
            if(dist <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                State = CreatureState.Skill;
                _coolTime = 0;
                //BroadcastMove();
                return;
            }

            //이동
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]); 
            
            //다른 플레이어에게 알림
            BroadcastMove();
        }

        void BroadcastMove()
        {
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
        }

        long _coolTime = 0;
        protected virtual void UpdateSkill()
        {
            if(_coolTime == 0)
            {
                //유효한 타겟인지 체크
                if(_target == null || _target.Room != Room || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                //스킬이 아직 사용 가능한지
                Vector2Int dir = _target.CellPos - CellPos;
                int dist = dir.cellDistFromZero;
                bool canUseSkill = dist <= _skillRange && (dir.x == 0 || dir.y == 0);
                if(canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                //타겟팅 방향 주시
                MoveDir lookDir = GetDirFromVec(dir);
                if(Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadcastMove();
                }
                Skill skillData = null;
                DataManager.SkillDict.TryGetValue(1, out skillData); //1번 스킬 데이터를 가져온다.

                //데미지 판정
                _target.OnDamaged(this, skillData.damage + Stat.STR);

                //스킬사용 broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                Room.Broadcast(skill);

                //스킬 쿨타임 적용
                int coolTime = (int)(1000*skillData.cooldown);
                _coolTime = Environment.TickCount64 + coolTime;
            }

            if(_coolTime > Environment.TickCount64)
                return;

            _coolTime = 0;
        }

        protected virtual void UpdateDead()
        {
            //TODO : 죽음
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);

            // 경험치, 아이템 드랍
            GameObject owner = attacker.GetOwner();
            if(owner != null && owner.ObjectType == GameObjectType.Player)
            {
                RewardData reward = GetRandomReward();
                if(reward != null)
                {
                    Player player = (Player)owner;
                    DbTransaction.RewardPlayer(player, reward, Room);
                }
            }
        }

        RewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

            int rand = new Random().Next(0, 101); //100분위로 하기로 했다.
            int total = 0;
            foreach (var reward in monsterData.rewards)
            {
                total += reward.probability;
                if (rand <= total)
                    return reward;
            }
            return null;
        }
    }
}
