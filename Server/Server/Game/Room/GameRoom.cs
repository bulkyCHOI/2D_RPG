using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        Map _map = new Map();
        public void Init(int mapId)
        {
            _map.LoadMap(mapId);
        }

        public void EnterRoom(Player newPlayer)
        {
            if(newPlayer == null)
                return;
            lock (_lock)
            {
                _players.Add(newPlayer.Info.ObjectId, newPlayer);
                newPlayer.Room = this;
                Console.WriteLine($"입장: {newPlayer.Info.ObjectId}");
                // 본인한테 정보 전송
                { 
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    // 본인한테 다른 플레이어 정보 전송
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if(p != newPlayer)  //위에서 한번 전송했으니까
                            spawnPacket.Objects.Add(p.Info);
                    }    
                    newPlayer.Session.Send(spawnPacket);
                }
                // 다른 플레이어들에게 정보 전송
                { 
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(newPlayer.Info);
                    foreach (Player p in _players.Values)  
                    {
                        if (p != newPlayer) // 본인한테는 이미 전송했으니까
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }
        public void LeaveRoom(int playerId)
        {
            lock (_lock)
            {
                Player player = null;
                if(_players.TryGetValue(playerId, out player) == false)
                    return; 
                
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
                // 다른 플레이어들에게 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.Info.ObjectId);
                    foreach (Player p in _players.Values)
                    {
                        if(player == p) // 본인한테는 이미 전송했으니까
                            continue;
                        p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;
            lock (_lock)
            {
                //TODO : 검증
                PositionInfo movePosInfo = movePacket.PosInfo; //가고싶은 좌표
                ObjectInfo info = player.Info; // 플레이어 정보
                
                //다른좌표로 이동할 경우, 갈수 있는지 체크
                if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    Vector2Int dest = new Vector2Int(movePosInfo.PosX, movePosInfo.PosY);
                    if (_map.CanGo(dest) == false)
                        return;
                }
                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                _map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)); //이동


                // 방에 있는 모든 플레이어에게 전송
                S_Move resMove = new S_Move();
                resMove.PlayerId = player.Info.ObjectId;
                resMove.PosInfo = movePacket.PosInfo;

                Broadcast(resMove);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;
            lock (_lock)
            {
                ObjectInfo playerInfo = player.Info; // 플레이어 정보
                if (playerInfo.PosInfo.State != CreatureState.Idle)  //이동중이면 스킬 사용 불가
                    return;

                //TODO : 스킬 사용 가능 여부 검증

                playerInfo.PosInfo.State = CreatureState.Skill;
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.PlayerId = playerInfo.ObjectId;
                skill.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(skill);

                if(skillPacket.Info.SkillId == 1) //평타
                {
                    //TODO : 데미지 판정
                    Vector2Int skillPos = player.GetFrontCellPos(playerInfo.PosInfo.MoveDir);
                    Player target = _map.Find(skillPos);
                    if(target != null)
                    {
                        Console.WriteLine("Hit Player !");
                    }
                }
                else if(skillPacket.Info.SkillId == 2) //스킬
                {
                    
                }
                

            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}
