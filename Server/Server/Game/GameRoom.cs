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
    }
}
