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

        List<Player> _players = new List<Player>();

        public void EnterRoom(Player newPlayer)
        {
            if(newPlayer == null)
                return;
            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;
                Console.WriteLine($"입장: {newPlayer.Info.PlayerId}");
                // 본인한테 정보 전송
                { 
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    // 본인한테 다른 플레이어 정보 전송
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players)
                    {
                        if(p != newPlayer)  //위에서 한번 전송했으니까
                            spawnPacket.Players.Add(p.Info);
                    }    
                    newPlayer.Session.Send(spawnPacket);
                }
                // 다른 플레이어들에게 정보 전송
                { 
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player p in _players)  
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
                Player player = _players.Find(p => p.Info.PlayerId == playerId);
                if (player == null)
                    return;
                
                _players.Remove(player);
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
                // 다른 플레이어들에게 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.Info.PlayerId);
                    foreach (Player p in _players)
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
                // 서버에서 좌표이동
                PlayerInfo playerInfo = player.Info; // 플레이어 정보
                playerInfo.PosInfo = movePacket.PosInfo; // 좌표 이동

                // 방에 있는 모든 플레이어에게 전송
                S_Move resMove = new S_Move();
                resMove.PlayerId = player.Info.PlayerId;
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
                PlayerInfo playerInfo = player.Info; // 플레이어 정보
                if (playerInfo.PosInfo.State != CreatureState.Idle)  //이동중이면 스킬 사용 불가
                    return;

                //TODO : 스킬 사용 가능 여부 검증

                //통과
                playerInfo.PosInfo.State = CreatureState.Skill;

                // 방에 있는 모든 플레이어에게 전송
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.PlayerId = playerInfo.PlayerId;
                skill.Info.SkillId = 1;
                Broadcast(skill);

                //TODO : 데미지 판정
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}
