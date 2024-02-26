using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;
            
            //검증
            PositionInfo movePosInfo = movePacket.PosInfo; //가고싶은 좌표
            ObjectInfo info = player.Info; // 플레이어 정보
                
            //다른좌표로 이동할 경우, 갈수 있는지 체크
            if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                Vector2Int dest = new Vector2Int(movePosInfo.PosX, movePosInfo.PosY);
                if (Map.CanGo(dest) == false)
                    return;
            }
            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)); //이동


            // 방에 있는 모든 플레이어에게 전송
            S_Move resMove = new S_Move();
            resMove.ObjectId = player.Info.ObjectId;
            resMove.PosInfo = movePacket.PosInfo;

            Broadcast(resMove);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;
            
            ObjectInfo playerInfo = player.Info; // 플레이어 정보
            if (playerInfo.PosInfo.State != CreatureState.Idle)  //이동중이면 스킬 사용 불가
                return;

            //TODO : 스킬 사용 가능 여부 검증

            playerInfo.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = playerInfo.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            Broadcast(skill);

            Data.Skill skillData = null;
            if(DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;
            switch(skillData.skillType)
            {
                case SkillType.SkillNone:
                case SkillType.SkillAuto:   //자동 스킬
                    Vector2Int skillPos = player.GetFrontCellPos(playerInfo.PosInfo.MoveDir);
                    GameObject target = Map.Find(skillPos);
                    if (target != null)
                    {
                        Console.WriteLine("Hit GameObject !");
                        target.OnDamaged(player, player.TotalAttack);
                    }
                    else
                        return;
                    break;
                case SkillType.SkillProjectile: //발사체 스킬
                    Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                    if (arrow != null)
                    {
                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = playerInfo.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = playerInfo.PosInfo.PosX;
                        arrow.PosInfo.PosY = playerInfo.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.speed;
                        //EnterGame(arrow);//job 방식으로 변경
                        Push(EnterGame, arrow);//job 방식으로 변경
                    }
                    else
                        return;
                    break;
            }
        }
    }
}
