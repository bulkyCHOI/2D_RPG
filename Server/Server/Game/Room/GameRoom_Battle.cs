using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Data;
using ServerCore;
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

            int mapId = Map.GetMapId(player.CellPos);
            //Console.WriteLine($"Player Position: ({player.CellPos.x}, {player.CellPos.y})");
            //Console.WriteLine($"MapId: {mapId}");
            if (mapId > 0)
            {
                //Console.WriteLine($"Player({player.Info.Name}) Move to Room({mapId})");
                S_Die diePacket = new S_Die();
                diePacket.ObjectId = player.Info.ObjectId;
                diePacket.AttackerId = player.Info.ObjectId;
                player.Room.Broadcast(player.CellPos, diePacket);

                GameRoom room = player.Room;   //Room에서 나가기 전에 Room을 저장해놓는다.
                room.LeaveGame(player.Info.ObjectId); //push로 하지 않아도 된다. 이 함수는 바로 처리된다.

                GameRoom newRoom = GameLogic.Instance.Find(mapId);  //2번방으로 강제 셋팅
                newRoom.EnterGame(player, randPos: true);   //다시 입장   //push로 하지 않아도 된다. 이 함수는 바로 처리된다.

                //MoveScene 패킷을 보내자
                //Console.WriteLine($"{mapId} 방으로 이동");
                S_MoveMap moveMap = new S_MoveMap();
                moveMap.MapNumber = mapId;
                player.Session.Send(moveMap);
            }


            // 방에 있는 모든 플레이어에게 전송
            S_Move resMove = new S_Move();
            resMove.ObjectId = player.Info.ObjectId;
            resMove.PosInfo = movePacket.PosInfo;

            Broadcast(player.CellPos, resMove);
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
            Broadcast(player.CellPos, skill);

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
                        //Console.WriteLine("Hit GameObject !");
                        target.OnDamaged(player, player.TotalMeleeAttack);
                    }
                    else
                        return;
                    break;
                case SkillType.SkillProjectile: //발사체 스킬
                    Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                    if (arrow != null && player.Stat.Mp >= 10)
                    {
                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = playerInfo.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = playerInfo.PosInfo.PosX;
                        arrow.PosInfo.PosY = playerInfo.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.speed;
                        //EnterGame(arrow);//job 방식으로 변경
                        Push(EnterGame, arrow, false);//job 방식으로 변경 //randPos = false

                        //플레이어 마나 감소
                        player.Stat.Mp -= 10;
                        S_ChangeMp changeMpPacket = new S_ChangeMp();
                        changeMpPacket.ObjectId = playerInfo.ObjectId;
                        changeMpPacket.Mp = player.Stat.Mp;
                        Broadcast(player.CellPos, changeMpPacket);
                    }
                    else
                        return;
                    break;
            }
        }
    }
}
